using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using MovementEffects;

public enum CutscenePlayerSubtype
{
    None,
    Cutscene,
    BattleAnim
}

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

    Action<object[]> EVTCall; // immediately-executable things
    Func<object[], IEnumerator<float>> FXCall; // coroutines
    object[] args;
    public float endTime { get; private set; }
    public bool killWhenDone { get; private set; }
    public bool isFX { get { return FXCall != null; } }
    public IEnumerator<float> handle { get; private set; }
    string uniqueTag;
    public bool isDone { get; private set; }


    public CutsceneOp(Action<object[]> evt, object[] _args)
    {
        isDone = false;
        handle = null;
        uniqueTag = evt.Method.ToString();
        killWhenDone = false;
        endTime = 0;
        FXCall = null;
        EVTCall = evt;
        args = _args;
    }

    public CutsceneOp(Func<object[], IEnumerator<float>> fx, object[] _args)
    {
        isDone = false;
        handle = null;
        uniqueTag = fx.Method.ToString();
        killWhenDone = false;
        endTime = 0;
        FXCall = fx;
        EVTCall = null;
        args = _args;
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
        if (EVTCall == null) r = Timing.RunCoroutine(FXCall(args), uniqueTag);
        else
        {
            EVTCall(args);
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
    public CutscenePlayerSubtype subtype;
    public GameObject spritePoolPrototype;
    public GameObject cutsceneDisplay;
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public Image backgroundImage;
    public Image foregroundImage;
    public Text textbox;
    public string[] cutsceneNames;
    public string myTag = "cutscenePlayer";
    public bool running;
    private AudioClip lastBGM;
    private Vector3 bgImgPos;
    private Vector3 fgImgPos;
    private CutsceneOp[] cutsceneOps;
    private CutsceneOp[][] cutsceneOpSets;
    public Queue<CutsceneSlaveSprite> slaveSprites;
    public Dictionary<string, CutsceneSlaveSprite> activeSlaves;
    private string[] strings;
    private string[][] cutsceneStringArrays;
    private int opIndex;
    private const string gfxPath = "CutsceneGFX/";
    private const string musicPath = "Audio/Music/";
    private const string mfxPath = "Audio/MFX/";
    private const string sfxPath = "Audio/SFX/";
    private const string battleAnimsPath = "Cutscenes/battle/";
    private const string cutscenesActionsPath = "Cutscenes/action_";
    private const string cutscenesTextPath = "Cutscenes/text_";
    private const int slaveCount = 24;

    void Awake()
    {
        bgImgPos = backgroundImage.transform.position;
        fgImgPos = foregroundImage.transform.position;
        foregroundImage.sprite = null;
        backgroundImage.sprite = null;
        backgroundImage.enabled = false;
        foregroundImage.enabled = false;
        slaveSprites = new Queue<CutsceneSlaveSprite>(slaveCount);
        activeSlaves = new Dictionary<string, CutsceneSlaveSprite>(slaveCount);
        if (subtype == CutscenePlayerSubtype.Cutscene)
        {
            cutsceneOpSets = new CutsceneOp[cutsceneNames.Length][];
            cutsceneStringArrays = new string[cutsceneNames.Length][];
            textbox.text = "";
            for (int i = 0; i < cutsceneNames.Length; i++) LoadCutscene(i);
        }
        else if (subtype == CutscenePlayerSubtype.BattleAnim)
        {
            cutsceneOpSets = new CutsceneOp[BattlerActionData.numberOfAnims][];
            for (int i = 0; i < BattlerActionData.numberOfAnims; i++) LoadCutscene(i);
        }
        else throw new Exception("Cutscene player on " + gameObject.name + " doesn't have a valid subtype set!");
        for (int i = 0; i < slaveCount; i++)
        {
            CutsceneSlaveSprite slave = Instantiate(spritePoolPrototype).GetComponent<CutsceneSlaveSprite>();
            slave.master = this;
            slave.gameObject.name = "csSlave_" + i;
            slave.transform.SetParent(transform);
            slave.transform.localPosition = Vector3.zero;
            slave.myTag = myTag + slave.gameObject.name;
            slave.gameObject.SetActive(false);
            slaveSprites.Enqueue(slave);
        }
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
        opIndex = 0;
        if (subtype == CutscenePlayerSubtype.Cutscene)
        {
            GameStateManager.Instance.GiveFocusToCutscene();
            lastBGM = bgmSource.clip;
            strings = cutsceneStringArrays[index];
        }
        Debug.Log(index);
        cutsceneOps = cutsceneOpSets[index];
        cutsceneOps[0].Run(myTag + 0);
        running = true;
    }

    private void LoadCutscene(int index)
    {
        TextAsset script;
        TextAsset stringsResource;
        if (subtype == CutscenePlayerSubtype.Cutscene)
        {
            string cutsceneName = cutsceneNames[index];
            script = Resources.Load<TextAsset>(cutscenesActionsPath + cutsceneName);
            stringsResource = Resources.Load<TextAsset>(cutscenesTextPath + cutsceneName);
            cutsceneStringArrays[index] = stringsResource.text.Split('\n');
            strings = cutsceneStringArrays[index];
        }
        else if (subtype == CutscenePlayerSubtype.BattleAnim)
        {
            if ((BattlerActionAnim)index == BattlerActionAnim.None) return;
            script = Resources.Load<TextAsset>(battleAnimsPath + ((BattlerActionAnim)index).ToString());
        }
        else throw new Exception("Subtype was valid and got unset sometime between Awake() and a LoadCutscene() call. This is, uh, weird.");
        cutsceneOpSets[index] = ParseCutsceneScript(script);
    }

    private CutsceneOp[] ParseCutsceneScript(TextAsset _script)
    {
        List<CutsceneOp> ops = new List<CutsceneOp>();
        string[] scriptLines = _script.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        bool nextLineIsEndFX = false;
        CutsceneOp op = default(CutsceneOp);
        for (int i = 0; i < scriptLines.Length; i++)
        {
            string[] terms = scriptLines[i].Split(' ');
            if (terms[0] == "skip" || terms[0] == "Skip") continue;
            else if (!nextLineIsEndFX)
            {
                op = GetOp(terms);
                if (op.isFX) nextLineIsEndFX = true;
                ops.Add(op);
            }
            else if (i > 0)
            {
                if (terms[0] != "endFX" && terms[0] != "EndFX") throw new Exception("Cutscene " + _script.name + " contains an FX statement with no following endFX statement!");
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

    private CutsceneOp GetOp(string[] lineSplit)
    {
        CutsceneOp op;
        Action<object[]> EVTCall = null; // immediately-executable things
        Func<object[], IEnumerator<float>> FXCall = null; // coroutines
        object[] args;
        switch (lineSplit[0])
        {
            case "EVT_PlayMFX":
                EVTCall = cutsceneEVT_PlayMFX;
                args = new object[] { Resources.Load<AudioClip>(mfxPath + lineSplit[1]) };
                break;
            case "EVT_PlaySFX":
                EVTCall = cutsceneEVT_PlaySFX;
                args = new object[] { Resources.Load<AudioClip>(sfxPath + lineSplit[1]) };
                break;
            case "EVT_SetBGM":
                EVTCall = cutsceneEVT_SetBGM;
                args = new object[] { Resources.Load<AudioClip>(musicPath + lineSplit[1]) };
                break;
            case "EVT_SetBackground":
                EVTCall = cutsceneEVT_SetBackground;
                args = new object[] { Resources.Load<Sprite>(gfxPath + lineSplit[1]) };
                break;
            case "EVT_SetForeground":
                EVTCall = cutsceneEVT_SetForeground;
                args = new object[] { Resources.Load<Sprite>(gfxPath + lineSplit[1]) };
                break;
            case "EVT_RefreshTextboxWith":
                EVTCall = cutsceneEVT_RefreshTextboxWith;
                args = new object[] { strings[int.Parse(lineSplit[1])] };
                break;
            case "EVT_End":
            case "End":
            case "end":
                EVTCall = cutsceneEVT_End;
                args = new object[0];
                break;
            case "EVT_LoadLevel":
                EVTCall = cutsceneEVT_LoadLevel;
                args = new object[] { TermToSceneID(lineSplit[1]) };
                break;
            case "EVT_MakeSlave":
                EVTCall = cutsceneEVT_MakeSlave;
                args = new object[] { lineSplit[1], float.Parse(lineSplit[2]), Resources.Load<Sprite>(gfxPath + lineSplit[3]), new Vector3(float.Parse(lineSplit[4]), float.Parse(lineSplit[5]), float.Parse(lineSplit[6])) };
                break;
            case "EVT_RetireSlave":
                EVTCall = cutsceneEVT_RetireSlave;
                args = new object[] { lineSplit[1] };
                break;
            case "EVT_MoveSlave":
                EVTCall = cutsceneEVT_MoveSlave;
                args = new object[] { lineSplit[1], new Vector3(float.Parse(lineSplit[2]), float.Parse(lineSplit[3]), float.Parse(lineSplit[4])), new Vector3(float.Parse(lineSplit[5]), float.Parse(lineSplit[6]), float.Parse(lineSplit[7])), float.Parse(lineSplit[8]) };
                break;
            case "EVT_ScaleSlave":
                EVTCall = cutsceneEVT_ScaleSlave;
                args = new object[] { lineSplit[1], new Vector3(float.Parse(lineSplit[2]), float.Parse(lineSplit[3]), float.Parse(lineSplit[4])), float.Parse(lineSplit[5]) };
                break;
            case "EVT_ChangeSlaveGraphic":
                EVTCall = cutsceneEVT_ChangeSlaveGraphic;
                args = new object[] { lineSplit[1], Resources.Load<Sprite>(gfxPath + lineSplit[3]) };
                break;
            case "EVT_ResetSlavePos":
                EVTCall = cutsceneEVT_ResetSlavePos;
                args = new object[] { lineSplit[1] };
                break;
            case "EVT_SetSlavePos":
                EVTCall = cutsceneEVT_SetSlavePos;
                args = new object[] { lineSplit[1], new Vector3(float.Parse(lineSplit[2]), float.Parse(lineSplit[3]), float.Parse(lineSplit[4])) };
                break;
            case "FX_Wait":
                FXCall = cutsceneFX_Wait;
                args = new object[] { float.Parse(lineSplit[1]) };
                break;
            case "FX_ScrollBackground":
                FXCall = cutsceneFX_ScrollBackground;
                args = new object[] { TermToDirection(lineSplit[1]), float.Parse(lineSplit[2]), float.Parse(lineSplit[3]) };
                break;
            case "FX_ScrollForeground":
                FXCall = cutsceneFX_ScrollForeground;
                args = new object[] { TermToDirection(lineSplit[1]), float.Parse(lineSplit[2]), float.Parse(lineSplit[3]) };
                break;
            case "FX_FadeOutBackground":
                FXCall = cutsceneFX_FadeOutBackground;
                args = new object[] { float.Parse(lineSplit[1]) };
                break;
            case "FX_FadeOutForeground":
                FXCall = cutsceneFX_FadeOutForeground;
                args = new object[] { float.Parse(lineSplit[1]) };
                break;
            case "FX_WaitForClick":
                FXCall = cutsceneFX_WaitForClick;
                args = new object[0];
                break;
            default:
                throw new Exception("Bad op: " + lineSplit[0]);
        }
        if (EVTCall != null) op = new CutsceneOp(EVTCall, args);
        else if (FXCall != null) op = new CutsceneOp(FXCall, args);
        else throw new Exception("Tried to parse invalid cutscene op: " + lineSplit[0]);
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

    void cutsceneEVT_PlayMFX(object[] args) // AudioClip clip
    {
        bgmSource.PlayOneShot((AudioClip)args[0]);
    }

    void cutsceneEVT_PlaySFX(object[] args) // AudioClip clip
    {
        sfxSource.PlayOneShot((AudioClip)args[0]);
    }

    void cutsceneEVT_SetBGM(object[] args) // AudioClip clip
    {
        if (subtype == CutscenePlayerSubtype.BattleAnim) throw new Exception("SetBGM isn't a permitted action for battle anims");
        bgmSource.clip = (AudioClip)args[0];
        bgmSource.Play();
        bgmSource.loop = true;
    }

    void cutsceneEVT_SetBackground(object[] args) // Sprite sprite, optionally Color color
    {
        backgroundImage.sprite = (Sprite)args[0];
        backgroundImage.enabled = (backgroundImage.sprite != null);
        if (args.Length > 1) backgroundImage.color = (Color)args[1];
        else backgroundImage.color = Color.white;
        backgroundImage.transform.position = bgImgPos;
    }

    void cutsceneEVT_SetForeground(object[] args) // Sprite sprite, optionally Color color
    {
        foregroundImage.sprite = (Sprite)args[0];
        foregroundImage.enabled = (foregroundImage.sprite != null);
        if (args.Length > 1) foregroundImage.color = (Color)args[1];
        else foregroundImage.color = Color.white;
        foregroundImage.transform.position = fgImgPos;
    }

    void cutsceneEVT_RefreshTextboxWith(object[] args) // String textString
    {
        if (subtype == CutscenePlayerSubtype.BattleAnim) throw new Exception("RefreshTextboxWith isn't a permitted action for battle anims");
        textbox.text = (string)args[0];
    }

    void cutsceneEVT_End(object[] args) // No arguments
    {
        if (subtype == CutscenePlayerSubtype.Cutscene)
        {
            GameStateManager.Instance.CutsceneHasCededFocus();
            bgmSource.clip = lastBGM;
            bgmSource.Play();
            bgmSource.loop = true;
        }
        running = false;
        cutsceneDisplay.SetActive(false);
    }

    void cutsceneEVT_LoadLevel(object[] args) // SceneIDType destLevel;
    {
        if (subtype == CutscenePlayerSubtype.BattleAnim) throw new Exception("LoadLevel isn't a permitted action for battle anims");
        GameStateManager.Instance.CutsceneHasCededFocus();
        LevelLoadManager.Instance.EnterLevel((SceneIDType)args[0]);
    }

    void cutsceneEVT_MakeSlave(object[] args) // string handle, float lifespan, Sprite sprite, Vector3 pos
    {
        string handle = (string)args[0];
        if (activeSlaves.ContainsKey(handle)) throw new Exception("Tried to create a slave with handle " + handle + " but that handle is already in use!");
        slaveSprites.Dequeue().Birth(handle, (float)args[1], (Sprite)args[2], (Vector3)args[3]);
    }

    void cutsceneEVT_RetireSlave (object[] args) // string handle
    {
        string handle = (string)args[0];
        if (activeSlaves.ContainsKey(handle)) activeSlaves[handle].Kill();
        else throw new Exception("Tried to retire slave with a bad handle: " + handle);
    }

    void cutsceneEVT_MoveSlave (object[] args) // string handle, Vector3 moveVector, Vector3 speed, float time
    {
        string handle = (string)args[0];
        if (activeSlaves.ContainsKey(handle)) activeSlaves[handle].MoveOverTime((Vector3)args[1], (Vector3)args[2], (float)args[3]);
        else throw new Exception("Tried to move slave under a bad handle: " + handle);
    }

    void cutsceneEVT_ScaleSlave (object[] args) // string handle, Vector3 scaleMod, float time
    {
        string handle = (string)args[0];
        if (activeSlaves.ContainsKey(handle)) activeSlaves[handle].ScaleOverTime((Vector3)args[1], (float)args[2]);
        else throw new Exception("Tried to scale slave under a bad handle: " + handle);
    }

    void cutsceneEVT_ChangeSlaveGraphic (object[] args) // string handle, Sprite sprite, optionally Color color
    {
        string handle = (string)args[0];
        if (activeSlaves.ContainsKey(handle))
        {
            Color c;
            if (args.Length > 2) c = (Color)args[2];
            else c = Color.white;
            activeSlaves[handle].ChangeGraphic((Sprite)args[1], c); 
        }
        else throw new Exception("Tried to change slave sprite with a bad handle: " + handle);
    }

    void cutsceneEVT_ResetSlavePos (object[] args) // string handle
    {
        string handle = (string)args[0];
        if (activeSlaves.ContainsKey(handle)) activeSlaves[handle].ResetToAnchorPos();
        else throw new Exception("Tried to reset slave position with a bad handle: " + handle);
    }

    void cutsceneEVT_SetSlavePos (object[] args) // string handle, Vector3 newLocalPos
    {
        string handle = (string)args[0];
        if (activeSlaves.ContainsKey(handle)) activeSlaves[handle].MoveInstantly((Vector3)args[1]);
        else throw new Exception("Tried to set slave position with a bad handle: " + handle);
    }

    IEnumerator<float> cutsceneFX_Wait(object[] args) // float seconds
    {
        yield return Timing.WaitForSeconds((float)args[0]);
    }

    IEnumerator<float> cutsceneFX_ScrollBackground(object[] args) // Direction direction, float dist, float time
    {
        Vector3 v = Vector3.zero;
        float distance = (float)args[1];
        float time = (float)args[2];
        switch ((Direction)args[0])
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

    IEnumerator<float> cutsceneFX_ScrollForeground(object[] args) // Direction direction, float dist, float time
    {
        Vector3 v = Vector3.zero;
        float distance = (float)args[1];
        float time = (float)args[2];
        switch ((Direction)args[0])
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

    IEnumerator<float> cutsceneFX_FadeOutBackground(object[] args) // float length
    {
        Color baseColor = backgroundImage.color;
        float timer = 0f;
        float l = (float)args[0];
        while (timer < l)
        {
            timer += Time.deltaTime;
            backgroundImage.color = Color.Lerp(baseColor, Color.clear, timer / l);
            yield return 0f;
        }
    }

    IEnumerator<float> cutsceneFX_FadeOutForeground(object[] args) // float length
    {
        Color baseColor = foregroundImage.color;
        float timer = 0f;
        float l = (float)args[0];
        while (timer < l)
        {
            timer += Time.deltaTime;
            foregroundImage.color = Color.Lerp(baseColor, Color.clear, timer / l);
            yield return 0f;
        }
    }

    IEnumerator<float> cutsceneFX_WaitForClick(object[] args) // no arguments
    {
        if (subtype == CutscenePlayerSubtype.BattleAnim) throw new Exception("WaitForClick isn't a permitted action for battle anims");
        while (!Input.GetMouseButtonDown(0)) yield return 0f;
    }
}