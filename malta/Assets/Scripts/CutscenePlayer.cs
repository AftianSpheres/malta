using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using MovementEffects;

internal class CutsceneOp
{
    /// <summary>
    /// Nasty hack to create something that will automatically return as a finished coroutine, simplifying control flow by letting WaitUntilDone handle EVTCall ops.
    /// </summary>
    /// <returns></returns>
    static IEnumerator<float> dummyCoroutine()
    {
        yield break;
    }

    Action<object, object> EVTCall; // immediately-executable things
    Func<object, object, IEnumerator<float>> FXCall; // coroutines
    object arg0;
    object arg1;
    public float endTime { get; private set; }
    public bool killWhenDone { get; private set; }
    public bool isFX { get { return FXCall != null; } }
    public IEnumerator<float> handle { get; private set; }
    string uniqueTag;
    public bool isDone { get; private set; }


    public CutsceneOp(Action<object, object> evt, object _arg0 = null, object _arg1 = null)
    {
        isDone = false;
        handle = null;
        uniqueTag = evt.Method.ToString();
        killWhenDone = false;
        endTime = 0;
        FXCall = null;
        EVTCall = evt;
        arg0 = _arg0;
        arg1 = _arg1;
    }

    public CutsceneOp(Func<object, object, IEnumerator<float>> fx, object _arg0 = null, object _arg1 = null)
    {
        isDone = false;
        handle = null;
        uniqueTag = fx.Method.ToString();
        killWhenDone = false;
        endTime = 0;
        FXCall = fx;
        EVTCall = null;
        arg0 = _arg0;
        arg1 = _arg1;
    }

    /// <summary>
    /// Pass 0 if we move on immediately.
    /// Pass float.MinValue if we continue until done.
    /// </summary>
    /// <param name="_endTime"></param>
    public void ConfigEndTime(float _endTime, bool _killWhenDone)
    {
        endTime = _endTime;
        killWhenDone = _killWhenDone;
    }

    public IEnumerator<float> Run(string tag)
    {
        IEnumerator<float> r;
        uniqueTag = tag + uniqueTag;
        if (EVTCall == null) r = Timing.RunCoroutine(FXCall(arg0, arg1), uniqueTag);
        else
        {
            EVTCall(arg0, arg1);
            r = Timing.RunCoroutine(dummyCoroutine(), uniqueTag);
        }
        handle = r;
        Timing.RunCoroutine(SelfMonitor(), uniqueTag);
        return r;
    }

    public void Stop()
    {
        Timing.KillCoroutines(uniqueTag);
    }

    private IEnumerator<float> SelfMonitor()
    {
        if (endTime == float.MinValue) yield return Timing.WaitUntilDone(handle);
        else yield return Timing.WaitForSeconds(endTime);
        isDone = true;
    }
}

public class CutscenePlayer : MonoBehaviour
{
    public GameObject cutsceneDisplay;
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public Image backgroundImage;
    public Image foregroundImage;
    public Text textbox;
    public string[] cutsceneNames;
    public string myTag = "cutscenePlayer";
    private AudioClip lastBGM;
    private Vector3 bgImgPos;
    private Vector3 fgImgPos;
    private CutsceneOp[] cutsceneOps;
    private CutsceneOp[][] cutsceneOpSets;
    private string[] strings;
    private string[][] cutsceneStringArrays;
    private bool running;
    private int opIndex;
    private const string gfxPath = "CutsceneGFX/";
    private const string musicPath = "Audio/Music/";
    private const string mfxPath = "Audio/MFX/";
    private const string sfxPath = "Audio/SFX/";
    private const string cutscenesActionsPath = "Cutscenes/action_";
    private const string cutscenesTextPath = "Cutscenes/text_";

    void Awake()
    {
        bgImgPos = backgroundImage.transform.position;
        fgImgPos = foregroundImage.transform.position;
        textbox.text = "";
        foregroundImage.sprite = null;
        backgroundImage.sprite = null;
        cutsceneOpSets = new CutsceneOp[cutsceneNames.Length][];
        cutsceneStringArrays = new string[cutsceneNames.Length][];
        for (int i = 0; i < cutsceneNames.Length; i++) LoadCutscene(i);
    }

    // Update is called once per frame
    void Update()
    {
        if (running)
        {
            if (opIndex + 1 < cutsceneOps.Length && CheckIfCurrentOpDone()) NextOp();
        }
    }

