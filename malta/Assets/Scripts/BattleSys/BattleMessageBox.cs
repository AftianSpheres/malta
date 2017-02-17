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
    Sacrifice,
    StunnedNoMove
}

public class BattleMessageBox : MonoBehaviour
{
    public BattleOverseer overseer;
    public Text bigMessageBoxText;
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
    private static string dividerString = System.Environment.NewLine + "------------------------------------------------------" + System.Environment.NewLine;
    private const int lineCount = 27;
    private List<string> messageStrings;

    // Use this for initialization
    void Start ()
    {
        mainStrings = mainStringsResource.text.Split('\n');
        actionNameStrings = actionNameStringsResource.text.Split('\n');
        corpseQueue = new Queue<Battler>();
        actorQueue = new Queue<Battler>();
        actionQueue = new Queue<BattlerAction>();
        messageQueue = new Queue<BattleMessageType>();
        messageStrings = new List<string>();
        bigMessageBoxText.text = "";
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

    private void NextMessage ()
    {
        timer = 0;
        BattleMessageType message = messageQueue.Dequeue();
        Battler actor = actorQueue.Dequeue();
        BattlerAction action = actionQueue.Dequeue();
        string baseLine;
        string nextMsg = "";
        switch (message)
        {
            case BattleMessageType.StandardTurnMessage:
                if (actor == null) throw new System.Exception("Can't do standard turn message with null battler!");
                if (overseer.standardActionPriorityBracket) baseLine = mainStrings[0];
                else baseLine = mainStrings[1];
                if (overseer.currentActingBattler.isEnemy) nextMsg = actor.adventurer.title + baseLine + actionNameStrings[(int)action];
                else nextMsg = actor.adventurer.fullName + baseLine + actionNameStrings[(int)action];
                break;
            case BattleMessageType.Silence:
                nextMsg = mainStrings[3];
                break;
            case BattleMessageType.ShieldWall:
                nextMsg = mainStrings[4];
                break;
            case BattleMessageType.ShieldBlock:
                nextMsg = mainStrings[5];
                break;
            case BattleMessageType.Heal:
                nextMsg = mainStrings[6];
                break;
            case BattleMessageType.Haste:
                nextMsg = mainStrings[7];
                break;
            case BattleMessageType.Barrier:
                nextMsg = mainStrings[8];
                break;
            case BattleMessageType.MultiHeal:
                nextMsg = mainStrings[9];
                break;
            case BattleMessageType.Encore:
                nextMsg = mainStrings[10];
                break;
            case BattleMessageType.Feedback:
                nextMsg = mainStrings[11];
                break;
            case BattleMessageType.Revenge:
                nextMsg = mainStrings[12];
                break;
            case BattleMessageType.Critical:
                nextMsg = mainStrings[13];
                break;
            case BattleMessageType.SavedAlly:
                nextMsg = mainStrings[14];
                break;
            case BattleMessageType.Flanking:
                nextMsg = mainStrings[15];
                break;
            case BattleMessageType.SomebodyDead:
                Battler bat = corpseQueue.Dequeue();
                if (bat.isEnemy) nextMsg = bat.adventurer.title + mainStrings[2];
                else nextMsg = bat.adventurer.fullName + mainStrings[2];
                break;
            case BattleMessageType.Win:
                nextMsg = mainStrings[16];
                break;
            case BattleMessageType.Loss:
                nextMsg = mainStrings[17];
                break;
            case BattleMessageType.Retreat:
                nextMsg = mainStrings[18];
                break;
            case BattleMessageType.FailedCast:
                if (overseer.currentActingBattler.isEnemy) nextMsg = actor.adventurer.title + mainStrings[19];
                else nextMsg = actor.adventurer.fullName + mainStrings[19];
                break;
            case BattleMessageType.PulledOutOfBattle:
                nextMsg = actor.adventurer.fullName + mainStrings[20];
                break;
            case BattleMessageType.ReenteredBattle:
                nextMsg = actor.adventurer.fullName + mainStrings[21];
                break;
            case BattleMessageType.StunBuff:
                nextMsg = mainStrings[22];
                break;
            case BattleMessageType.StunBuffFaded:
                nextMsg = mainStrings[23];
                break;
            case BattleMessageType.Stun:
                nextMsg = mainStrings[24];
                break;
            case BattleMessageType.PumpedUp:
                nextMsg = mainStrings[25];
                break;
            case BattleMessageType.MovesBetweenLines:
                nextMsg = mainStrings[26];
                break;
            case BattleMessageType.SwiftlyMovesBetweenLines:
                nextMsg = mainStrings[27];
                break;
            case BattleMessageType.AttunedBuff:
                nextMsg = mainStrings[28];
                break;
            case BattleMessageType.AttunedBuffFaded:
                nextMsg = mainStrings[29];
                break;
            case BattleMessageType.Dodged:
                nextMsg = mainStrings[30];
                break;
            case BattleMessageType.DrainKill:
                nextMsg = mainStrings[31];
                break;
            case BattleMessageType.FailedToDrain:
                nextMsg = mainStrings[32];
                break;
            case BattleMessageType.MagBoost:
                nextMsg = mainStrings[33];
                break;
            case BattleMessageType.StatBoostsLost:
                nextMsg = mainStrings[34];
                break;
            case BattleMessageType.SacrificeLost:
                nextMsg = mainStrings[35];
                break;
            case BattleMessageType.Sacrifice:
                nextMsg = mainStrings[36];
                break;
            case BattleMessageType.StunnedNoMove:
                if (overseer.currentActingBattler.isEnemy) nextMsg = actor.adventurer.title + mainStrings[37];
                else nextMsg = actor.adventurer.fullName + mainStrings[37];
                break;
        }
        string[] nextLineSplit = Util.GetLinesFrom(nextMsg);
        messageStrings.Insert(0, dividerString);
        for (int i = nextLineSplit.Length - 1; i > -1; i--) messageStrings.Insert(0, nextLineSplit[i]);
        for (int i = messageStrings.Count - 1; i >= lineCount; i--) messageStrings.RemoveAt(i);
        //messageStrings.Insert(0, nextMsg);
        // i need indexable lifo...
        // actually split these by lines!!!
        bigMessageBoxText.text = string.Concat(messageStrings.ToArray());
    }
}
