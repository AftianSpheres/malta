using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleOverseer : MonoBehaviour
{
    public BattleTheater theater;
    public Adventurer[][] enemyPartyConfigs;
    public Battler[] enemyParty;
    public Battler[] playerParty;
    public Battler currentActingBattler;
    public Battler currentTurnTarget;
    public BattlerAction nextAction;
    public SortedList<float, Battler> turnOrderList;
    private Battler[] allBattlers;
    private bool encoreWaitingForEnemies;
    private bool encoreWaitingForPlayer;
    private bool nextActionIsEnemyDeathblow;
    private bool nextActionIsPlayerDeathblow;
    private int battleNo = 0;
    private int turn = 0;
    private int turnStep = 0;
    private const float battleStepLength = 0.5f;
    private List<Battler> validEnemyTargets;
    private List<Battler> validPlayerTargets;
    private float timer;

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(Bootstrap());
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    IEnumerator TurnSequence ()
    {
        StartTurn();

        // Battle start interrupts

        if (turn == 0) for (int i = 0; i < turnOrderList.Count; i++)
            {
                if (TurnStep_Interrupt_BattleStart(turnOrderList[turnOrderList.Keys[i]]))
                {
                    timer = 0;
                    theater.ProcessAction();
                    while (timer < battleStepLength || theater.processing)
                    {
                        timer += Time.deltaTime;
                        yield return null;
                    }
                } 
            }

        // Standard priority attacks + deathblow interrupts

        for (int i = 0; i < turnOrderList.Count; i++)
        {
            TurnStep_StandardAction_Fetch(turnOrderList[turnOrderList.Keys[i]]);
            if (currentActingBattler.deathblowList.Count > 0)
            {
                for (int i2 = 0; i2 < turnOrderList.Count; i2++) if (TurnStep_Interrupt_Deathblow(turnOrderList[turnOrderList.Keys[i2]]))
                {
                    timer = 0;
                    theater.ProcessAction();
                    while (timer < battleStepLength || theater.processing)
                        {
                        timer += Time.deltaTime;
                        yield return null;
                    }
                }
            }
            if (currentActingBattler.isEnemy) currentActingBattler.ExecuteAttack(validPlayerTargets, validEnemyTargets);
            else currentActingBattler.ExecuteAttack(validEnemyTargets, validPlayerTargets);
            timer = 0;
            theater.ProcessAction();
            while (timer < battleStepLength || theater.processing)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        // Hit-revenge interrupts


    }

    void TurnStep_StandardAction_Fetch (Battler bat)
    {
        currentActingBattler = bat;
        if (bat.isEnemy)
        {
            nextAction = bat.GetAction(turn, validPlayerTargets, validEnemyTargets);
        }
        else nextAction = bat.GetAction(turn, validEnemyTargets, validPlayerTargets);
    }

    bool TurnStep_Interrupt_BattleStart (Battler bat)
    {
        BattlerAction action;
        if (bat.isEnemy)
        {
            action = bat.GetInterruptAction(BattlerActionInterruptType.BattleStart, turn, validEnemyTargets, validPlayerTargets);
            if (action != BattlerAction.None) bat.AttackWith(validPlayerTargets, validEnemyTargets, action);
        }
        else
        {
            action = bat.GetInterruptAction(BattlerActionInterruptType.BattleStart, turn, validPlayerTargets, validEnemyTargets);
            if (action != BattlerAction.None) bat.AttackWith(validEnemyTargets, validPlayerTargets, action);
        }
        return action != BattlerAction.None;     
    }

    bool TurnStep_Interrupt_Deathblow (Battler bat)
    {
        BattlerAction action;
        if (bat.isEnemy)
        {
            action = bat.GetInterruptAction(BattlerActionInterruptType.OnAllyDeathblow, turn, validEnemyTargets, validPlayerTargets);
            if (action != BattlerAction.None) bat.AttackWith(validPlayerTargets, validEnemyTargets, action);
        }
        else
        {
            action = bat.GetInterruptAction(BattlerActionInterruptType.OnAllyDeathblow, turn, validPlayerTargets, validEnemyTargets);
            if (action != BattlerAction.None) bat.AttackWith(validEnemyTargets, validPlayerTargets, action);
        }
        return action != BattlerAction.None;
    }

    IEnumerator Bootstrap ()
    {
        while (GameDataManager.Instance == null) yield return null;
        PopulatePlayerParty();
        StartNextBattle();
    }

    private void BuildLists ()
    {
        turnOrderList = new SortedList<float, Battler>(allBattlers.Length);
        validEnemyTargets = new List<Battler>(enemyParty.Length);
        validPlayerTargets = new List<Battler>(playerParty.Length);
        for (int i = 0; i < allBattlers.Length; i++)
        {
            if (allBattlers[i] != null && allBattlers[i].isValidTarget)
            {
                turnOrderList.Add(allBattlers[i].moveSpeed, allBattlers[i]);
                if (allBattlers[i].isEnemy) validEnemyTargets.Add(allBattlers[i]);
                else validPlayerTargets.Add(allBattlers[i]);
            }
        }
    }

    private void ExecuteNextTurnStep ()
    {
        Battler actingBattler = turnOrderList[turnStep];
        turnStep++;
        if (turnStep >= turnOrderList.Count)
        {

        } 
    }

    private void PopulateEnemyParty ()
    {
        Adventurer[] enemyAdventurers = enemyPartyConfigs[battleNo];
        int i2 = 0;
        for (int i = 0; i < enemyAdventurers.Length && i2 < enemyParty.Length; i++)
        {
            if (enemyAdventurers[i] != null)
            {
                enemyParty[i2].SetBattlerActive();
                enemyParty[i2].GenerateBattleData(enemyAdventurers[i]);
                i2++;
            }
        }
        while (i2 < playerParty.Length)
        {
            enemyParty[i2].SetBattlerInactive();
            i2++;
        }
    }

    private void PopulatePlayerParty ()
    {
        Adventurer[] playerAdventurers = new Adventurer[] { GameDataManager.Instance.sovereignAdventurer, GameDataManager.Instance.houseAdventurers[0],
                                                            GameDataManager.Instance.houseAdventurers[1], GameDataManager.Instance.forgeAdventurer };
        int i2 = 0;
        for (int i = 0; i < playerAdventurers.Length && i2 < playerParty.Length; i++)
        {
            if (playerAdventurers[i] != null && playerAdventurers[i].initialized)
            {
                playerParty[i2].GenerateBattleData(playerAdventurers[i]);
                i2++;
            }
        }
        while (i2 < playerParty.Length)
        {
            playerParty[i2].SetBattlerInactive();
            i2++;
        }
    }

    private void RebuildAllBattlersArray ()
    {
        Battler[] longAllBattlers = new Battler[playerParty.Length + enemyParty.Length];
        int i2 = 0;
        for (int i = 0; i < playerParty.Length; i++) if (playerParty[i].gameObject.activeInHierarchy)
            {
                longAllBattlers[i2] = playerParty[i];
                i2++;
            }
        for (int i = 0; i < enemyParty.Length; i++) if (enemyParty[i].gameObject.activeInHierarchy)
            {
                longAllBattlers[i2] = enemyParty[i];
                i2++;
            }
        allBattlers = new Battler[i2];
        for (int i = 0; i < allBattlers.Length; i++) allBattlers[i] = longAllBattlers[i];
    }

    private void StartNextBattle ()
    {
        PopulateEnemyParty();
        RebuildAllBattlersArray();
    }

    private void StartTurn ()
    {
        for (int i = 0; i < allBattlers.Length; i++) if (!allBattlers[i].gameObject.activeInHierarchy) RebuildAllBattlersArray();
        BuildLists();
    }

    public void GiveEncore (bool toEnemy)
    {
        if (toEnemy) encoreWaitingForEnemies = true;
        else encoreWaitingForPlayer = true;
    }
}