    private bool CheckIfCurrentOpDone()
    {
        CutsceneOp op = cutsceneOps[opIndex];
        bool result = false;
        if (!op.isFX || op.isDone) result = true; // never _not_ done if it's not an FX op
        return result;
    }

    private void NextOp()
    {
        if (cutsceneOps[opIndex].killWhenDone) cutsceneOps[opIndex].Stop();
        opIndex++;
        CutsceneOp op = cutsceneOps[opIndex];
        op.Run(myTag + opIndex.ToString());
    }

    public void StartCutscene(int index)
    {
        cutsceneDisplay.SetActive(true);
        GameStateManager.Instance.GiveFocusToCutscene();
        lastBGM = bgmSource.clip;
        opIndex = 0;
        strings = cutsceneStringArrays[index];
        cutsceneOps = cutsceneOpSets[index];
        cutsceneOps[0].Run(myTag + 0);
        running = true;
    }

    private void LoadCutscene(int index)
    {
        string cutsceneName = cutsceneNames[index];
        TextAsset script = Resources.Load<TextAsset>(cutscenesActionsPath + cutsceneName);
        TextAsset stringsResource = Resources.Load<TextAsset>(cutscenesTextPath + cutsceneName);
        cutsceneStringArrays[index] = stringsResource.text.Split('\n');
        strings = cutsceneStringArrays[index];
        cutsceneOpSets[index] = ParseCutsceneScript(script);
    }

    private CutsceneOp[] ParseCutsceneScript(TextAsset _script)
    {
        List<CutsceneOp> ops = new List<CutsceneOp>();
        string[] scriptLines = _script.text.Split('\n');
        bool nextLineIsEndFX = false;
        CutsceneOp op = default(CutsceneOp);
        for (int i = 0; i < scriptLines.Length; i++)
        {
            string[] terms = scriptLines[i].Split(new char[] { ' ' }, 4);
            if (terms[0] == "skip" || terms.Length < 3) continue;
            else if (!nextLineIsEndFX)
            {
                op = GetOp(terms[0], terms[1], terms[2]);
                if (op.isFX) nextLineIsEndFX = true;
                ops.Add(op);
            }
            else if (i > 0)
            {
                if (terms[0] != "endFX") throw new Exception("Cutscene " + _script.name + " contains an FX statement with no following endFX statement!");
                else
                {
                    float f;
                    bool b;
                    if (terms[1] == "waitSeconds")
                    {
                        f = float.Parse(terms[2]);
                        b = f >= 0;
                        if (f < 0) f = Mathf.Abs(f);
                    }
                    else if (terms[1] == "waitUntilDone")
                    {
                        f = float.MinValue;
                        b = bool.Parse(terms[2]);
                    }
                    else throw new Exception("Malformed endFX statement on line " + i.ToString() + " of cutscene " + _script.name);
                    op.ConfigEndTime(f, b);
                    nextLineIsEndFX = false;
                }
            }
            else throw new Exception("The cutscene parser is seriously fucked, and it's looking for endFX on line 0.");
        }
        return ops.ToArray();
    }

