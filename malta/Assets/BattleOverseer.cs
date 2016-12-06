using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleOverseer : MonoBehaviour
{
    public Adventurer[][] enemyPartyConfigs;
    public Battler[] enemyParty;
    public Battler[] playerParty;
    public SortedList<float, Battler> turnOrderList;
    private Battler[] allBattlers;
    private int battleNo = 0;
    private int turn = 0;
    private int turnStep = 0;

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(Bootstrap());
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    IEnumerator Bootstrap ()
    {
        while (GameDataManager.Instance == null) yield return null;
        PopulatePlayerParty();
        StartNextBattle();
    }

    private void BuildTurnOrderList ()
    {
        turnOrderList = new SortedList<float, Battler>(allBattlers.Length);
        for (int i = 0; i < allBattlers.Length; i++) turnOrderList.Add(allBattlers[i].moveSpeed, allBattlers[i]);
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
        BuildTurnOrderList();
    }
}
