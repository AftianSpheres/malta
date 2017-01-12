using UnityEngine;
using System.Collections.Generic;
using MovementEffects;

public class BattleOverseer : MonoBehaviour
{
    public BattleTheater theater;
    public BattleMessageBox messageBox;
    public Battler[] enemyParty;
    public Battler[] playerParty;
    public Battler currentActingBattler;
    public Battler currentTurnTarget;
    public Battler standardPriorityActingBattler;
    public Battler outOfBattleBattler;
    public BattlerAction nextAction;
    public PopupMenu battleEndPopup;
    public SortedList<float, Battler> turnOrderList;
    public Battler[] allBattlers { get; private set; }
    public ScreenChanger screenChanger;
    public GameObject retreatButton;
    public BattlerActionAnim lastActionAnim;
    public bool standardActionPriorityBracket;
    public bool retreatingAtStartOfNextTurn { get; private set; }
    public bool currentlyInFight { get; private set; }
    [System.NonSerialized] public AdventureSubstage[] adventure;
    [System.NonSerialized] public int battleNo = 0;
    private bool currentBattleResolved;
    public bool encoreWaitingForEnemies { get; private set; }
    public bool encoreWaitingForPlayer { get; private set; }
    public int playerDeaths;
    public int turn { get; private set; }
    public string lastDeadPlayerAdvName = "If this appears report it";
    private const float battleStepLength = .66f;
    private List<Battler> validEnemyTargets;
    private List<Battler> validPlayerTargets;
    const string _owned = "_BattleOverseerCoroutine";
    private int[] baseEndlessAdventurePayout;
    private bool _forceAllBattlersRegen = false;
    public bool tutorial { get; private set; }

    // Use this for initialization
    void Start ()
    {
        turnOrderList = new SortedList<float, Battler>(enemyParty.Length + playerParty.Length);
        validEnemyTargets = new List<Battler>(enemyParty.Length);
        validPlayerTargets = new List<Battler>(playerParty.Length);
        retreatButton.SetActive(false);
        Timing.RunCoroutine(Bootstrap(), _owned);
        baseEndlessAdventurePayout = new int[] { GameDataManager.Instance.dataStore.nextRandomAdventureAnte, GameDataManager.Instance.dataStore.nextRandomAdventureAnte, GameDataManager.Instance.dataStore.nextRandomAdventureAnte };
	}

