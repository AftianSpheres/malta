using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BattleTheater : MonoBehaviour
{
    public AudioClip ffLose;
    public AudioClip ffWin;
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
    public bool processing { get; private set; }
    private bool _listenForSourceToStopPlaying;
    private string[] turnInfoStrings;
    const float retreatAnimLength = 2.5f;
    const float retreatAnimUIDist = 1000;
    const float retreatAnimWorldDist = 20;
	
    void Awake ()
    {
        turnInfoStrings = turnInfoStringsResource.text.Split('\n');
    }

	// Update is called once per frame
	void Update ()
    {
        if (messageBox.messageQueue.Count > 0) processing = true;
        else if (_listenForSourceToStopPlaying)
        {
            if (source.isPlaying) processing = true;
            else _listenForSourceToStopPlaying = false;
        }
        else processing = false;
	}

    public void StartOfTurn ()
    {
        RefreshTurnInfoPanel();
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

    public void LoseBattle ()
    {
        source.Stop();
        source.PlayOneShot(ffLose);
        _listenForSourceToStopPlaying = true;
    }

    public void WinBattle ()
    {
        source.Stop();
        source.PlayOneShot(ffWin);
        _listenForSourceToStopPlaying = true;
    }

    public void Retreat ()
    {
        StartCoroutine(_Retreat());
    }

    IEnumerator _Retreat ()
    {
        float timer = 0;
        sfxSource.PlayOneShot(retreatSFX);
        Vector3 battleBGBasePos = battleBG.transform.position;
        Vector3 enemyPartyBasePos = enemyParty.transform.position;
        while (timer < retreatAnimLength)
        {
            timer += Time.deltaTime;
            battleBG.transform.position = Vector3.Lerp(battleBGBasePos, battleBGBasePos + (Vector3.left * retreatAnimWorldDist), timer / retreatAnimLength);
            enemyParty.transform.position = Vector3.Lerp(enemyPartyBasePos, enemyPartyBasePos + (Vector3.right * retreatAnimUIDist), timer / retreatAnimLength);
            yield return null;
        }
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
