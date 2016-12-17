using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MovementEffects;

public class BattleTheater : MonoBehaviour
{
    public AudioClip ffLose;
    public AudioClip ffWin;
    public AudioClip bgmAdventureWin;
    public AudioClip retreatSFX;
    public AudioSource source;
    public AudioSource sfxSource;
    public BattleOverseer overseer;
    public BattleMessageBox messageBox;
    public GameObject enemyParty;
    public SpriteRenderer battleBG;
    public Sprite[] battleBG_bgs;
    public TextAsset turnInfoStringsResource;
    public Text turnInfoPanel;
    public bool processing { get { return _processing || messageBox.processing; } }
    private string[] turnInfoStrings;
    private bool _processing;
    const float retreatAnimLength = 2.5f;
    const float retreatAnimUIDist = 1000;
    const float retreatAnimWorldDist = 20;
	
    void Awake ()
    {
        turnInfoStrings = turnInfoStringsResource.text.Split('\n');
    }

    private void Update()
    {

    }

    public void StartOfTurn ()
    {
        RefreshTurnInfoPanel();
        messageBox.FlushWhenReady();
    }

    public void ProcessAction ()
    {
        for (int i = 0; i < overseer.allBattlers.Length; i++) if (overseer.allBattlers[i] != null) overseer.allBattlers[i].puppet.Respond();
    }

    public void StartBattle ()
    {
        if (GameDataManager.Instance.adventureLevel < AdventureSubstageLoader.randomAdventureBaseLevel) battleBG.sprite = battleBG_bgs[GameDataManager.Instance.adventureLevel];
        else battleBG.sprite = battleBG_bgs[AdventureSubstageLoader.randomAdventureBaseLevel];
        source.clip = overseer.adventure[overseer.battleNo].battleBGM;
        source.Play();
    }

    IEnumerator<float> _PlayFanfare (AudioClip ff)
    {
        source.Stop();
        source.PlayOneShot(ff);
        while (source.isPlaying) yield return 0f;
    }

    public IEnumerator<float> LoseBattle ()
    {
        return Timing.RunCoroutine(_PlayFanfare(ffLose));
    }

    public void WinAdventure ()
    {
        source.clip = bgmAdventureWin;
        source.Play();
    }

    public IEnumerator<float> WinBattle ()
    {
        return Timing.RunCoroutine(_PlayFanfare(ffWin));
    }

    public void Retreat ()
    {
        Timing.RunCoroutine(_Retreat());
    }

    IEnumerator<float> _Retreat ()
    {
        float timer = 0;
        _processing = true;
        sfxSource.PlayOneShot(retreatSFX);
        Vector3 battleBGBasePos = battleBG.transform.position;
        Vector3 enemyPartyBasePos = enemyParty.transform.position;
        while (timer < retreatAnimLength)
        {
            timer += Timing.DeltaTime;
            battleBG.transform.position = Vector3.Lerp(battleBGBasePos, battleBGBasePos + (Vector3.left * retreatAnimWorldDist), timer / retreatAnimLength);
            enemyParty.transform.position = Vector3.Lerp(enemyPartyBasePos, enemyPartyBasePos + (Vector3.right * retreatAnimUIDist), timer / retreatAnimLength);
            yield return 0f;
        }   
        _processing = false;
    }

    private void RefreshTurnInfoPanel ()
    {
        string line0;
        if (GameDataManager.Instance.adventureLevel > AdventureSubstageLoader.randomAdventureBaseLevel) line0 = turnInfoStrings[AdventureSubstageLoader.randomAdventureBaseLevel];
        else line0 = turnInfoStrings[GameDataManager.Instance.adventureLevel];
        string line1 = turnInfoStrings[4] + (overseer.battleNo + 1).ToString() + turnInfoStrings[5];
        string line2 = turnInfoStrings[6] + (overseer.turn + 1).ToString();
        turnInfoPanel.text = line0 + System.Environment.NewLine + line1 + System.Environment.NewLine + line2;
    }
}