    private CutsceneOp GetOp(string opRef, string arg0Ref, string arg1Ref)
    {
        CutsceneOp op;
        Action<object, object> EVTCall = null; // immediately-executable things
        Func<object, object, IEnumerator<float>> FXCall = null; // coroutines
        object arg0 = null;
        object arg1 = null;
        switch (opRef)
        {
            case "EVT_PlayMFX":
                EVTCall = cutsceneEVT_PlayMFX;
                arg0 = Resources.Load<AudioClip>(mfxPath + arg0Ref);
                break;
            case "EVT_PlaySFX":
                EVTCall = cutsceneEVT_PlaySFX;
                arg0 = Resources.Load<AudioClip>(sfxPath + arg0Ref);
                break;
            case "EVT_SetBGM":
                EVTCall = cutsceneEVT_SetBGM;
                arg0 = Resources.Load<AudioClip>(musicPath + arg0Ref);
                break;
            case "EVT_SetBackground":
                EVTCall = cutsceneEVT_SetBackground;
                arg0 = Resources.Load<Sprite>(gfxPath + arg0Ref);
                break;
            case "EVT_SetForeground":
                EVTCall = cutsceneEVT_SetForeground;
                arg0 = Resources.Load<Sprite>(gfxPath + arg0Ref);
                break;
            case "EVT_RefreshTextboxWith":
                EVTCall = cutsceneEVT_RefreshTextboxWith;
                arg0 = strings[int.Parse(arg0Ref)];
                break;
            case "EVT_End":
                EVTCall = cutsceneEVT_End;
                break;
            case "EVT_LoadLevel":
                EVTCall = cutsceneEVT_LoadLevel;
                arg0 = TermToSceneID(arg0Ref);
                break;
            case "FX_Wait":
                FXCall = cutsceneFX_Wait;
                arg0 = float.Parse(arg0Ref);
                break;
            case "FX_ScrollBackground":
                FXCall = cutsceneFX_ScrollBackground;
                arg0 = TermToDirection(arg0Ref);
                arg1 = float.Parse(arg1Ref);
                break;
            case "FX_ScrollForeground":
                FXCall = cutsceneFX_ScrollForeground;
                arg0 = TermToDirection(arg0Ref);
                arg1 = float.Parse(arg1Ref);
                break;
            case "FX_FadeOutBackground":
                FXCall = cutsceneFX_FadeOutBackground;
                arg0 = float.Parse(arg0Ref);
                break;
            case "FX_FadeOutForeground":
                FXCall = cutsceneFX_FadeOutForeground;
                arg0 = float.Parse(arg0Ref);
                break;
            case "FX_WaitForClick":
                FXCall = cutsceneFX_WaitForClick;
                break;
            default:
                throw new Exception("Bad op: " + opRef);
        }
        if (EVTCall != null) op = new CutsceneOp(EVTCall, arg0, arg1);
        else if (FXCall != null) op = new CutsceneOp(FXCall, arg0, arg1);
        else throw new Exception("Tried to parse invalid cutscene op: " + opRef);
        return op;
    }

    private SceneIDType TermToSceneID(string term)
    {
        SceneIDType scene;
        switch (term)
        {
            case "Title":
                scene = SceneIDType.TitleScene;
                break;
            case "Town":
                scene = SceneIDType.TownScene;
                break;
            case "Overworld":
                scene = SceneIDType.OverworldScene;
                break;
            case "Fight":
                scene = SceneIDType.FightScene;
                break;
            default:
                throw new Exception(term + " is not a valid scene ID.");
        }
        return scene;
    }

    private Direction TermToDirection(string term)
    {
        Direction dir;
        switch (term) // blehhhh
        {
            case "Down":
                dir = Direction.Down;
                break;
            case "DownLeft":
                dir = Direction.DownLeft;
                break;
            case "DownRight":
                dir = Direction.DownRight;
                break;
            case "Up":
                dir = Direction.Up;
                break;
            case "UpLeft":
                dir = Direction.UpLeft;
                break;
            case "UpRight":
                dir = Direction.UpRight;
                break;
            case "Left":
                dir = Direction.Left;
                break;
            case "Right":
                dir = Direction.Right;
                break;
            default:
                throw new Exception(term + " is not a valid Direction.");
        }
        return dir;
    }

    void cutsceneEVT_PlayMFX(object mfxClip, object notUsed1)
    {
        bgmSource.PlayOneShot((AudioClip)mfxClip);
    }

    void cutsceneEVT_PlaySFX(object sfxClip, object notUsed1)
    {
        sfxSource.PlayOneShot((AudioClip)sfxClip);
    }

    void cutsceneEVT_SetBGM(object bgmClip, object notUsed1)
    {
        bgmSource.clip = (AudioClip)bgmClip;
        bgmSource.Play();
        bgmSource.loop = true;
    }

    void cutsceneEVT_SetBackground(object sprite, object color = null)
    {
        backgroundImage.sprite = (Sprite)sprite;
        if (color != null) backgroundImage.color = (Color)color;
        else backgroundImage.color = Color.white;
        backgroundImage.transform.position = bgImgPos;
    }

    void cutsceneEVT_SetForeground(object sprite, object color = null)
    {
        foregroundImage.sprite = (Sprite)sprite;
        if (color != null) foregroundImage.color = (Color)color;
        else foregroundImage.color = Color.white;
        foregroundImage.transform.position = fgImgPos;
    }

    void cutsceneEVT_RefreshTextboxWith(object textString, object notUsed1)
    {
        textbox.text = (string)textString;
    }

