using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class GameDataManager_DataStore
{
    public Adventurer sovereignAdventurer;
    public Adventurer forgeAdventurer;
    public Adventurer[] houseAdventurers;
    public AdventurerClass warriorClassUnlock;
    public AdventurerClass mysticClassUnlock;
    public AdventurerAttack sovereignTactic;
    public AdventurerSpecial sovereignSkill;
    public AdventurerMugshot sovereignMugshot;
    public bool[] housesBuilt;
    public bool[] housesOutbuildingsBuilt;
    public string sovereignFirstName;
    public string sovereignLastName;
    public string sovereignName { get { return sovereignAdventurer.fullName; } }
    public string yourTownName;
    public string[] fakeTownNames;
    public bool pendingUpgrade_ClayPit;
    public bool pendingUpgrade_Docks;
    public bool pendingUpgrade_Mason;
    public bool pendingUpgrade_Mine;
    public bool pendingUpgrade_Sawmill;
    public bool pendingUpgrade_Smith;
    public bool pendingUpgrade_Woodlands;
    public bool unlock_forgeOutbuilding;
    public bool unlock_raceFae;
    public bool unlock_raceOrc;
    public bool unlock_sovSpe_CalledShots;
    public bool unlock_sovSpe_HammerSmash;
    public bool unlock_sovSpe_Protect;
    public bool unlock_Taskmaster;
    public bool unlock_WizardsTower;
    public int adventureLevel;
    public int buildingLv_Docks;
    public int buildingLv_Mason;
    public int buildingLv_Sawmill;
    public int buildingLv_Smith;
    public int buildingLv_WizardsTower;
    public int harvestLv_ClayPit;
    public int harvestLv_Mine;
    public int harvestLv_Woodlands;
    public int nextRandomAdventureAnte;
    public int resBricks;
    public int resBricks_max;
    public int resBricks_maxUpgrades;
    public int resClay;
    public int resClay_max;
    public int resClay_maxUpgrades;
    public int resLumber;
    public int resLumber_max;
    public int resLumber_maxUpgrades;
    public int resMetal;
    public int resMetal_max;
    public int resMetal_maxUpgrades;
    public int resOre;
    public int resOre_max;
    public int resOre_maxUpgrades;
    public int resPlanks;
    public int resPlanks_max;
    public int resPlanks_maxUpgrades;
    public int pendingUpgradeTimer_ClayPit;
    public int pendingUpgradeTimer_Docks;
    public int pendingUpgradeTimer_Mason;
    public int pendingUpgradeTimer_Mine;
    public int pendingUpgradeTimer_Sawmill;
    public int pendingUpgradeTimer_Smith;
    public int pendingUpgradeTimer_Woodlands;

    public GameDataManager_DataStore ()
    {
        yourTownName = "Citysburg";
        fakeTownNames = new string[] { "Town A", "Town No. 2", "Tertiary Town" };
        pendingUpgrade_ClayPit = false;
        pendingUpgrade_Docks = false;
        pendingUpgrade_Mason = false;
        pendingUpgrade_Mine = false;
        pendingUpgrade_Sawmill = false;
        pendingUpgrade_Smith = false;
        pendingUpgrade_Woodlands = false;
        unlock_forgeOutbuilding = false;
        unlock_raceFae = false;
        unlock_raceOrc = false;
        unlock_sovSpe_CalledShots = false;
        unlock_sovSpe_HammerSmash = false;
        unlock_sovSpe_Protect = false;
        unlock_Taskmaster = false;
        unlock_WizardsTower = false;
        adventureLevel = 0;
        buildingLv_Docks = 0;
        buildingLv_Mason = 0;
        buildingLv_Sawmill = 0;
        buildingLv_Smith = 0;
        buildingLv_WizardsTower = 1;
        harvestLv_ClayPit = 1;
        harvestLv_Mine = 1;
        harvestLv_Woodlands = 1;
        nextRandomAdventureAnte = 2;
        resBricks = 0;
        resBricks_max = 999; // we actually recalc these immediately, lol
        resBricks_maxUpgrades = 0;
        resClay = 0;
        resClay_max = 999;
        resClay_maxUpgrades = 0;
        resLumber = 0;
        resLumber_max = 999;
        resLumber_maxUpgrades = 0;
        resMetal = 0;
        resMetal_max = 999;
        resMetal_maxUpgrades = 0;
        resOre = 0;
        resOre_max = 999;
        resOre_maxUpgrades = 0;
        resPlanks = 0;
        resPlanks_max = 999;
        resPlanks_maxUpgrades = 0;
        pendingUpgradeTimer_ClayPit = 0;
        pendingUpgradeTimer_Docks = 0;
        pendingUpgradeTimer_Mason = 0;
        pendingUpgradeTimer_Mine = 0;
        pendingUpgradeTimer_Sawmill = 0;
        pendingUpgradeTimer_Smith = 0;
        pendingUpgradeTimer_Woodlands = 0;
        sovereignFirstName = "Dude";
        sovereignLastName = "Huge";
        housesBuilt = new bool[] { true, false };
        housesOutbuildingsBuilt = new bool[] { false, false };
        houseAdventurers = new Adventurer[2];
        sovereignAdventurer = new Adventurer();
        forgeAdventurer = new Adventurer();
        warriorClassUnlock = AdventurerClass.Warrior;
        mysticClassUnlock = AdventurerClass.Mystic;
        for (int i = 0; i < housesBuilt.Length; i++)
        {
            houseAdventurers[i] = new Adventurer();
        }
        sovereignTactic = AdventurerAttack.GetBehindMe;
        sovereignSkill = AdventurerSpecial.None;
        sovereignMugshot = AdventurerMugshot.Sovereign0;
    }

    public void SaveToFile(string path)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream f = File.Create(path);
        formatter.Serialize(f, this);
        f.Close();
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Application.ExternalCall("sync");
        }
    }
}

