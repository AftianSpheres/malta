using UnityEngine;
using System.Collections;

/// <summary>
/// Manages persistent game state shit + timer-driven progression.
/// This shit does way too much; long-term I need to break it
/// into multiple managers.
/// </summary>
public class GameDataManager : Manager<GameDataManager>
{
    public Adventurer sovereignAdventurer;
    public Adventurer forgeAdventurer;
    public Adventurer[] houseAdventurers = new Adventurer[2];
    public AdventurerClass warriorClassUnlock { get; private set; }
    public AdventurerClass mysticClassUnlock { get; private set; }
    public AdventurerAttack sovereignTactic;
    public AdventurerSpecial sovereignSkill;
    public bool pendingUpgrade_ClayPit = false;
    public bool pendingUpgrade_Docks = false;
    public bool pendingUpgrade_Mason = false;
    public bool pendingUpgrade_Mine = false;
    public bool pendingUpgrade_Sawmill = false;
    public bool pendingUpgrade_Smith = false;
    public bool pendingUpgrade_Woodlands = false;
    public bool unlock_raceFae = false;
    public bool unlock_raceOrc = false;
    public bool unlock_Taskmaster = false;
    public int buildingLv_Docks = 0;
    public int buildingLv_Mason = 0;
    public int buildingLv_Sawmill = 0;
    public int buildingLv_Smith = 0;
    public int harvestLv_ClayPit = 1;
    public int harvestLv_Mine = 1;
    public int harvestLv_Woodlands = 1;
    public int resBricks = 0;
    public int resBricks_max;
    public int resBricks_maxUpgrades = 0;
    public int resClay = 0;
    public int resClay_max;
    public int resClay_maxUpgrades = 0;
    public int resLumber = 0;
    public int resLumber_max;
    public int resLumber_maxUpgrades = 0;
    public int resMetal = 0;
    public int resMetal_max;
    public int resMetal_maxUpgrades = 0;
    public int resOre = 0;
    public int resOre_max;
    public int resOre_maxUpgrades = 0;
    public int resPlanks = 0;
    public int resPlanks_max;
    public int resPlanks_maxUpgrades = 0;
    private float lastSecondTimestamp;
    private int resourceGainTimer = 0;
    private const int resourceGainThreshold = 59;
    private const int resourceMaximumsBaseValue= 600; // ten hours
    private const int pendingUpgradeBaseTime = 300; // five minutes;
    private int pendingUpgradeTimer_ClayPit = 0;
    private int pendingUpgradeTimer_Docks = 0;
    private int pendingUpgradeTimer_Mason = 0;
    private int pendingUpgradeTimer_Mine = 0;
    private int pendingUpgradeTimer_Sawmill = 0;
    private int pendingUpgradeTimer_Smith = 0;
    private int pendingUpgradeTimer_Woodlands = 0;
    public bool[] housesBuilt = { true, false };
    public bool[] housesOutbuildingsBuilt = { false, false };

    void Start ()
    {
        sovereignTactic = AdventurerAttack.GetBehindMe;
        sovereignSkill = AdventurerSpecial.Protect;
        warriorClassUnlock = AdventurerClass.Warrior;
        mysticClassUnlock = AdventurerClass.Mystic;
        sovereignAdventurer = ScriptableObject.CreateInstance<Adventurer>();
        sovereignAdventurer.Reroll(AdventurerClass.Sovereign, AdventurerSpecies.Human, false, new int[] { 0, 0, 0, 0 });
        lastSecondTimestamp = Time.time;
        RecalculateResourceMaximums();
        if (Application.isEditor) Time.timeScale = 6.0f; // speed things up for in-editor testing
    }

