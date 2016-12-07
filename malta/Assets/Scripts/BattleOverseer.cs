using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleOverseer : MonoBehaviour
{
    public BattleTheater theater;
    public BattleMessageBox messageBox;
    public Adventurer[][] enemyPartyConfigs;
    public Battler[] enemyParty;
    public Battler[] playerParty;
    public Battler currentActingBattler;
    public Battler currentTurnTarget;
    public BattlerAction nextAction;
    public SortedList<float, Battler> turnOrderList;
    public Battler[] allBattlers { get; private set; }
    public bool standardActionPriorityBracket;
    private bool currentBattleResolved;
    private bool encoreWaitingForEnemies;
    private bool encoreWaitingForPlayer;
    private bool nextActionIsEnemyDeathblow;
    private bool nextActionIsPlayerDeathblow;
    private int battleNo = 0;
    private int turn = 0;
    private const float battleStepLength = 1.33f;
    private List<Battler> validEnemyTargets;
    private List<Battler> validPlayerTargets;
    private float timer;
    private bool processingBattle = false;
    private bool processingTurn = false;
    private bool processingTurnStep = false;

	// Use this for initialization
	void Start ()
    {
        enemyPartyConfigs = new Adventurer[][] { new Adventurer[] { ScriptableObject.CreateInstance<Adventurer>(), ScriptableObject.CreateInstance<Adventurer>(), ScriptableObject.CreateInstance<Adventurer>(), ScriptableObject.CreateInstance<Adventurer>() } };
        for (int i = 0; i < enemyPartyConfigs[0].Length; i++) enemyPartyConfigs[0][i].Reroll(AdventurerClass.Bowman, AdventurerSpecies.Human, false, new int[] { 0, 0, 0, 0 });
        StartCoroutine(Bootstrap());
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (processingBattle && !processingTurn) StartCoroutine(RunTurn());
	}

    IEnumerator RunTurn ()
    {
        Debug.Log("Start turn " + turn.ToString());
        processingTurn = true;
        StartTurn();
        StartCoroutine(HandleAheadOfStandardActions());
        while (processingTurnStep) yield return null;
        if (currentBattleResolved)
        {
            EndBattle();
            yield break;
        }
        StartCoroutine(HandleStandardPriorityActions());
        while (processingTurnStep) yield return null;
        if (currentBattleResolved)
        {
            EndBattle();
            yield break;
        }
        StartCoroutine(HandleBehindStandardActions());
        while (processingTurnStep) yield return null;
        if (currentBattleResolved)
        {
            EndBattle();
            yield break;
        }
        StartCoroutine(HandleStandardPriorityActions(true));
        while (processingTurnStep) yield return null;
        if (currentBattleResolved)
        {
            EndBattle();
            yield break;
        }
        ClearBattlerHitStatuses();
        processingTurn = false;
    }

    IEnumerator HandleAheadOfStandardActions ()
    {
        Debug.Log("Priority 0 going");
        processingTurnStep = true;
        if (turn == 0) for (int i = 0; i < turnOrderList.Count; i++)
            {
                if (SubTurnStep_Interrupt_BattleStart(turnOrderList[turnOrderList.Keys[i]]))
                {
                    timer = 0;
                    theater.ProcessAction();
                    while (timer < battleStepLength || theater.processing)
                    {
                        timer += Time.deltaTime;
                        yield return null;
                    }
                    if (CheckIfBattleResolved()) yield break;
                }
            }
        processingTurnStep = false;
    }

    IEnumerator HandleStandardPriorityActions (bool isEncore = false)
    {
        Debug.Log("Priority 1 going");
        processingTurnStep = true;
        for (int i = 0; i < turnOrderList.Count; i++)
        {
            if (isEncore) if (turnOrderList[turnOrderList.Keys[i]].isEnemy && !encoreWaitingForEnemies || !turnOrderList[turnOrderList.Keys[i]].isEnemy && !encoreWaitingForPlayer)
            {
                processingTurnStep = false;
                continue;
            }
            SubTurnStep_StandardAction_Fetch(turnOrderList[turnOrderList.Keys[i]]);
            if (currentActingBattler.deathblowList.Count > 0)
            {
                for (int i2 = 0; i2 < turnOrderList.Count; i2++) if (SubTurnStep_Interrupt_Deathblow(turnOrderList[turnOrderList.Keys[i2]]))
                    {
                        timer = 0;
                        theater.ProcessAction();
                        while (timer < battleStepLength || theater.processing)
                        {
                            timer += Time.deltaTime;
                            yield return null;
                        }
                        if (CheckIfBattleResolved()) yield break;
                    }
            }
            standardActionPriorityBracket = true;
            if (currentActingBattler.isEnemy) currentActingBattler.ExecuteAttack(validPlayerTargets, validEnemyTargets);
            else currentActingBattler.ExecuteAttack(validEnemyTargets, validPlayerTargets);
            timer = 0;
            theater.ProcessAction();
            while (timer < battleStepLength || theater.processing)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            standardActionPriorityBracket = false;
            if (CheckIfBattleResolved()) yield break;
        }
        processingTurnStep = false;
    }

    IEnumerator HandleBehindStandardActions ()
    {
        Debug.Log("Priority 2 going");
        processingTurnStep = true;
        for (int i = 0; i < turnOrderList.Count; i++)
        {
            Battler bat = turnOrderList[turnOrderList.Keys[i]];
            Battler[] bats;
            if (bat.isEnemy) bats = enemyParty;
            else bats = playerParty; 
            for (int i2 = 0; i2 < bats.Length; i2++)
            {
                if (bats[i2] != null && bats[i2].incomingHit)
                {
                    if (SubTurnStep_Interrupt_OnAllySideHit(bat, bats[i2], bats))
                    {
                        timer = 0;
                        theater.ProcessAction();
                        while (timer < battleStepLength || theater.processing)
                        {
                            timer += Time.deltaTime;
                            yield return null;
                        }
                        if (CheckIfBattleResolved()) yield break;
                    }
                }
            }
        }
        processingTurnStep = false;
    }

    public void EndBattle ()
    {
        if (validPlayerTargets.Count > 0) messageBox.Step(BattleMessageType.Win);
        else messageBox.Step(BattleMessageType.Loss);
    }

    bool CheckIfBattleResolved ()
    {
        for (int i = 0; i < validEnemyTargets.Count; i++) if (validEnemyTargets[i].dead) validEnemyTargets.RemoveAt(i);
        for (int i = 0; i < validPlayerTargets.Count; i++) if (validPlayerTargets[i].dead) validPlayerTargets.RemoveAt(i);
        currentBattleResolved = (validEnemyTargets.Count < 1 || validPlayerTargets.Count < 1);
        if (currentBattleResolved) processingTurnStep = false; // clean up before breaking out of those coroutines
        processingBattle = !currentBattleResolved;
        return currentBattleResolved;
    }

    void SubTurnStep_StandardAction_Fetch (Battler bat)
    {
        currentActingBattler = bat;
        if (bat.isEnemy)
        {
            nextAction = bat.GetAction(turn, validEnemyTargets, validPlayerTargets);
            bat.ReadyAttack(validPlayerTargets, validEnemyTargets, nextAction);
        }
        else
        {
            nextAction = bat.GetAction(turn, validPlayerTargets, validEnemyTargets);
            bat.ReadyAttack(validEnemyTargets, validPlayerTargets, nextAction);
        }
    }

    bool SubTurnStep_Interrupt_BattleStart (Battler bat)
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

    bool SubTurnStep_Interrupt_Deathblow (Battler bat)
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

    bool SubTurnStep_Interrupt_OnAllySideHit (Battler bat, Battler batHit, Battler[] bats)
    {
        BattlerAction action;
        if (bat.isEnemy)
        {
            if (bat == batHit) action = bat.GetInterruptAction(BattlerActionInterruptType.OnHit, turn, validEnemyTargets, validPlayerTargets);
            else action = bat.GetInterruptAction(BattlerActionInterruptType.OnAllyHit, turn, validEnemyTargets, validPlayerTargets);
            if (action != BattlerAction.None) bat.AttackWith(validPlayerTargets, validEnemyTargets, action);
        }
        else
        {
            if (bat == batHit) action = bat.GetInterruptAction(BattlerActionInterruptType.OnHit, turn, validPlayerTargets, validEnemyTargets);
            else action = bat.GetInterruptAction(BattlerActionInterruptType.OnAllyHit, turn, validPlayerTargets, validEnemyTargets);
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
                if (turnOrderList.ContainsKey(allBattlers[i].moveSpeed))
                {
                    if (Random.Range(0, 2) == 0) turnOrderList.Add(allBattlers[i].moveSpeed + (0.01f * i), allBattlers[i]);
                    else turnOrderList.Add(allBattlers[i].moveSpeed - (0.01f * i), allBattlers[i]);
                }
                else turnOrderList.Add(allBattlers[i].moveSpeed, allBattlers[i]);
                if (allBattlers[i].isEnemy) validEnemyTargets.Add(allBattlers[i]);
                else validPlayerTargets.Add(allBattlers[i]);
            }
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
        processingBattle = true;
        PopulateEnemyParty();
        RebuildAllBattlersArray();
        StartCoroutine(RunTurn());
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

    public void ClearBattlerHitStatuses ()
    {
        for (int i = 0; i < allBattlers.Length; i++) if (allBattlers[i] != null) allBattlers[i].incomingHit = false;
    }
}
