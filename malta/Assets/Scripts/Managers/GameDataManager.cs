using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class GameDataManager_DataStore
{
    public SceneIDType lastScene;
    public Adventurer sovereignAdventurer;
    public Adventurer[] houseAdventurers;
    public BattlerAction sovereignTactic;
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
    public SovereignWpn sovWpn_Mace;
    public SovereignWpn sovWpn_Knives;
    public SovereignWpn sovWpn_Staff;
    public SovereignWpn sovWpn_Set { get { if (sovereignEquippedWeaponType == WpnType.Mace) return sovWpn_Mace; else if (sovereignEquippedWeaponType == WpnType.Knives) return sovWpn_Knives; else return sovWpn_Staff; } }
    public bool sovereignOnBackRow;
    public WpnType sovereignEquippedWeaponType;
    public string yourTownName;
    public string[] fakeTownNames;
    public bool pendingUpgrade_Docks;
    public bool pendingUpgrade_Mason;
    public bool pendingUpgrade_Sawmill;
    public bool pendingUpgrade_Smith;
    public bool unlock_sovSpe_CalledShots;
    public bool unlock_sovSpe_HammerSmash;
    public bool unlock_sovSpe_Protect;
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
    public const int housingLevelCap = 20;
    public ProgressionFlags progressionFlags;
    public SovereignWpn buyable0;
    public SovereignWpn buyable1;
    public WarriorPromotes unlockedWarriorPromotes;
    public MysticPromotes unlockedMysticPromotes;
    public WarriorPromotes nextWarriorPromote;
    public MysticPromotes nextMysticPromote;
    public int[] nextPromoteUnlockCosts;
    public int nextPromoteUnlockBattles;
    public bool peddlerIsPresent;
    public int peddlerPrice;

    public GameDataManager_DataStore ()
    {
        lastScene = SceneIDType.OverworldScene;
        yourTownName = "Citysburg";
        fakeTownNames = new string[] { "Town A", "Town No. 2", "Tertiary Town" };
        pendingUpgrade_Docks = false;
        pendingUpgrade_Mason = false;
        pendingUpgrade_Sawmill = false;
        pendingUpgrade_Smith = false;
        unlock_sovSpe_CalledShots = false;
        unlock_sovSpe_HammerSmash = false;
        unlock_sovSpe_Protect = false;
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
        partyAdventurer0Index = 0;
        partyAdventurer1Index = 1;
        partyAdventurer2Index = 2;
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
        sovereignTactic = BattlerAction.GetBehindMe;
        sovereignSkill = AdventurerSpecial.None;
        sovereignMugshot = AdventurerMugshot.Sovereign0;
        progressionFlags = 0;
        buyable0 = null;
        buyable1 = null;
        unlockedMysticPromotes = MysticPromotes.None;
        unlockedWarriorPromotes = WarriorPromotes.None;
        peddlerIsPresent = false;
        peddlerPrice = 25;
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
    private const int pendingUpgradeBaseTime = 150; // 2.5 min;
    private const string saveName = "/save.bin";

    void Start ()
    {
        lastSecondTimestamp = Time.time;
        saveExisted = LoadFromSave();
    }

    void DebugBlob ()
    {
        Debug.logger.logEnabled = true;
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            string s = "";
            for (int i = 1; i > 1 << 31; )
            {
                if (HasFlag((ProgressionFlags)i)) s = s + ((ProgressionFlags)i).ToString() +" ";
                i = i << 1;
            }
            Debug.Log(s);
        }
        if (Input.GetKeyDown(KeyCode.Home))
        {
            BattleEndDataRefresh();
        }
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
    }

    void Update ()
    {
        DebugBlob();
        if (Time.time - lastSecondTimestamp >= 1.0f)
        {
            lastSecondTimestamp = Time.time;
            UpdateProcessing_ResourceGain();
            if (HasFlag(ProgressionFlags.TaskmasterUnlock)) UpdateProcessing_ResourceGain(); // output doubled in the quickest, laziest way possible
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Docks, ref dataStore.buildingLv_Docks, ref dataStore.pendingUpgrade_Docks, ref dataStore.pendingUpgradeTimer_Docks); // this is a hack
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Mason, ref dataStore.buildingLv_Mason, ref dataStore.pendingUpgrade_Mason, ref dataStore.resBricks_maxUpgrades);
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Sawmill, ref dataStore.buildingLv_Sawmill, ref dataStore.pendingUpgrade_Sawmill, ref dataStore.resPlanks_maxUpgrades);
            HandlePendingUpgrade(ref dataStore.pendingUpgradeTimer_Smith, ref dataStore.buildingLv_Smith, ref dataStore.pendingUpgrade_Smith, ref dataStore.resMetal_maxUpgrades);
        }
        for (int i = 0; i < dataStore.housingLevel; i++)
        {
            if (!dataStore.houseAdventurers[i].initialized) dataStore.houseAdventurers[i].Reroll(AdventurerClass.Warrior, AdventurerSpecies.Human, dataStore.housingUnitUpgrades[i], Adventurer.GetRandomStatPoint());
            else if (dataStore.houseAdventurers[i].isDeceased) dataStore.houseAdventurers[i].ReplaceDead();
        }
        if (dataStore.adventureLevel > 1 && (!HasFlag(ProgressionFlags.FaeUnlock) || !HasFlag(ProgressionFlags.OrcUnlock)))
        {
            SetFlag(ProgressionFlags.FaeUnlock);
            SetFlag(ProgressionFlags.OrcUnlock);
        }
        if (dataStore.adventureLevel > 2 && !HasFlag(ProgressionFlags.TowerUnlock)) SetFlag(ProgressionFlags.TowerUnlock);
        if (dataStore.sovWpn_Set == null)
        {
            dataStore.sovWpn_Knives = new SovereignWpn(0, WpnType.Knives);
            dataStore.sovWpn_Mace = new SovereignWpn(0, WpnType.Mace);
            dataStore.sovWpn_Staff = new SovereignWpn(0, WpnType.Staff);
            dataStore.sovereignEquippedWeaponType = WpnType.Mace;
            dataStore.sovereignAdventurer.PushWpnToSovereign();
        }

    }

    void DetermineNextPromoteUnlock ()
    {
        int unlockedWpCount = 0;
        int unlockedMpCount = 0;
        List<WarriorPromotes> availableWp = new List<WarriorPromotes>();
        List<MysticPromotes> availableMp = new List<MysticPromotes>();
        for (int i = 1; i > 1 << 31; )
        {
            if (i <= WarriorPromotes_Metadata.LastVal)
            {
                if (WarriorPromoteUnlocked((WarriorPromotes)i)) unlockedWpCount++;
                else availableWp.Add((WarriorPromotes)i);
            }
            if (i <= MysticPromotes_Metadata.LastVal)
            {
                if (MysticPromoteUnlocked((MysticPromotes)i)) unlockedMpCount++;
                else availableMp.Add((MysticPromotes)i);
            }
            i = i << 1;
        }
        int wScore = 0;
        int mScore = 0;
        if (availableWp.Count > 0)
        {
            if (dataStore.unlockedWarriorPromotes == 0) wScore++;
            if (unlockedWpCount <= unlockedMpCount) wScore++;
        }
        if (availableMp.Count > 0)
        {
            if (dataStore.unlockedMysticPromotes == 0) mScore++;
            if (unlockedMpCount <= unlockedWpCount) mScore++;
        }
        if (wScore > 0 && mScore > 0)
        {
            if (Random.Range(0, 2) == 0) wScore = 0;
            else mScore = 0; 
        }
        if (wScore > 0)
        {
            dataStore.nextWarriorPromote = availableWp[Random.Range(0, availableMp.Count)];
            dataStore.nextMysticPromote = MysticPromotes.None;
        }
        else if (mScore > 0)
        {
            dataStore.nextMysticPromote = availableMp[Random.Range(0, availableMp.Count)];
            dataStore.nextWarriorPromote = WarriorPromotes.None;
        }
        else
        {
            dataStore.nextWarriorPromote = WarriorPromotes.None;
            dataStore.nextMysticPromote = MysticPromotes.None;
        }
        int c = unlockedWpCount + unlockedMpCount;
        if (c < 1) dataStore.nextPromoteUnlockBattles = 0;
        else if (dataStore.nextWarriorPromote == WarriorPromotes.None && dataStore.nextMysticPromote == MysticPromotes.None) dataStore.nextPromoteUnlockBattles = int.MinValue;
        else dataStore.nextPromoteUnlockBattles = Random.Range(c, c * 3);
        int r = 20 + (c * 30);
        dataStore.nextPromoteUnlockCosts = new int[] { r, r, r };
    }

    void RerollBuyableWpns ()
    {
        WpnType wpnType0 = (WpnType)Random.Range(1, 4);
        WpnType wpnType1 = (WpnType)Random.Range(1, 4);
        switch (dataStore.adventureLevel)
        {
            case 0:
                dataStore.buyable0 = new SovereignWpn(1, wpnType0);
                dataStore.buyable1 = new SovereignWpn(0, wpnType1);
                break;
            case 1:
                dataStore.buyable0 = new SovereignWpn(1, wpnType0);
                dataStore.buyable1 = new SovereignWpn(1, wpnType1);
                break;
            case 2:
                dataStore.buyable0 = new SovereignWpn(2, wpnType0);
                dataStore.buyable1 = new SovereignWpn(1, wpnType1);
                break;
            default:
                dataStore.buyable0 = new SovereignWpn(2, wpnType0);
                dataStore.buyable1 = new SovereignWpn(2, wpnType1);
                break;
        }
    }

    public void UnlockNextPromote ()
    {
        if (dataStore.nextWarriorPromote != WarriorPromotes.None)
        {
            UnlockWarriorPromote(dataStore.nextWarriorPromote);
            dataStore.nextWarriorPromote = WarriorPromotes.None;
        }
        else if (dataStore.nextMysticPromote != MysticPromotes.None)
        {
            UnlockMysticPromote(dataStore.nextMysticPromote);
            dataStore.nextMysticPromote = MysticPromotes.None;
        }
        DetermineNextPromoteUnlock();
    }

    public void BattleEndDataRefresh ()
    {
        const int peddlerRarity = 8;
        if (dataStore.peddlerIsPresent) dataStore.peddlerIsPresent = false;
        else if (Random.Range(0, peddlerRarity) == 0) dataStore.peddlerIsPresent = true;
        RerollBuyableWpns();
        if (dataStore.nextPromoteUnlockBattles > 0) dataStore.nextPromoteUnlockBattles--;
    }

    public void SetFlag (ProgressionFlags flag, bool unset = false)
    {
        if (unset) dataStore.progressionFlags &= ~flag;
        else dataStore.progressionFlags |= flag;
    }

    public bool HasFlag (ProgressionFlags flag)
    {
        return ((dataStore.progressionFlags & flag) == flag);
    }

    public void ChangeSetSovWpn (WpnType wpn)
    {
        dataStore.sovereignEquippedWeaponType = wpn;
        if (wpn == WpnType.Mace) dataStore.sovereignOnBackRow = false;
        else if (wpn == WpnType.Staff) dataStore.sovereignOnBackRow = true;
        dataStore.sovereignAdventurer.PushWpnToSovereign();
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
        else
        {
            dataStore.sovereignAdventurer.SanityCheck();
            for (int i = 0; i < dataStore.housingLevel; i++) dataStore.houseAdventurers[i].SanityCheck();
        }
        return r;
    }

    public Adventurer GetPartyMember (int partySlot)
    {
        int advIndex = -1;
        switch (partySlot)
        {
            case 0:
                advIndex = dataStore.partyAdventurer0Index;
                break;
            case 1:
                advIndex = dataStore.partyAdventurer1Index;
                break;
            case 2:
                advIndex = dataStore.partyAdventurer2Index;
                break;
        }
        Adventurer a;
        if (advIndex < 0 || advIndex >= dataStore.housingLevel) a = null;
        else a = dataStore.houseAdventurers[advIndex];
        return a;
    }

    public void SetPartyMember (int partySlot, int advIndex)
    {
        switch (partySlot)
        {
            case 0:
                dataStore.partyAdventurer0Index = advIndex;
                break;
            case 1:
                dataStore.partyAdventurer1Index = advIndex;
                break;
            case 2:
                dataStore.partyAdventurer2Index = advIndex;
                break;
            default:
                throw new System.Exception("Called SetPartyMember with invalid party slot: " + partySlot);
        }
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

    public void GiveSovereignBuyableWpn (bool wpn2 = false)
    {
        const int wpnLvMax = 2;
        SovereignWpn boughtWpn;
        if (wpn2)
        {
            boughtWpn = dataStore.buyable1;
            dataStore.buyable1 = null;
        }
        else
        {
            boughtWpn = dataStore.buyable0;
            dataStore.buyable0 = null;
        }
        switch (boughtWpn.wpnType)
        {
            case WpnType.Mace:
                dataStore.sovWpn_Mace = boughtWpn;
                break;
            case WpnType.Knives:
                dataStore.sovWpn_Knives = boughtWpn;
                break;
            case WpnType.Staff:
                dataStore.sovWpn_Staff = boughtWpn;
                break;
        }
        if (boughtWpn.wpnLevel == wpnLvMax && !HasFlag(ProgressionFlags.FirstTier2WpnBought)) SetFlag(ProgressionFlags.FirstTier2WpnBought);
    }

    public void RegenerateDataStore()
    {
        dataStore = new GameDataManager_DataStore();
        RecalculateResourceMaximums();
        DetermineNextPromoteUnlock();
        for (int i = 0; i < dataStore.housingLevel; i++)
        {
            dataStore.houseAdventurers[i].Reroll(AdventurerClass.Warrior, AdventurerSpecies.Human, dataStore.housingUnitUpgrades[i], Adventurer.GetRandomStatPoint());
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
        if (HasFlag(ProgressionFlags.TowerUnlock)) gain += Mathf.RoundToInt(gain * 0.1f * dataStore.buildingLv_WizardsTower);
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

    public void UnlockMysticPromote (MysticPromotes promote)
    {
        dataStore.unlockedMysticPromotes = dataStore.unlockedMysticPromotes | promote;
    }

    public bool MysticPromoteUnlocked (MysticPromotes promote)
    {
        return ((dataStore.unlockedMysticPromotes & promote) == promote);
    }

    public void UnlockWarriorPromote (WarriorPromotes promote)
    {
        dataStore.unlockedWarriorPromotes = dataStore.unlockedWarriorPromotes | promote;
    }

    public bool WarriorPromoteUnlocked (WarriorPromotes promote)
    {
        return ((dataStore.unlockedWarriorPromotes & promote) == promote);
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

    public void SetSovereignTactic (BattlerAction attack)
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

    public int GetManaFromResourceCosts (int b, int p, int m)
    {
        float bricksMulti;
        if (dataStore.resBricks_Lv > 0) bricksMulti = 0.5f;
        else bricksMulti = 1 / 3f;
        float planksMulti;
        if (dataStore.resPlanks_Lv > 0) planksMulti = 0.5f;
        else planksMulti = 1 / 3f;
        float metalMulti;
        if (dataStore.resMetal_Lv > 0) metalMulti = 0.5f;
        else metalMulti = 1 / 3f;
        return Mathf.CeilToInt(b * bricksMulti) + Mathf.CeilToInt(p * planksMulti) + Mathf.CeilToInt(m * metalMulti);
    }

    public int GetManaFromResourceCosts (int[] costs)
    {
        return GetManaFromResourceCosts(costs[0], costs[1], costs[2]);
    }

    public void AwardMana (int m)
    {
        if (m < 1) throw new System.Exception("awarded " + m.ToString() + " mana, what the fuck are you even doing you utter shithead");
        dataStore.resMana += m;
        if (dataStore.resMana > dataStore.resMana_max) dataStore.resMana = dataStore.resMana_max;
    }

    public void BurnResourcesIntoMana (int[] costs)
    {
        if (SpendResourcesIfPossible(costs) == false) throw new System.Exception("How are you calling BurnResourcesIntoMana without the costs???");
        AwardMana(GetManaFromResourceCosts(costs));
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