    void cutsceneEVT_End(object notUsed0, object notUsed1)
    {
        GameStateManager.Instance.CutsceneHasCededFocus();
        bgmSource.clip = lastBGM;
        bgmSource.Play();
        bgmSource.loop = true;
        running = false;
        cutsceneDisplay.SetActive(false);
    }

    void cutsceneEVT_LoadLevel(object scene, object notUsed1)
    {
        GameStateManager.Instance.CutsceneHasCededFocus();
        LevelLoadManager.Instance.EnterLevel((SceneIDType)scene);
    }

    IEnumerator<float> cutsceneFX_Wait(object length, object notUsed1)
    {
        yield return Timing.WaitForSeconds((float)length);
    }

    IEnumerator<float> cutsceneFX_ScrollBackground(object direction, object dist_time_array) // this kinda sucks - dist_time_array needs to be a two-element float[] where element 0 = total distance and element 1 = total scroll time. eww. but it has to be object for the generic func...
    {
        Vector3 v = Vector3.zero;
        float distance = ((float[])dist_time_array)[0];
        float time = ((float[])dist_time_array)[1];
        switch ((Direction)direction)
        {
            case Direction.Down:
                v = Vector3.down;
                break;
            case Direction.DownLeft:
                v = (Vector3.down + Vector3.left) / 2;
                break;
            case Direction.DownRight:
                v = (Vector3.down + Vector3.right) / 2;
                break;
            case Direction.Up:
                v = Vector3.up;
                break;
            case Direction.UpLeft:
                v = (Vector3.up + Vector3.left) / 2;
                break;
            case Direction.UpRight:
                v = (Vector3.up + Vector3.right) / 2;
                break;
            case Direction.Left:
                v = Vector3.left;
                break;
            case Direction.Right:
                v = Vector3.right;
                break;
        }
        v *= distance;
        v += bgImgPos;
        float timer = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            backgroundImage.transform.position = Vector3.Lerp(bgImgPos, v, time / timer);
            yield return 0f;
        }
    }

    IEnumerator<float> cutsceneFX_ScrollForeground(object direction, object dist_time_array) // this kinda sucks - dist_time_array needs to be a two-element float[] where element 0 = total distance and element 1 = total scroll time. eww. but it has to be object for the generic func...
    {
        Vector3 v = Vector3.zero;
        float distance = ((float[])dist_time_array)[0];
        float time = ((float[])dist_time_array)[1];
        switch ((Direction)direction)
        {
            case Direction.Down:
                v = Vector3.down;
                break;
            case Direction.DownLeft:
                v = (Vector3.down + Vector3.left) / 2;
                break;
            case Direction.DownRight:
                v = (Vector3.down + Vector3.right) / 2;
                break;
            case Direction.Up:
                v = Vector3.up;
                break;
            case Direction.UpLeft:
                v = (Vector3.up + Vector3.left) / 2;
                break;
            case Direction.UpRight:
                v = (Vector3.up + Vector3.right) / 2;
                break;
            case Direction.Left:
                v = Vector3.left;
                break;
            case Direction.Right:
                v = Vector3.right;
                break;
        }
        v *= distance;
        v += fgImgPos;
        float timer = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            foregroundImage.transform.position = Vector3.Lerp(fgImgPos, v, time / timer);
            yield return 0f;
        }
    }

    IEnumerator<float> cutsceneFX_FadeOutBackground(object length, object notUsed1)
    {
        Color baseColor = backgroundImage.color;
        float timer = 0f;
        float l = (float)length;
        while (timer < l)
        {
            timer += Time.deltaTime;
            backgroundImage.color = Color.Lerp(baseColor, Color.clear, timer / l);
            yield return 0f;
        }
    }

    IEnumerator<float> cutsceneFX_FadeOutForeground(object length, object notUsed1)
    {
        Color baseColor = foregroundImage.color;
        float timer = 0f;
        float l = (float)length;
        while (timer < l)
        {
            timer += Time.deltaTime;
            foregroundImage.color = Color.Lerp(baseColor, Color.clear, timer / l);
            yield return 0f;
        }
    }

    IEnumerator<float> cutsceneFX_WaitForClick(object notUsed0, object notUsed1)
    {
        while (!Input.GetMouseButtonDown(0)) yield return 0f;
    }
}