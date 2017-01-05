using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class GameDataManager_DataStore
{
    public SceneIDType lastScene;
    public Adventurer sovereignAdventurer;
    public Adventurer[] houseAdventurers;
    public AdventurerClass warriorClassUnlock;
    public AdventurerClass mysticClassUnlock;
    public AdventurerAttack sovereignTactic;
    public AdventurerSpecial sovereignSkill;
    public AdventurerMugshot sovereignMugshot;
    public int housingLevel;
    public int partyAdventurer0Index;
    public int partyAdventurer1Index;
    public int partyAdventurer2Index;
    public bool[] housingUnitUpgrades;
    public string sovereignFirstName;
    public string sovereignLastName;
    public string sovereignName { get { return sovereignAdventurer.fullName; } }
    public string yourTownName;
    public string[] fakeTownNames;
    public bool pendingUpgrade_Docks;
    public bool pendingUpgrade_Mason;
    public bool pendingUpgrade_Sawmill;
    public bool pendingUpgrade_Smith;
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
    public int nextRandomAdventureAnte;
    public int resBricks;
    public int resBricks_max;
    public int resBricks_maxUpgrades;
    public int resBricks_Lv;
    public int resMetal;
    public int resMetal_max;
    public int resMetal_maxUpgrades;
    public int resMetal_Lv;
    public int resPlanks;
    public int resPlanks_max;
    public int resPlanks_maxUpgrades;
    public int resPlanks_Lv;
    public int resMana;
    public int resMana_max;
    public int pendingUpgradeTimer_Docks;
    public int pendingUpgradeTimer_Mason;
    public int pendingUpgradeTimer_Sawmill;
    public int pendingUpgradeTimer_Smith;
    public int lastInspectedAdventurerIndex;
    const int manaCap = 100;
    const int housingLevelCap = 20;

    public GameDataManager_DataStore ()
    {
        lastScene = SceneIDType.OverworldScene;
        yourTownName = "Citysburg";
        fakeTownNames = new string[] { "Town A", "Town No. 2", "Tertiary Town" };
        pendingUpgrade_Docks = false;
        pendingUpgrade_Mason = false;
        pendingUpgrade_Sawmill = false;
        pendingUpgrade_Smith = false;
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
        nextRandomAdventureAnte = 2;
        resBricks = 0;
        resBricks_max = 999; // we actually recalc these immediately, lol
        resBricks_maxUpgrades = 0;
        resMetal = 0;
        resMetal_max = 999;
        resMetal_maxUpgrades = 0;
        resPlanks = 0;
        resPlanks_max = 999;
        resPlanks_maxUpgrades = 0;
        resMana = 0;
        resMana_max = manaCap;
        pendingUpgradeTimer_Docks = 0;
        pendingUpgradeTimer_Mason = 0;
        pendingUpgradeTimer_Sawmill = 0;
        pendingUpgradeTimer_Smith = 0;
        lastInspectedAdventurerIndex = 0;
        partyAdventurer0Index = -1;
        partyAdventurer1Index = -1;
        partyAdventurer2Index = -1;
        sovereignFirstName = "Dude";
        sovereignLastName = "Huge";
        housingLevel = 0;
        housingUnitUpgrades = new bool[housingLevelCap];
        houseAdventurers = new Adventurer[housingLevelCap];
        for (int i = 0; i < houseAdventurers.Length; i++)
        {
            houseAdventurers[i] = new Adventurer();
        }
        sovereignAdventurer = new Adventurer();
        warriorClassUnlock = AdventurerClass.Warrior;
        mysticClassUnlock = AdventurerClass.Mystic;
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
        if (Application.platform == RuntimePlatform.WebGLPlayer) Application.ExternalCall("sync");
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
    public bool saveExisted { get; private set; }
    private float lastSecondTimestamp;
    private int resourceGainTimer = 0;
    private const int resourceGainThreshold = 59;
    private const int resourceMaximumsBaseValue= 600; // ten hours
    private const int pendingUpgradeBaseTime = 300; // five minutes;
    private const string saveName = "/save.bin";

    void Start ()
    {
        lastSecondTimestamp = Time.time;
        saveExisted = LoadFromSave();
    }

    void Update ()
    {
        if (Input.GetKey(KeyCode.Pause))
        {
            Time.timeScale = 100.0f; // speed things up for testing
        }
        else Time.timeScale = 1.0f;
        if (Input.GetKey(KeyCode.Backslash))
        {
            dataStore.resBricks += 30;
            dataStore.resMetal += 30;
            dataStore.resPlanks += 30;
            dataStore.resMana += 30;
        }
        if (Time.time - lastSecondTimestamp >= 1.0f)
        {
            lastSecondTimestamp = Time.time;
            UpdateProcessing_ResourceGain();
            if (dataStore.unlock_Taskmaster) UpdateProcessing_ResourceGain(); // output doubled in the quickest, laziest way possible
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Docks, ref dataStore.buildingLv_Docks, ref dataStore.pendingUpgrade_Docks, ref dataStore.pendingUpgradeTimer_Docks); // this is a hack
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Mason, ref dataStore.buildingLv_Mason, ref dataStore.pendingUpgrade_Mason, ref dataStore.resBricks_maxUpgrades);
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Sawmill, ref dataStore.buildingLv_Sawmill, ref dataStore.pendingUpgrade_Sawmill, ref dataStore.resPlanks_maxUpgrades);
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Smith, ref dataStore.buildingLv_Smith, ref dataStore.pendingUpgrade_Smith, ref dataStore.resMetal_maxUpgrades);
        }
        for (int i = 0; i < dataStore.housingLevel; i++) if (!dataStore.houseAdventurers[i].initialized) dataStore.houseAdventurers[i].Reroll(dataStore.warriorClassUnlock, AdventurerSpecies.Human, dataStore.housingUnitUpgrades[i], Adventurer.GetRandomStatPoint());
        if (dataStore.adventureLevel > 1 && (!dataStore.unlock_raceFae || !dataStore.unlock_raceOrc))
        {
            dataStore.unlock_raceFae = true;
            dataStore.unlock_raceOrc = true;
        }
        if (dataStore.adventureLevel > 2 && !dataStore.unlock_WizardsTower) dataStore.unlock_WizardsTower = true;

    }

    public bool LoadFromSave ()
    {
        bool r = false;
        if (File.Exists(Application.persistentDataPath + saveName))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream f = File.OpenRead(Application.persistentDataPath + saveName);
            try
            {
                dataStore = formatter.Deserialize(f) as GameDataManager_DataStore;
                r = true;
                f.Close();
            }
            catch (System.Exception)
            {
                f.Close();
                EraseSave();
                r = false;
            }
        }
        if (dataStore == null) RegenerateDataStore();
        return r;
    }

    public void Save ()
    {
        dataStore.SaveToFile(Application.persistentDataPath + saveName);
        saveExisted = true;
    }

    public void EraseSave ()
    {
        RegenerateDataStore();
        if (File.Exists(Application.persistentDataPath + saveName)) File.Delete(Application.persistentDataPath + saveName);
        if (Application.platform == RuntimePlatform.WebGLPlayer) Application.ExternalCall("sync");
    }

    public void RegenerateDataStore()
    {
        dataStore = new GameDataManager_DataStore();
        RecalculateResourceMaximums();
        for (int i = 0; i < dataStore.housingLevel; i++)
        {
            dataStore.houseAdventurers[i].Reroll(dataStore.warriorClassUnlock, AdventurerSpecies.Human, dataStore.housingUnitUpgrades[i], Adventurer.GetRandomStatPoint());
        }
        dataStore.sovereignMugshot = (AdventurerMugshot)Random.Range((int)AdventurerMugshot.Sovereign0, (int)AdventurerMugshot.Sovereign7 + 1);
        dataStore.sovereignAdventurer.Reroll(AdventurerClass.Sovereign, AdventurerSpecies.Human, false, new int[] { 0, 0, 0, 0 });
        saveExisted = false;
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
            case ResourceType.Metal:
                gain = GetResourceGainRate(dataStore.buildingLv_Smith, 4);
                break;
            case ResourceType.Planks:
                gain = GetResourceGainRate(dataStore.buildingLv_Sawmill, 4);
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
        dataStore.mysticClassUnlock = advClass;
    }

    public void PromoteWarriorsTo (AdventurerClass advClass)
    {
        for (int i = 0; i < dataStore.houseAdventurers.Length; i++)
        {
            if (dataStore.houseAdventurers[i] != null && dataStore.houseAdventurers[i].advClass == dataStore.warriorClassUnlock) dataStore.houseAdventurers[i].Reclass(advClass);
        }
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

    public bool CheckManaAvailability (int mana)
    {
        bool result = false;
        if (dataStore.resMana >= mana) result = true;
        return result;
    }

    public bool CheckMaterialAvailability (int[] resourceNums)
    {
        return CheckMaterialAvailability(resourceNums[0], resourceNums[1], resourceNums[2]);
    }

    public bool CheckMaterialAvailability(int numBrick, int numPlanks, int numMetal)
    {
        bool result = false;
        if (dataStore.resBricks >= numBrick && dataStore.resPlanks >= numPlanks && dataStore.resMetal >= numMetal) result = true;
        return result;
    }

    public bool SpendManaIfPossible (int mana)
    {
        bool result = false;
        if (dataStore.resMana >= mana)
        {
            dataStore.resMana -= mana;
            result = true;
        }
        return result;
    }

    public bool SpendResourcesIfPossible (int[] resourceNums)
    {
        return SpendResourcesIfPossible(resourceNums[0], resourceNums[1], resourceNums[2]);
    }

    public bool SpendResourcesIfPossible (int numBrick, int numPlanks, int numMetal)
    {
        bool result = false;
        if (CheckMaterialAvailability(numBrick, numPlanks, numMetal))
        {
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
        }
        RecalculateResourceMaximums();
    }

    void _in_SetBuildingUpgradePending (ref bool pendingUpgrade, ref int pendingUpgradeTimer, int buildingLv)
    {
        pendingUpgrade = true;
        pendingUpgradeTimer = pendingUpgradeBaseTime * buildingLv;
    }


}