    void Update ()
    {
        if (Time.time - lastSecondTimestamp >= 1.0f)
        {
            lastSecondTimestamp = Time.time;
            UpdateProcessing_ResourceGain();
            if (unlock_Taskmaster) UpdateProcessing_ResourceGain(); // output doubled in the quickest, laziest way possible
            HandlePendingUpgrade(ref pendingUpgradeTimer_ClayPit, ref harvestLv_ClayPit, ref pendingUpgrade_ClayPit);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Docks, ref buildingLv_Docks, ref pendingUpgrade_Docks);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Mason, ref buildingLv_Mason, ref pendingUpgrade_Mason);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Mine, ref harvestLv_Mine, ref pendingUpgrade_Mine);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Sawmill, ref buildingLv_Sawmill, ref pendingUpgrade_Sawmill);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Smith, ref buildingLv_Smith, ref pendingUpgrade_Smith);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Woodlands, ref harvestLv_Woodlands, ref pendingUpgrade_Woodlands);
        }

    }

    void HandlePendingUpgrade(ref int pendingUpgradeTimer, ref int buildingLv, ref bool pendingUpgrade)
    {
        if (pendingUpgradeTimer> 0) pendingUpgradeTimer -= buildingLv_Docks + 1; // spec didn't indicate exactly how docks work afaik, but linear scaling seems less broken than exponential
        else if (pendingUpgrade)
        {
            buildingLv++;
            pendingUpgrade = false;
        }
    }

    void RecalculateResourceMaximums ()
    {
        _in_RecalculateResourceMaximums(ref resClay_max, harvestLv_ClayPit - 1, resClay_maxUpgrades);
        _in_RecalculateResourceMaximums(ref resLumber_max, harvestLv_Woodlands - 1, resLumber_maxUpgrades);
        _in_RecalculateResourceMaximums(ref resOre_max, harvestLv_Mine - 1, resOre_maxUpgrades);
        _in_RecalculateResourceMaximums(ref resBricks_max, buildingLv_Mason, resBricks_maxUpgrades);
        _in_RecalculateResourceMaximums(ref resMetal_max, buildingLv_Smith, resMetal_maxUpgrades);
        _in_RecalculateResourceMaximums(ref resPlanks_max, buildingLv_Sawmill, resPlanks_maxUpgrades);
    }

    void _in_RecalculateResourceMaximums(ref int max, int structureLevel, int maxUpgrades)
    {
        if (structureLevel > 0) max = Mathf.FloorToInt(Mathf.Pow(2, structureLevel)) * resourceMaximumsBaseValue;
        else max = resourceMaximumsBaseValue;
        for (int i = 0; i < maxUpgrades; i++) max *= 4;
    }

    void UpdateProcessing_ResourceGain ()
    {
        resourceGainTimer++;
        if (resourceGainTimer >= resourceGainThreshold)
        {
            resourceGainTimer = 0;
            _in_ResourceGain(ref resClay, harvestLv_ClayPit - 1, resClay_max, 1);
            _in_ResourceGain(ref resLumber, harvestLv_Woodlands - 1, resLumber_max, 1);
            _in_ResourceGain(ref resOre, harvestLv_Mine - 1, resOre_max, 1);
            _in_ResourceGain(ref resBricks, buildingLv_Mason, resBricks_max, 4);
            _in_ResourceGain(ref resMetal, buildingLv_Smith, resMetal_max, 4);
            _in_ResourceGain(ref resPlanks, buildingLv_Sawmill, resPlanks_max, 4);
        }
    }

    void _in_ResourceGain(ref int res, int structureLevel, int max, int baseGain)
    {
        if (structureLevel > 0) res += baseGain * Mathf.FloorToInt(Mathf.Pow(2, (structureLevel)));
        else res += baseGain;
        if (res > max) res = max;
    }

    public void SetBuildingUpgradePending(BuildingType building)
    {
        switch (building)
        {
            case BuildingType.Docks:
                _in_SetBuildingUpgradePending(ref pendingUpgrade_Docks, ref pendingUpgradeTimer_Docks, buildingLv_Docks + 1);
                break;
            case BuildingType.Smith:
                _in_SetBuildingUpgradePending(ref pendingUpgrade_Smith, ref pendingUpgradeTimer_Smith, buildingLv_Smith + 1);
                break;
            case BuildingType.Sawmill:
                _in_SetBuildingUpgradePending(ref pendingUpgrade_Sawmill, ref pendingUpgradeTimer_Sawmill, buildingLv_Sawmill + 1);
                break;
            case BuildingType.Mason:
                _in_SetBuildingUpgradePending(ref pendingUpgrade_Mason, ref pendingUpgradeTimer_Mason, buildingLv_Mason + 1);
                break;
            case BuildingType.ClayPit:
                _in_SetBuildingUpgradePending(ref pendingUpgrade_ClayPit, ref pendingUpgradeTimer_ClayPit, harvestLv_ClayPit);
                break;
            case BuildingType.Mine:
                _in_SetBuildingUpgradePending(ref pendingUpgrade_Mine, ref pendingUpgradeTimer_Mine, harvestLv_Mine);
                break;
            case BuildingType.Woodlands:
                _in_SetBuildingUpgradePending(ref pendingUpgrade_Woodlands, ref pendingUpgradeTimer_Woodlands, harvestLv_Woodlands);
                break;
            default:
                throw new System.Exception("Tried to level up un-level-able building of type: " + building.ToString());
        }
    }

    void _in_SetBuildingUpgradePending(ref bool pendingUpgrade, ref int pendingUpgradeTimer, int buildingLv)
    {
        pendingUpgrade = true;
        pendingUpgradeTimer = pendingUpgradeBaseTime * buildingLv;
    }
}