    IEnumerator<float> RunTurn ()
    {
        if (retreatingAtStartOfNextTurn) Retreat();
        StartTurn();
        theater.StartOfTurn();
        while (theater.processing) yield return 0f;
        if (!currentBattleResolved)
        {
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(HandleAheadOfStandardActions(), _owned));
            while (theater.processing) yield return 0f;
            if (!currentBattleResolved)
            {
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(HandleStandardPriorityActions(), _owned));
                while (theater.processing) yield return 0f;
                if (!currentBattleResolved && (encoreWaitingForEnemies || encoreWaitingForPlayer))
                {
                    yield return Timing.WaitUntilDone(Timing.RunCoroutine(HandleStandardPriorityActions(), _owned));
                    while (theater.processing) yield return 0f;
                }
                ClearBattlerHitStatuses();
            }
        }
        while (theater.processing) yield return 0f;
        EndTurn();
    }

    IEnumerator<float> HandleAheadOfStandardActions ()
    {
        if (turn == 0)
        {
            for (int i = 0; i < turnOrderList.Count; i++)
            {
                Battler bat = turnOrderList[turnOrderList.Keys[i]];
                if (!bat.isValidTarget) continue;
                if (SubTurnStep_Interrupt_BattleStart(bat))
                {
                    theater.ProcessAction();
                    while (theater.processing) yield return 0f;
                    yield return Timing.WaitForSeconds(battleStepLength);
                    if (CheckIfBattleResolved()) yield break;
                }
            }
        }
    }

    IEnumerator<float> HandleStandardPriorityActions (bool isEncore = false)
    {
        for (int i = 0; i < turnOrderList.Count; i++)
        {
            Battler bat = turnOrderList[turnOrderList.Keys[i]];
            if (!bat.isValidTarget) continue;
            if (isEncore) if (bat.isEnemy && !encoreWaitingForEnemies || !bat.isEnemy && !encoreWaitingForPlayer) continue;
            SubTurnStep_StandardAction_Fetch(bat);
            if (standardPriorityActingBattler.deathblowList.Count > 0)
            {
                Battler exBat;
                for (int i2 = 0; i2 < turnOrderList.Count; i2++)
                {
                    exBat = turnOrderList[turnOrderList.Keys[i2]];
                    if (exBat.isEnemy != standardPriorityActingBattler.isEnemy && SubTurnStep_Interrupt_Deathblow(exBat))
                    {
                        theater.ProcessAction();
                        while (theater.processing) yield return 0f;
                        yield return Timing.WaitForSeconds(battleStepLength);
                        if (CheckIfBattleResolved()) yield break;
                    }
                }
            }
            currentActingBattler = standardPriorityActingBattler;
            if (currentActingBattler.isValidTarget)
            {
                standardActionPriorityBracket = true;
                if (currentActingBattler.isEnemy) currentActingBattler.ExecuteAttack(validPlayerTargets, validEnemyTargets);
                else currentActingBattler.ExecuteAttack(validEnemyTargets, validPlayerTargets);
                theater.ProcessAction();
                while (theater.processing) yield return 0f;
                yield return Timing.WaitForSeconds(battleStepLength);
                Battler[] bats;
                standardActionPriorityBracket = false;
                if (currentActingBattler.isEnemy) bats = playerParty; // dumbest bug ever: "why don't I run hit interrupts when I'm checking for the enemy party???"
                else bats = enemyParty;
                for (int i2 = 0; i2 < bats.Length; i2++)
                {
                    if (bats[i2] != null && bats[i2].isValidTarget)
                    {
                        if (SubTurnStep_Interrupt_AllyHit(bats[i2], bats))
                        {
                            theater.ProcessAction();
                            while (theater.processing) yield return 0f;
                            yield return Timing.WaitForSeconds(battleStepLength);
                            if (CheckIfBattleResolved()) yield break;
                        }
                        if (SubTurnStep_Interrupt_SelfHit(bats[i2]))
                        {
                            theater.ProcessAction();
                            while (theater.processing) yield return 0f;
                            yield return Timing.WaitForSeconds(battleStepLength);
                            if (CheckIfBattleResolved()) yield break;
                        }
                    }
                }
            }
            if (CheckIfBattleResolved()) yield break;
        }
    }

    IEnumerator<float> WinAndContinueBattling ()
    {
        retreatButton.SetActive(true);
        messageBox.Step(BattleMessageType.Win);
        yield return Timing.WaitUntilDone(theater.WinBattle());
        while (theater.processing) yield return 0f;
        yield return Timing.WaitForSeconds(battleStepLength * 3);
        retreatButton.SetActive(false);
        StartNextBattle();
    }

    IEnumerator<float> WinAndPrepareForAdventureExit ()
    {
        messageBox.Step(BattleMessageType.Win);
        yield return Timing.WaitUntilDone(theater.WinBattle());
        yield return Timing.WaitForSeconds(battleStepLength * 3);
        theater.WinAdventure();
        while (theater.processing) yield return 0f;
        if (tutorial)
        {
            GameDataManager.Instance.dataStore.housingLevel++;
        }
        else
        {
            if (GameDataManager.Instance.dataStore.adventureLevel >= AdventureSubstageLoader.randomAdventureBaseLevel)
            {
                GameDataManager.Instance.dataStore.nextRandomAdventureAnte += Random.Range(1, 4);
                GameDataManager.Instance.dataStore.resBricks += baseEndlessAdventurePayout[0];
                GameDataManager.Instance.dataStore.resPlanks += baseEndlessAdventurePayout[1];
                GameDataManager.Instance.dataStore.resMetal += baseEndlessAdventurePayout[2];
            }
            GameDataManager.Instance.dataStore.adventureLevel++;
        }
        battleEndPopup.Open();
    }

    IEnumerator<float> RetreatAndPrepareForAdventureExit ()
    {
        messageBox.Step(BattleMessageType.Retreat);
        theater.Retreat();
        while (theater.processing) yield return 0f;
        if (GameDataManager.Instance.dataStore.adventureLevel >= AdventureSubstageLoader.randomAdventureBaseLevel && !playerParty[0].dead)
        {
            GameDataManager.Instance.dataStore.resBricks += baseEndlessAdventurePayout[0] / 2;
            GameDataManager.Instance.dataStore.resPlanks += baseEndlessAdventurePayout[1] / 2;
            GameDataManager.Instance.dataStore.resMetal += baseEndlessAdventurePayout[2] / 2;
        }
        yield return Timing.WaitForSeconds(battleStepLength * 3);
        battleEndPopup.Open();
    }

    IEnumerator<float> LoseAndPrepareForAdventureExit ()
    {
        messageBox.Step(BattleMessageType.Loss);
        yield return Timing.WaitUntilDone(theater.LoseBattle());
        while (theater.processing) yield return 0f;
        yield return Timing.WaitForSeconds(battleStepLength);
        screenChanger.Activate();
    }

    public void EndBattle ()
    {
        currentlyInFight = false;
        if (validEnemyTargets.Count == 0)
        {
            battleNo++;
            Timing.KillCoroutines(_owned);
            if (battleNo < adventure.Length) Timing.RunCoroutine(WinAndContinueBattling(), _owned);
            else Timing.RunCoroutine(WinAndPrepareForAdventureExit(), _owned);
        }
        else if (validPlayerTargets.Count == 0) Timing.RunCoroutine(LoseAndPrepareForAdventureExit(), _owned);
        else if (retreatingAtStartOfNextTurn) Timing.RunCoroutine(RetreatAndPrepareForAdventureExit(), _owned);
        else throw new System.Exception("Tried to end battle... but both parties are still capable of fighting, and the player isn't retreating???");
    }

    bool CheckIfBattleResolved ()
    {
        if (playerParty[0].dead) SetToRetreat(); // first slot is always the sovereign, so if first slot is dead sovereign is dead
        for (int i = 0; i < validEnemyTargets.Count; i++) if (validEnemyTargets[i].dead) validEnemyTargets.RemoveAt(i);
        for (int i = 0; i < validPlayerTargets.Count; i++) if (validPlayerTargets[i].dead) validPlayerTargets.RemoveAt(i);
        currentBattleResolved = (validEnemyTargets.Count < 1 || validPlayerTargets.Count < 1);
        return currentBattleResolved;
    }

    void SubTurnStep_StandardAction_Fetch (Battler bat)
    {
        standardPriorityActingBattler = bat;
        if (bat.isEnemy)
        {
            nextAction = bat.GetStandardAction(turn, validEnemyTargets, validPlayerTargets);
            bat.ReadyAttack(validPlayerTargets, validEnemyTargets, nextAction);
        }
        else
        {
            nextAction = bat.GetStandardAction(turn, validPlayerTargets, validEnemyTargets);
            bat.ReadyAttack(validEnemyTargets, validPlayerTargets, nextAction);
        }
    }

    bool SubTurnStep_Interrupt_BattleStart (Battler bat)
    {
        BattlerAction action;
        currentActingBattler = bat;
        if (bat.isEnemy)
        {
            action = bat.GetInterruptAction(BattlerActionInterruptType.BattleStart);
            if (action != BattlerAction.None) bat.AttackWith(validPlayerTargets, validEnemyTargets, action);
        }
        else
        {
            action = bat.GetInterruptAction(BattlerActionInterruptType.BattleStart);
            if (action != BattlerAction.None) bat.AttackWith(validEnemyTargets, validPlayerTargets, action);
        }
        return action != BattlerAction.None;     
    }

    bool SubTurnStep_Interrupt_Deathblow (Battler bat)
    {
        BattlerAction action;
        currentActingBattler = bat;
        if (bat.isEnemy)
        {
            action = bat.GetInterruptAction(BattlerActionInterruptType.OnAllyDeathblow);
            if (action != BattlerAction.None) bat.AttackWith(validPlayerTargets, validEnemyTargets, action);
        }
        else
        {
            action = bat.GetInterruptAction(BattlerActionInterruptType.OnAllyDeathblow);
            if (action != BattlerAction.None) bat.AttackWith(validEnemyTargets, validPlayerTargets, action);
        }
        return action != BattlerAction.None;
    }

    bool SubTurnStep_Interrupt_AllyHit (Battler bat, Battler[] bats)
    {
        BattlerAction action = BattlerAction.None;
        bool tryToCutIn = false;
        for (int i = 0; i < bats.Length; i++) if (bats[i].isValidTarget && bats[i].incomingHit && bats[i] != bat)
        {
            tryToCutIn = true;
            break;
        }
        if (tryToCutIn)
        {
            action = bat.GetInterruptAction(BattlerActionInterruptType.OnAllyHit);
            if (action != BattlerAction.None)
            {
                currentActingBattler = bat;
                if (bat.isEnemy) bat.AttackWith(validPlayerTargets, validEnemyTargets, action);
                else bat.AttackWith(validEnemyTargets, validPlayerTargets, action);
            }
        }
        return action != BattlerAction.None;
    }

    bool SubTurnStep_Interrupt_SelfHit(Battler bat)
    {
        BattlerAction action = BattlerAction.None;
        if (bat.incomingHit)
        {
            action = bat.GetInterruptAction(BattlerActionInterruptType.OnHit);
            if (action != BattlerAction.None)
            {
                currentActingBattler = bat;
                if (bat.isEnemy) bat.AttackWith(validPlayerTargets, validEnemyTargets, action);
                else bat.AttackWith(validEnemyTargets, validPlayerTargets, action);
            }
        }
        return action != BattlerAction.None;
    }

    IEnumerator<float> Bootstrap ()
    {
        while (GameDataManager.Instance.dataStore == null) yield return 0f;
        if (GameDataManager.Instance.dataStore.housingLevel < 2) tutorial = true;
        if (tutorial)
        {
            if (GameDataManager.Instance.dataStore.housingLevel == 0) adventure = AdventureSubstageLoader.adventureTuto0Substages;
            else adventure = AdventureSubstageLoader.adventureTuto1Substages;
        }
        else
        {
            if (GameDataManager.Instance.dataStore.adventureLevel < AdventureSubstageLoader.randomAdventureBaseLevel) adventure = AdventureSubstageLoader.prebuiltAdventures[GameDataManager.Instance.dataStore.adventureLevel];
            else adventure = AdventureSubstageLoader.randomAdventure;
        }

        PopulatePlayerParty();
        StartNextBattle();
    }

    private void BuildLists ()
    {
        turnOrderList.Clear();
        validEnemyTargets.Clear();
        validPlayerTargets.Clear();
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
        Adventurer[] enemyAdventurers;
        AdventureSubstage substage = adventure[battleNo];
        enemyAdventurers = new Adventurer[substage.enemiesClasses.Length];
        for (int i = 0; i < enemyAdventurers.Length; i++)
        {
            enemyAdventurers[i] = new Adventurer();
            int[] bonus;
            if (GameDataManager.Instance.dataStore.adventureLevel < AdventureSubstageLoader.randomAdventureBaseLevel) bonus = new int[] { 0, 0, 0, 0 };
            else
            {
                int a = GameDataManager.Instance.dataStore.adventureLevel - AdventureSubstageLoader.randomAdventureBaseLevel;
                bonus = new int[] { 0, a, a, a };
            }
            enemyAdventurers[i].Reroll(substage.enemiesClasses[i], substage.enemiesSpecies[i], substage.eliteStatuses[i], bonus);
        }
        int i2 = 0;
        for (int i = 0; i < enemyAdventurers.Length && i2 < enemyParty.Length; i++)
        {
            if (enemyAdventurers[i] != null)
            {
                enemyParty[i2].GenerateBattleData(enemyAdventurers[i]);
                i2++;
            }
        }
        while (i2 < enemyParty.Length)
        {
            enemyParty[i2].SetBattlerInactive();
            i2++;
        }
    }

    private void PopulatePlayerParty ()
    {
        Adventurer[] playerAdventurers = new Adventurer[] { GameDataManager.Instance.dataStore.sovereignAdventurer, GameDataManager.Instance.GetPartyMember(0),
                                                            GameDataManager.Instance.GetPartyMember(1), GameDataManager.Instance.GetPartyMember(2) };
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
        _forceAllBattlersRegen = false;
        Battler[] longAllBattlers = new Battler[playerParty.Length + enemyParty.Length];
        int i2 = 0;
        for (int i = 0; i < playerParty.Length; i++) if (playerParty[i].isValidTarget)
            {
                longAllBattlers[i2] = playerParty[i];
                i2++;
            }
        for (int i = 0; i < enemyParty.Length; i++) if (enemyParty[i].isValidTarget)
            {
                longAllBattlers[i2] = enemyParty[i];
                i2++;
            }
        allBattlers = new Battler[i2];
        for (int i = 0; i < allBattlers.Length; i++) allBattlers[i] = longAllBattlers[i];
    }

    private void StartNextBattle ()
    {
        currentlyInFight = true;
        currentBattleResolved = false;
        turn = 0;
        PopulateEnemyParty();
        RebuildAllBattlersArray();
        theater.StartBattle();
        Timing.RunCoroutine(RunTurn(), _owned);
    }

    private void StartTurn ()
    {
        for (int i = 0; i < allBattlers.Length; i++)
        {
            if (!allBattlers[i].isValidTarget || _forceAllBattlersRegen)
            {
                RebuildAllBattlersArray();
                break;
            }
        }
        for (int i = 0; i < playerParty.Length; i++) if (playerParty[i].existsInBattle) playerParty[i].Upkeep();
        for (int i = 0; i < enemyParty.Length; i++) if (enemyParty[i].existsInBattle) enemyParty[i].Upkeep();
        BuildLists();
    }

    private void EndTurn ()
    {
        if (outOfBattleBattler != null && outOfBattleBattler._wantsToReenterBattle)
        {
            outOfBattleBattler.ReenterBattle();
            _forceAllBattlersRegen = true;
        }
        turn++;
        encoreWaitingForEnemies = false;
        encoreWaitingForPlayer = false;
        nextAction = BattlerAction.None;
        currentActingBattler = default(Battler);
        currentTurnTarget = default(Battler);
        if (currentBattleResolved) EndBattle();
        else Timing.RunCoroutine(RunTurn(), _owned);
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

    public void SetToRetreat ()
    {
        retreatButton.SetActive(false);
        retreatingAtStartOfNextTurn = true;
    }

    private void Retreat ()
    {
        currentBattleResolved = true;
    }
}
