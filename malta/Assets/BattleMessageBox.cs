using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum BattleMessageType
{
    StandardTurnMessage,
    Silence,
    ShieldWall,
    ShieldBlock,
    Heal,
    Haste,
    Barrier,
    MultiHeal,
    Encore,
    Feedback,
    Revenge,
    Critical,
    SavedAlly,
    Flanking,
    SomebodyDead,
    Win,
    Loss
}

public class BattleMessageBox : MonoBehaviour
{
    public BattleOverseer overseer;
    public Text uiText;
    public string line0;
    public string line1;
    public TextAsset mainStringsResource;
    public TextAsset actionNameStringsResource;
    public Queue<BattleMessageType> messageQueue;
    public Queue<Battler> corpseQueue;
    private string[] mainStrings;
    private string[] actionNameStrings;
    private float timer;
    private const float messageDelay = .66f;

	// Use this for initialization
	void Start ()
    {
        mainStrings = mainStringsResource.text.Split('\n');
        actionNameStrings = actionNameStringsResource.text.Split('\n');
        corpseQueue = new Queue<Battler>();
        messageQueue = new Queue<BattleMessageType>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (messageQueue.Count > 0)
        {
            if (timer < messageDelay) timer += Time.deltaTime;
            else if (timer >= messageDelay) NextMessage();
        }
	}

    public void Step(BattleMessageType message = BattleMessageType.StandardTurnMessage)
    {
        messageQueue.Enqueue(message);
    }

    private void NextMessage ()
    {
        timer = 0;
        BattleMessageType message = messageQueue.Dequeue();
        bool drawToLine1 = true;
        if (line0 == "") drawToLine1 = false;
        else if (line1 != "") line0 = line1;
        string baseLine;
        string nextLine = "";
        switch (message)
        {
            case BattleMessageType.StandardTurnMessage:
                if (overseer.standardActionPriorityBracket) baseLine = mainStrings[0];
                else baseLine = mainStrings[1];
                if (overseer.currentActingBattler.isEnemy) nextLine = overseer.currentActingBattler.adventurer.title + baseLine + actionNameStrings[(int)overseer.nextAction];
                else nextLine = overseer.currentActingBattler.adventurer.fullName + baseLine + actionNameStrings[(int)overseer.nextAction];
                break;
            case BattleMessageType.Silence:
                nextLine = mainStrings[3];
                break;
            case BattleMessageType.ShieldWall:
                nextLine = mainStrings[4];
                break;
            case BattleMessageType.ShieldBlock:
                nextLine = mainStrings[5];
                break;
            case BattleMessageType.Heal:
                nextLine = mainStrings[6];
                break;
            case BattleMessageType.Haste:
                nextLine = mainStrings[7];
                break;
            case BattleMessageType.Barrier:
                nextLine = mainStrings[8];
                break;
            case BattleMessageType.MultiHeal:
                nextLine = mainStrings[9];
                break;
            case BattleMessageType.Encore:
                nextLine = mainStrings[10];
                break;
            case BattleMessageType.Feedback:
                nextLine = mainStrings[11];
                break;
            case BattleMessageType.Revenge:
                nextLine = mainStrings[12];
                break;
            case BattleMessageType.Critical:
                nextLine = mainStrings[13];
                break;
            case BattleMessageType.SavedAlly:
                nextLine = mainStrings[14];
                break;
            case BattleMessageType.Flanking:
                nextLine = mainStrings[15];
                break;
            case BattleMessageType.SomebodyDead:
                Battler bat = corpseQueue.Dequeue();
                if (bat.isEnemy) nextLine = bat.adventurer.title + mainStrings[2];
                else nextLine = bat.adventurer.fullName + mainStrings[2];
                break;
            case BattleMessageType.Win:
                nextLine = mainStrings[16];
                break;
            case BattleMessageType.Loss:
                nextLine = mainStrings[17];
                break;
        }
        if (drawToLine1) line1 = nextLine;
        else line0 = nextLine;
        uiText.text = line0 + System.Environment.NewLine + line1;
    }
}
