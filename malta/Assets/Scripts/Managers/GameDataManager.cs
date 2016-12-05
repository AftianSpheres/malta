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
    public AdventurerAttack sovereignTactic { get; private set; }
    public AdventurerSpecial sovereignSkill { get; private set; }
    public string sovereignFirstName = "Dude";
    public string sovereignLastName = "Huge";
    public string sovereignName { get { return sovereignAdventurer.fullName; } }
    public string yourTownName = "Citysburg";
    public string[] fakeTownNames = { "Town A", "Town No. 2", "Tertiary Town" };
    public bool pendingUpgrade_ClayPit = false;
    public bool pendingUpgrade_Docks = false;
    public bool pendingUpgrade_Mason = false;
    public bool pendingUpgrade_Mine = false;
    public bool pendingUpgrade_Sawmill = false;
    public bool pendingUpgrade_Smith = false;
    public bool pendingUpgrade_Woodlands = false;
    public bool unlock_forgeOutbuilding = false;
    public bool unlock_raceFae = false;
    public bool unlock_raceOrc = false;
    public bool unlock_sovSpe_CalledShots = false;
    public bool unlock_sovSpe_HammerSmash = false;
    public bool unlock_sovSpe_Protect = false;
    public bool unlock_Taskmaster = false;
    public bool unlock_WizardsTower = false;
    public int adventureLevel = 0;
    public int buildingLv_Docks = 0;
    public int buildingLv_Mason = 0;
    public int buildingLv_Sawmill = 0;
    public int buildingLv_Smith = 0;
    public int buildingLv_WizardsTower = 1;
    public int harvestLv_ClayPit = 1;
    public int harvestLv_Mine = 1;
    public int harvestLv_Woodlands = 1;
    public int nextRandomAdventureAnte = 2;
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
    public int pendingUpgradeTimer_ClayPit = 0;
    public int pendingUpgradeTimer_Docks = 0;
    public int pendingUpgradeTimer_Mason = 0;
    public int pendingUpgradeTimer_Mine = 0;
    public int pendingUpgradeTimer_Sawmill = 0;
    public int pendingUpgradeTimer_Smith = 0;
    public int pendingUpgradeTimer_Woodlands = 0;
    public bool[] housesBuilt = { true, false };
    public bool[] housesOutbuildingsBuilt = { false, false };

    void Start ()
    {
        sovereignTactic = AdventurerAttack.GetBehindMe;
        sovereignSkill = AdventurerSpecial.None;
        warriorClassUnlock = AdventurerClass.Warrior;
        mysticClassUnlock = AdventurerClass.Mystic;
        sovereignAdventurer = ScriptableObject.CreateInstance<Adventurer>();
        sovereignAdventurer.Reroll(AdventurerClass.Sovereign, AdventurerSpecies.Human, false, new int[] { 0, 0, 0, 0 });
        lastSecondTimestamp = Time.time;
        RecalculateResourceMaximums();
    }

    void Update ()
    {
        if (Input.GetKey(KeyCode.Pause)) Time.timeScale = 100.0f; // speed things up for testing
        else Time.timeScale = 1.0f;
        if (Time.time - lastSecondTimestamp >= 1.0f)
        {
            lastSecondTimestamp = Time.time;
            UpdateProcessing_ResourceGain();
            if (unlock_Taskmaster) UpdateProcessing_ResourceGain(); // output doubled in the quickest, laziest way possible
            HandlePendingUpgrade(ref pendingUpgradeTimer_ClayPit, ref harvestLv_ClayPit, ref pendingUpgrade_ClayPit, ref resClay_maxUpgrades);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Docks, ref buildingLv_Docks, ref pendingUpgrade_Docks, ref pendingUpgradeTimer_Docks); // this is a hack
            HandlePendingUpgrade(ref pendingUpgradeTimer_Mason, ref buildingLv_Mason, ref pendingUpgrade_Mason, ref resBricks_maxUpgrades);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Mine, ref harvestLv_Mine, ref pendingUpgrade_Mine, ref resOre_maxUpgrades);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Sawmill, ref buildingLv_Sawmill, ref pendingUpgrade_Sawmill, ref resPlanks_maxUpgrades);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Smith, ref buildingLv_Smith, ref pendingUpgrade_Smith, ref resMetal_maxUpgrades);
            HandlePendingUpgrade(ref pendingUpgradeTimer_Woodlands, ref harvestLv_Woodlands, ref pendingUpgrade_Woodlands, ref resLumber_maxUpgrades);
        }
        for (int i = 0; i < housesBuilt.Length; i++)
        {
            if (housesBuilt[i] && houseAdventurers[i] == null)
            {
                houseAdventurers[i] = ScriptableObject.CreateInstance<Adventurer>();
                houseAdventurers[i].Reroll(AdventurerClass.Warrior, AdventurerSpecies.Human, housesOutbuildingsBuilt[i], new int[] { 0, 0, 0, 0 });
            }
        }
        if (unlock_forgeOutbuilding && !unlock_Taskmaster && forgeAdventurer != null)
        {
            forgeAdventurer = ScriptableObject.CreateInstance<Adventurer>();
            forgeAdventurer.Reroll(AdventurerClass.Warrior, AdventurerSpecies.Human, false, new int[] { 0, 0, 0, 0 });
        }

    }

    void HandlePendingUpgrade (ref int pendingUpgradeTimer, ref int buildingLv, ref bool pendingUpgrade, ref int capUpgrades)
    {
        if (pendingUpgradeTimer> 0) pendingUpgradeTimer -= buildingLv_Docks + 1; // spec didn't indicate exactly how docks work afaik, but linear scaling seems less broken than exponential
        else if (pendingUpgrade)
        {
            buildingLv++;
            pendingUpgradeTimer = 0;
            capUpgrades = 0;
            pendingUpgrade = false;
            RecalculateResourceMaximums();
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

    void _in_RecalculateResourceMaximums (ref int max, int structureLevel, int maxUpgrades)
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

    void _in_ResourceGain (ref int res, int structureLevel, int max, int baseGain)
    {
        res += GetResourceGainRate(structureLevel, baseGain);
        if (res > max) res = max;
    }

    public int GetResourceGainRate(int structureLevel, int baseGain)
    {
        int gain = baseGain;
        if (structureLevel > 0) gain = baseGain * Mathf.FloorToInt(Mathf.Pow(2, (structureLevel)));
        if (unlock_WizardsTower) gain += Mathf.RoundToInt(gain * 0.1f * buildingLv_WizardsTower);
        return gain;
    }

    public int GetResourceGainRate(ResourceType resource)
    {
        int gain = -1;
        switch (resource)
        {
            case ResourceType.Bricks:
                gain = GetResourceGainRate(buildingLv_Mason, 4);
                break;
            case ResourceType.Clay:
                gain = GetResourceGainRate(harvestLv_ClayPit - 1, 1);
                break;
            case ResourceType.Metal:
                gain = GetResourceGainRate(buildingLv_Smith, 4);
                break;
            case ResourceType.Ore:
                gain = GetResourceGainRate(harvestLv_Mine - 1, 1);
                break;
            case ResourceType.Planks:
                gain = GetResourceGainRate(buildingLv_Sawmill, 4);
                break;
            case ResourceType.Lumber:
                gain = GetResourceGainRate(harvestLv_Woodlands - 1, 1);
                break;
        }
        return gain;
    }

    public void PromoteMysticsTo (AdventurerClass advClass)
    {
        for (int i = 0; i < houseAdventurers.Length; i++)
        {
            if (houseAdventurers[i] != null && houseAdventurers[i].advClass == mysticClassUnlock) houseAdventurers[i].advClass = advClass;
        }
        if (forgeAdventurer != null && forgeAdventurer.advClass == mysticClassUnlock) forgeAdventurer.advClass = advClass;
        mysticClassUnlock = advClass;
    }

    public void PromoteWarriorsTo (AdventurerClass advClass)
    {
        for (int i = 0; i < houseAdventurers.Length; i++)
        {
            if (houseAdventurers[i] != null && houseAdventurers[i].advClass == warriorClassUnlock) houseAdventurers[i].advClass = advClass;
        }
        if (forgeAdventurer != null && forgeAdventurer.advClass == warriorClassUnlock) forgeAdventurer.advClass = advClass;
        warriorClassUnlock = advClass;
    }

    public void SetBuildingUpgradePending (BuildingType building)
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

    public void SetSovereignSpecial (AdventurerSpecial special)
    {
        sovereignSkill = special;
        sovereignAdventurer.special = special;
    }

    public void SetSovereignTactic (AdventurerAttack attack)
    {
        sovereignTactic = attack;
        sovereignAdventurer.attacks[1] = attack;
    }

    public bool CheckMaterialAvailability (int[] resourceNums)
    {
        return CheckMaterialAvailability(resourceNums[0], resourceNums[1], resourceNums[2], resourceNums[3], resourceNums[4], resourceNums[5]);
    }

    public bool CheckMaterialAvailability(int numClay, int numLumber, int numOre, int numBrick, int numPlanks, int numMetal)
    {
        bool result = false;
        if (resClay >= numClay && resLumber >= numLumber && resOre >= numOre && resBricks >= numBrick && resPlanks >= numPlanks && resMetal >= numMetal) result = true;
        return result;
    }

    public bool SpendResourcesIfPossible (int[] resourceNums)
    {
        return SpendResourcesIfPossible(resourceNums[0], resourceNums[1], resourceNums[2], resourceNums[3], resourceNums[4], resourceNums[5]);
    }

    public bool SpendResourcesIfPossible (int numClay, int numLumber, int numOre, int numBrick, int numPlanks, int numMetal)
    {
        bool result = false;
        if (CheckMaterialAvailability(numClay, numLumber, numOre, numBrick, numPlanks, numMetal))
        {
            resClay -= numClay;
            resLumber -= numLumber;
            resOre -= numOre;
            resBricks -= numBrick;
            resPlanks -= numPlanks;
            resMetal -= numMetal;
            result = true;
        }
        return result;
    }

    public void UpgradeResourceCap (ResourceType resource)
    {
        switch (resource)
        {
            case ResourceType.Bricks:
                resBricks_maxUpgrades++;
                break;
            case ResourceType.Metal:
                resMetal_maxUpgrades++;
                break;
            case ResourceType.Planks:
                resPlanks_maxUpgrades++;
                break;
            case ResourceType.Clay:
                resClay_maxUpgrades++;
                break;
            case ResourceType.Ore:
                resOre_maxUpgrades++;
                break;
            case ResourceType.Lumber:
                resLumber_maxUpgrades++;
                break;
        }
        RecalculateResourceMaximums();
    }

    void _in_SetBuildingUpgradePending (ref bool pendingUpgrade, ref int pendingUpgradeTimer, int buildingLv)
    {
        pendingUpgrade = true;
        pendingUpgradeTimer = pendingUpgradeBaseTime * buildingLv;
    }


}