/// <summary>
/// Manages persistent game state shit + timer-driven progression.
/// This shit does way too much; long-term I need to break it
/// into multiple managers.
/// </summary>
/// 
public class GameDataManager : Manager<GameDataManager>
{
    public GameDataManager_DataStore dataStore;
    private float lastSecondTimestamp;
    private int resourceGainTimer = 0;
    private const int resourceGainThreshold = 59;
    private const int resourceMaximumsBaseValue= 600; // ten hours
    private const int pendingUpgradeBaseTime = 300; // five minutes;
    private const string saveName = "/save.bin";

    void Start ()
    {
        lastSecondTimestamp = Time.time;

        if (File.Exists(Application.persistentDataPath + saveName))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream f = File.OpenRead(Application.persistentDataPath + saveName);
            try
            {
                dataStore = formatter.Deserialize(f) as GameDataManager_DataStore;
            }
            catch (System.Exception)
            {
                dataStore = default(GameDataManager_DataStore); // if we throw an exception trying to open the save it doesn't really matter what that is - no recovering from it, so just pretend the save wasn't even there
            }
        }
        if (dataStore == null) RegenerateDataStore();

    }

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.F12)) dataStore.SaveToFile(Application.persistentDataPath + saveName);     
        if (Input.GetKey(KeyCode.Pause))
        {
            Time.timeScale = 100.0f; // speed things up for testing
        }
        else Time.timeScale = 1.0f;
        if (Input.GetKey(KeyCode.Backslash))
        {
            dataStore.resBricks += 30;
            dataStore.resClay += 30;
            dataStore.resLumber += 30;
            dataStore.resOre += 30;
            dataStore.resMetal += 30;
            dataStore.resPlanks += 30;
        }
        if (Time.time - lastSecondTimestamp >= 1.0f)
        {
            lastSecondTimestamp = Time.time;
            UpdateProcessing_ResourceGain();
            if (dataStore.unlock_Taskmaster) UpdateProcessing_ResourceGain(); // output doubled in the quickest, laziest way possible
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_ClayPit, ref dataStore.harvestLv_ClayPit, ref dataStore.pendingUpgrade_ClayPit, ref dataStore.resClay_maxUpgrades);
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Docks, ref dataStore.buildingLv_Docks, ref dataStore.pendingUpgrade_Docks, ref dataStore.pendingUpgradeTimer_Docks); // this is a hack
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Mason, ref dataStore.buildingLv_Mason, ref dataStore.pendingUpgrade_Mason, ref dataStore.resBricks_maxUpgrades);
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Mine, ref dataStore.harvestLv_Mine, ref dataStore.pendingUpgrade_Mine, ref dataStore.resOre_maxUpgrades);
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Sawmill, ref dataStore.buildingLv_Sawmill, ref dataStore.pendingUpgrade_Sawmill, ref dataStore.resPlanks_maxUpgrades);
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Smith, ref dataStore.buildingLv_Smith, ref dataStore.pendingUpgrade_Smith, ref dataStore.resMetal_maxUpgrades);
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Woodlands, ref dataStore.harvestLv_Woodlands, ref dataStore.pendingUpgrade_Woodlands, ref dataStore.resLumber_maxUpgrades);
        }
        for (int i = 0; i < dataStore.housesBuilt.Length; i++) if (dataStore.housesBuilt[i] && !dataStore.houseAdventurers[i].initialized) dataStore.houseAdventurers[i].Reroll(dataStore.warriorClassUnlock, AdventurerSpecies.Human, dataStore.housesOutbuildingsBuilt[i], new int[] { 0, 0, 0, 0 });
        if (dataStore.unlock_forgeOutbuilding && !dataStore.unlock_Taskmaster && !dataStore.forgeAdventurer.initialized) dataStore.forgeAdventurer.Reroll(dataStore.warriorClassUnlock, AdventurerSpecies.Human, false, new int[] { 0, 0, 0, 0 });
        if (dataStore.adventureLevel > 1 && (!dataStore.unlock_raceFae || !dataStore.unlock_raceOrc))
        {
            dataStore.unlock_raceFae = true;
            dataStore.unlock_raceOrc = true;
        }
        if (dataStore.adventureLevel > 2 && !dataStore.unlock_WizardsTower) dataStore.unlock_WizardsTower = true;

    }

    public void RegenerateDataStore()
    {
        dataStore = new GameDataManager_DataStore();
        RecalculateResourceMaximums();
        for (int i = 0; i < dataStore.housesBuilt.Length; i++)
        {
            if (dataStore.housesBuilt[i])
            {
                dataStore.houseAdventurers[i].Reroll(dataStore.warriorClassUnlock, AdventurerSpecies.Human, dataStore.housesOutbuildingsBuilt[i], new int[] { 0, 0, 0, 0 });
            }
        }
        dataStore.sovereignMugshot = (AdventurerMugshot)Random.Range((int)AdventurerMugshot.Sovereign0, (int)AdventurerMugshot.Sovereign7 + 1);
        dataStore.sovereignAdventurer.Reroll(AdventurerClass.Sovereign, AdventurerSpecies.Human, false, new int[] { 0, 0, 0, 0 });
    }

    void HandlePendingUpgrade (ref int pendingUpgradeTimer, ref int buildingLv, ref bool pendingUpgrade, ref int capUpgrades)
    {
        if (pendingUpgradeTimer> 0) pendingUpgradeTimer -= dataStore.buildingLv_Docks + 1; // spec didn't indicate exactly how docks work afaik, but linear scaling seems less broken than exponential
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
        _in_RecalculateResourceMaximums(ref dataStore.resClay_max, dataStore.harvestLv_ClayPit - 1, dataStore.resClay_maxUpgrades);
        _in_RecalculateResourceMaximums(ref dataStore.resLumber_max, dataStore.harvestLv_Woodlands - 1, dataStore.resLumber_maxUpgrades);
        _in_RecalculateResourceMaximums(ref dataStore.resOre_max, dataStore.harvestLv_Mine - 1, dataStore.resOre_maxUpgrades);
        _in_RecalculateResourceMaximums(ref dataStore.resBricks_max, dataStore.buildingLv_Mason, dataStore.resBricks_maxUpgrades);
        _in_RecalculateResourceMaximums(ref dataStore.resMetal_max, dataStore.buildingLv_Smith, dataStore.resMetal_maxUpgrades);
        _in_RecalculateResourceMaximums(ref dataStore.resPlanks_max, dataStore.buildingLv_Sawmill, dataStore.resPlanks_maxUpgrades);
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
            _in_ResourceGain(ref dataStore.resClay, dataStore.harvestLv_ClayPit - 1, dataStore.resClay_max, 1);
            _in_ResourceGain(ref dataStore.resLumber, dataStore.harvestLv_Woodlands - 1, dataStore.resLumber_max, 1);
            _in_ResourceGain(ref dataStore.resOre, dataStore.harvestLv_Mine - 1, dataStore.resOre_max, 1);
            _in_ResourceGain(ref dataStore.resBricks, dataStore.buildingLv_Mason, dataStore.resBricks_max, 4);
            _in_ResourceGain(ref dataStore.resMetal, dataStore.buildingLv_Smith, dataStore.resMetal_max, 4);
            _in_ResourceGain(ref dataStore.resPlanks, dataStore.buildingLv_Sawmill, dataStore.resPlanks_max, 4);
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
        if (dataStore.unlock_WizardsTower) gain += Mathf.RoundToInt(gain * 0.1f * dataStore.buildingLv_WizardsTower);
        return gain;
    }

    public int GetResourceGainRate(ResourceType resource)
    {
        int gain = -1;
        switch (resource)
        {
            case ResourceType.Bricks:
                gain = GetResourceGainRate(dataStore.buildingLv_Mason, 4);
                break;
            case ResourceType.Clay:
                gain = GetResourceGainRate(dataStore.harvestLv_ClayPit - 1, 1);
                break;
            case ResourceType.Metal:
                gain = GetResourceGainRate(dataStore.buildingLv_Smith, 4);
                break;
            case ResourceType.Ore:
                gain = GetResourceGainRate(dataStore.harvestLv_Mine - 1, 1);
                break;
            case ResourceType.Planks:
                gain = GetResourceGainRate(dataStore.buildingLv_Sawmill, 4);
                break;
            case ResourceType.Lumber:
                gain = GetResourceGainRate(dataStore.harvestLv_Woodlands - 1, 1);
                break;
        }
        return gain;
    }

    public void PromoteMysticsTo (AdventurerClass advClass)
    {
        for (int i = 0; i < dataStore.houseAdventurers.Length; i++)
        {
            if (dataStore.houseAdventurers[i] != null && dataStore.houseAdventurers[i].advClass == dataStore.mysticClassUnlock) dataStore.houseAdventurers[i].Reclass(advClass);
        }
        if (dataStore.forgeAdventurer != null && dataStore.forgeAdventurer.advClass == dataStore.mysticClassUnlock) dataStore.forgeAdventurer.Reclass(advClass);
        dataStore.mysticClassUnlock = advClass;
    }

    public void PromoteWarriorsTo (AdventurerClass advClass)
    {
        for (int i = 0; i < dataStore.houseAdventurers.Length; i++)
        {
            if (dataStore.houseAdventurers[i] != null && dataStore.houseAdventurers[i].advClass == dataStore.warriorClassUnlock) dataStore.houseAdventurers[i].Reclass(advClass);
        }
        if (dataStore.forgeAdventurer != null && dataStore.forgeAdventurer.advClass == dataStore.warriorClassUnlock) dataStore.forgeAdventurer.Reclass(advClass);
        dataStore.warriorClassUnlock = advClass;
    }

    public void SetBuildingUpgradePending (BuildingType building)
    {
        switch (building)
        {
            case BuildingType.Docks:
                _in_SetBuildingUpgradePending(ref dataStore.pendingUpgrade_Docks, ref dataStore.pendingUpgradeTimer_Docks, dataStore.buildingLv_Docks + 1);
                break;
            case BuildingType.Smith:
                _in_SetBuildingUpgradePending(ref dataStore.pendingUpgrade_Smith, ref dataStore.pendingUpgradeTimer_Smith, dataStore.buildingLv_Smith + 1);
                break;
            case BuildingType.Sawmill:
                _in_SetBuildingUpgradePending(ref dataStore.pendingUpgrade_Sawmill, ref dataStore.pendingUpgradeTimer_Sawmill, dataStore.buildingLv_Sawmill + 1);
                break;
            case BuildingType.Mason:
                _in_SetBuildingUpgradePending(ref dataStore.pendingUpgrade_Mason, ref dataStore.pendingUpgradeTimer_Mason, dataStore.buildingLv_Mason + 1);
                break;
            case BuildingType.ClayPit:
                _in_SetBuildingUpgradePending(ref dataStore.pendingUpgrade_ClayPit, ref dataStore.pendingUpgradeTimer_ClayPit, dataStore.harvestLv_ClayPit);
                break;
            case BuildingType.Mine:
                _in_SetBuildingUpgradePending(ref dataStore.pendingUpgrade_Mine, ref dataStore.pendingUpgradeTimer_Mine, dataStore.harvestLv_Mine);
                break;
            case BuildingType.Woodlands:
                _in_SetBuildingUpgradePending(ref dataStore.pendingUpgrade_Woodlands, ref dataStore.pendingUpgradeTimer_Woodlands, dataStore.harvestLv_Woodlands);
                break;
            default:
                throw new System.Exception("Tried to level up un-level-able building of type: " + building.ToString());
        }
    }

    public void SetSovereignSpecial (AdventurerSpecial special)
    {
        dataStore.sovereignSkill = special;
        dataStore.sovereignAdventurer.special = special;
    }

    public void SetSovereignTactic (AdventurerAttack attack)
    {
        dataStore.sovereignTactic = attack;
        dataStore.sovereignAdventurer.attacks[1] = attack;
    }

    public bool CheckMaterialAvailability (int[] resourceNums)
    {
        return CheckMaterialAvailability(resourceNums[0], resourceNums[1], resourceNums[2], resourceNums[3], resourceNums[4], resourceNums[5]);
    }

    public bool CheckMaterialAvailability(int numClay, int numLumber, int numOre, int numBrick, int numPlanks, int numMetal)
    {
        bool result = false;
        if (dataStore.resClay >= numClay && dataStore.resLumber >= numLumber && dataStore.resOre >= numOre && dataStore.resBricks >= numBrick && dataStore.resPlanks >= numPlanks && dataStore.resMetal >= numMetal) result = true;
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
            dataStore.resClay -= numClay;
            dataStore.resLumber -= numLumber;
            dataStore.resOre -= numOre;
            dataStore.resBricks -= numBrick;
            dataStore.resPlanks -= numPlanks;
            dataStore.resMetal -= numMetal;
            result = true;
        }
        return result;
    }

    public void UpgradeResourceCap (ResourceType resource)
    {
        switch (resource)
        {
            case ResourceType.Bricks:
                dataStore.resBricks_maxUpgrades++;
                break;
            case ResourceType.Metal:
                dataStore.resMetal_maxUpgrades++;
                break;
            case ResourceType.Planks:
                dataStore.resPlanks_maxUpgrades++;
                break;
            case ResourceType.Clay:
                dataStore.resClay_maxUpgrades++;
                break;
            case ResourceType.Ore:
                dataStore.resOre_maxUpgrades++;
                break;
            case ResourceType.Lumber:
                dataStore.resLumber_maxUpgrades++;
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
