using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MovementEffects;

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
    Loss,
    Retreat,
    FailedCast,
    PulledOutOfBattle,
    ReenteredBattle,
    StunBuff,
    StunBuffFaded,
    Stun,
    PumpedUp,
    MovesBetweenLines,
    SwiftlyMovesBetweenLines,
    AttunedBuff,
    AttunedBuffFaded,
    Dodged,
    DrainKill,
    FailedToDrain,
    MagBoost,
    StatBoostsLost,
    SacrificeLost,
    Sacrifice
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
    public Queue<Battler> actorQueue;
    public Queue<Battler> corpseQueue;
    public Queue<BattlerAction> actionQueue;
    public bool processing { get { return _processing || messageQueue.Count > 0 || !_clear; } }
    private string[] mainStrings;
    private string[] actionNameStrings;
    private float timer = messageDelay;
    private bool _processing;
    private const float messageDelay = .33f;
    private bool _clear = true;

	// Use this for initialization
	void Start ()
    {
        mainStrings = mainStringsResource.text.Split('\n');
        actionNameStrings = actionNameStringsResource.text.Split('\n');
        corpseQueue = new Queue<Battler>();
        actorQueue = new Queue<Battler>();
        actionQueue = new Queue<BattlerAction>();
        messageQueue = new Queue<BattleMessageType>();
        uiText.text = "";
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (messageQueue.Count > 0)
        {
            if (timer < messageDelay) timer += Timing.DeltaTime;
            else if (timer >= messageDelay) NextMessage();
            if (messageQueue.Count == 0) timer = 0;
        }
        else if (!_clear)
        {
            if (timer < messageDelay) timer += Timing.DeltaTime;
            else if (timer >= messageDelay) _clear = true;
        }
	}

    public void Step(BattleMessageType message = BattleMessageType.StandardTurnMessage, Battler actor = default(Battler), BattlerAction action = BattlerAction.None)
    {
        _clear = false;
        messageQueue.Enqueue(message);
        actorQueue.Enqueue(actor);
        actionQueue.Enqueue(action);
    }

    public void FlushWhenReady ()
    {
        Timing.RunCoroutine(_FlushWhenReady());
    }

    IEnumerator<float> _FlushWhenReady ()
    {
        _processing = true;
        while (messageQueue.Count > 0 || timer < messageDelay) yield return 0f;
        Flush();
        timer = 0;
        _processing = false;
        _clear = false;
    }

    private void Flush ()
    {
        uiText.text = "";
        line0 = "";
        line1 = "";
    }

    private void NextMessage ()
    {
        timer = 0;
        BattleMessageType message = messageQueue.Dequeue();
        Battler actor = actorQueue.Dequeue();
        BattlerAction action = actionQueue.Dequeue();
        bool drawToLine1 = true;
        if (line0 == "") drawToLine1 = false;
        else if (line1 != "") line0 = line1;
        string baseLine;
        string nextLine = "";
        switch (message)
        {
            case BattleMessageType.StandardTurnMessage:
                if (actor == null) throw new System.Exception("Can't do standard turn message with null battler!");
                if (overseer.standardActionPriorityBracket) baseLine = mainStrings[0];
                else baseLine = mainStrings[1];
                if (overseer.currentActingBattler.isEnemy) nextLine = actor.adventurer.title + baseLine + actionNameStrings[(int)action];
                else nextLine = actor.adventurer.fullName + baseLine + actionNameStrings[(int)action];
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
            case BattleMessageType.Retreat:
                nextLine = mainStrings[18];
                break;
            case BattleMessageType.FailedCast:
                if (overseer.currentActingBattler.isEnemy) nextLine = actor.adventurer.title + mainStrings[19];
                else nextLine = actor.adventurer.fullName + mainStrings[19];
                break;
            case BattleMessageType.PulledOutOfBattle:
                nextLine = actor.adventurer.fullName + mainStrings[20];
                break;
            case BattleMessageType.ReenteredBattle:
                nextLine = actor.adventurer.fullName + mainStrings[21];
                break;
            case BattleMessageType.StunBuff:
                nextLine = mainStrings[22];
                break;
            case BattleMessageType.StunBuffFaded:
                nextLine = mainStrings[23];
                break;
            case BattleMessageType.Stun:
                nextLine = mainStrings[24];
                break;
            case BattleMessageType.PumpedUp:
                nextLine = mainStrings[25];
                break;
            case BattleMessageType.MovesBetweenLines:
                nextLine = mainStrings[26];
                break;
            case BattleMessageType.SwiftlyMovesBetweenLines:
                nextLine = mainStrings[27];
                break;
            case BattleMessageType.AttunedBuff:
                nextLine = mainStrings[28];
                break;
            case BattleMessageType.AttunedBuffFaded:
                nextLine = mainStrings[29];
                break;
            case BattleMessageType.Dodged:
                nextLine = mainStrings[30];
                break;
            case BattleMessageType.DrainKill:
                nextLine = mainStrings[31];
                break;
            case BattleMessageType.FailedToDrain:
                nextLine = mainStrings[32];
                break;
            case BattleMessageType.MagBoost:
                nextLine = mainStrings[33];
                break;
            case BattleMessageType.StatBoostsLost:
                nextLine = mainStrings[34];
                break;
            case BattleMessageType.SacrificeLost:
                nextLine = mainStrings[35];
                break;
            case BattleMessageType.Sacrifice:
                nextLine = mainStrings[36];
                break;
        }
        if (drawToLine1) line1 = nextLine;
        else line0 = nextLine;
        uiText.text = line0 + System.Environment.NewLine + line1;
    }
}
