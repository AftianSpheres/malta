using UnityEngine;
using UnityEngine.UI;
using System;

enum DevStatusOptions
{
    None,
    UpgradeReady,
    UpgradeInProgress,
    NotYetBuilt,
    InsufficientResources,
    OutbuildingReady,
    AllGood
}

public class BuildingInfoPanel : MonoBehaviour
{
    public BuildingType buildingType;
    public int nonUniqueBuildingsIndex;
    public GameObject materialsNeededSection;
    public GameObject pendingUpgradeTimerSection;
    public Text clayResCounter;
    public Text woodResCounter;
    public Text oreResCounter;
    public Text bricksResCounter;
    public Text planksResCounter;
    public Text metalResCounter;
    public Text timerCounter;
    public Text devStatusLabel;
    public Text devTypeLabel;
    public TextAsset stringsResource;
    private int buildingLevelCached = -1;
    private int buildingUpgradeTimerCached = -1;
    private int[] cachedMaterialQuantities = { -1, -1, -1, -1, -1, -1 };
    private string[] strings;
    private DevStatusOptions cachedDevStatus;

	// Use this for initialization
	void Start ()
    {
        strings = stringsResource.text.Split('\n');
    }

    // Update is called once per frame
    void Update ()
    {
        if (GameDataManager.Instance != null)
        {
            switch (buildingType)
            {
                case BuildingType.House:
                    UpdateProcessing_Houses();
                    break;
                case BuildingType.Forge:
                    UpdateProcessing_Forge();
                    break;
                case BuildingType.Docks:
                    UpdateProcessing_UpgradableBuildings(ref GameDataManager.Instance.pendingUpgrade_Docks, ref GameDataManager.Instance.pendingUpgradeTimer_Docks, 
                        ref GameDataManager.Instance.buildingLv_Docks, 22, TownBuilding.GetUpgradeCost_Docks);
                    break;
                case BuildingType.Mason:
                    UpdateProcessing_UpgradableBuildings(ref GameDataManager.Instance.pendingUpgrade_Mason, ref GameDataManager.Instance.pendingUpgradeTimer_Mason,
                        ref GameDataManager.Instance.buildingLv_Mason, 23, TownBuilding.GetUpgradeCost_MasonClaypit);
                    break;
                case BuildingType.Sawmill:
                    UpdateProcessing_UpgradableBuildings(ref GameDataManager.Instance.pendingUpgrade_Sawmill, ref GameDataManager.Instance.pendingUpgradeTimer_Sawmill,
                        ref GameDataManager.Instance.buildingLv_Sawmill, 24, TownBuilding.GetUpgradeCost_SawmillWoodlands);
                    break;
                case BuildingType.Smith:
                    UpdateProcessing_UpgradableBuildings(ref GameDataManager.Instance.pendingUpgrade_Smith, ref GameDataManager.Instance.pendingUpgradeTimer_Smith,
                        ref GameDataManager.Instance.buildingLv_Smith, 25, TownBuilding.GetUpgradeCost_SmithMines);
                    break;
                case BuildingType.ClayPit:
                    UpdateProcessing_UpgradableBuildings(ref GameDataManager.Instance.pendingUpgrade_ClayPit, ref GameDataManager.Instance.pendingUpgradeTimer_ClayPit,
                        ref GameDataManager.Instance.harvestLv_ClayPit, 28, TownBuilding.GetUpgradeCost_MasonClaypit);
                    break;
                case BuildingType.Woodlands:
                    UpdateProcessing_UpgradableBuildings(ref GameDataManager.Instance.pendingUpgrade_Woodlands, ref GameDataManager.Instance.pendingUpgradeTimer_Woodlands,
                        ref GameDataManager.Instance.harvestLv_Woodlands, 27, TownBuilding.GetUpgradeCost_SawmillWoodlands);
                    break;
                case BuildingType.Mine:
                    UpdateProcessing_UpgradableBuildings(ref GameDataManager.Instance.pendingUpgrade_Mine, ref GameDataManager.Instance.pendingUpgradeTimer_Mine,
                        ref GameDataManager.Instance.harvestLv_Mine, 26, TownBuilding.GetUpgradeCost_SmithMines);
                    break;
            }
        }
	}

    private void RefreshResourceCounters (int[] costs)
    {
        clayResCounter.text = costs[0].ToString();
        woodResCounter.text = costs[1].ToString();
        oreResCounter.text = costs[2].ToString();
        bricksResCounter.text = costs[3].ToString();
        planksResCounter.text = costs[4].ToString();
        metalResCounter.text = costs[5].ToString();
    }

    private void RefreshTimer (float timer, float level)
    {
        timerCounter.text = PopupMenu.GetTimerReadout(timer, level);
    }

    private void UpdateProcessing_Forge ()
    {
        if (GameDataManager.Instance.warriorClassUnlock == AdventurerClass.Warrior || GameDataManager.Instance.mysticClassUnlock == AdventurerClass.Mystic)
        {
            if (!materialsNeededSection.activeInHierarchy) materialsNeededSection.SetActive(true);
            if (GameDataManager.Instance.resClay != cachedMaterialQuantities[0] ||
                GameDataManager.Instance.resLumber != cachedMaterialQuantities[1] ||
                GameDataManager.Instance.resOre != cachedMaterialQuantities[2] ||
                GameDataManager.Instance.resBricks != cachedMaterialQuantities[3] ||
                GameDataManager.Instance.resPlanks != cachedMaterialQuantities[4] ||
                GameDataManager.Instance.resMetal != cachedMaterialQuantities[5])
            {
                cachedMaterialQuantities = new int[] {GameDataManager.Instance.resClay, GameDataManager.Instance.resLumber, GameDataManager.Instance.resOre,
                                                                  GameDataManager.Instance.resBricks, GameDataManager.Instance.resPlanks, GameDataManager.Instance.resMetal};
                int[] costs = TownBuilding.GetUpgradeCost_Forge();
                RefreshResourceCounters(costs);
                if (GameDataManager.Instance.CheckMaterialAvailability(costs))
                {
                    if (devStatusLabel.text != strings[30]) devStatusLabel.text = strings[30];
                }
                else if (devStatusLabel.text != strings[15]) devStatusLabel.text = strings[15];
            }
        }
        else if (!GameDataManager.Instance.unlock_forgeOutbuilding)
        {
            if (!materialsNeededSection.activeInHierarchy) materialsNeededSection.SetActive(true);
            if (GameDataManager.Instance.resClay != cachedMaterialQuantities[0] ||
                GameDataManager.Instance.resLumber != cachedMaterialQuantities[1] ||
                GameDataManager.Instance.resOre != cachedMaterialQuantities[2] ||
                GameDataManager.Instance.resBricks != cachedMaterialQuantities[3] ||
                GameDataManager.Instance.resPlanks != cachedMaterialQuantities[4] ||
                GameDataManager.Instance.resMetal != cachedMaterialQuantities[5])
            {
                cachedMaterialQuantities = new int[] {GameDataManager.Instance.resClay, GameDataManager.Instance.resLumber, GameDataManager.Instance.resOre,
                                                                  GameDataManager.Instance.resBricks, GameDataManager.Instance.resPlanks, GameDataManager.Instance.resMetal};
                int[] costs = TownBuilding.GetUpgradeCost_Forge();
                RefreshResourceCounters(costs);
                if (GameDataManager.Instance.CheckMaterialAvailability(costs))
                {
                    if (devStatusLabel.text != strings[16]) devStatusLabel.text = strings[16];
                }
                else if (devStatusLabel.text != strings[15]) devStatusLabel.text = strings[15];
            }
        }
        else
        {
            if (materialsNeededSection.activeInHierarchy) materialsNeededSection.SetActive(false);
            if (devStatusLabel.text != strings[17]) devStatusLabel.text = strings[17];
        }
    }

    private void UpdateProcessing_Houses ()
    {
        if (GameDataManager.Instance.housesBuilt[nonUniqueBuildingsIndex] == false)
        {
            if (devTypeLabel.text != strings[18]) devTypeLabel.text = strings[18];
            if (!materialsNeededSection.activeInHierarchy) materialsNeededSection.SetActive(true);
            if (GameDataManager.Instance.resClay != cachedMaterialQuantities[0] ||
                GameDataManager.Instance.resLumber != cachedMaterialQuantities[1] ||
                GameDataManager.Instance.resOre != cachedMaterialQuantities[2] ||
                GameDataManager.Instance.resBricks != cachedMaterialQuantities[3] ||
                GameDataManager.Instance.resPlanks != cachedMaterialQuantities[4] ||
                GameDataManager.Instance.resMetal != cachedMaterialQuantities[5])
            {
                cachedMaterialQuantities = new int[] {GameDataManager.Instance.resClay, GameDataManager.Instance.resLumber, GameDataManager.Instance.resOre,
                                                                  GameDataManager.Instance.resBricks, GameDataManager.Instance.resPlanks, GameDataManager.Instance.resMetal};
                RefreshResourceCounters(TownBuilding.houseConstructionCosts);
                if (GameDataManager.Instance.CheckMaterialAvailability(TownBuilding.houseConstructionCosts))
                {
                    if (devStatusLabel.text != strings[14]) devStatusLabel.text = strings[14];
                }
                else if (devStatusLabel.text != strings[15]) devStatusLabel.text = strings[15];
            }
        }
        else
        {
            if (GameDataManager.Instance.houseAdventurers[nonUniqueBuildingsIndex]!= null && GameDataManager.Instance.houseAdventurers[nonUniqueBuildingsIndex].initialized)
            {
                if (devTypeLabel.text != GameDataManager.Instance.houseAdventurers[nonUniqueBuildingsIndex].fullTitle) devTypeLabel.text = GameDataManager.Instance.houseAdventurers[nonUniqueBuildingsIndex].fullTitle;
            }
            else if (devTypeLabel.text != strings[19]) devTypeLabel.text = strings[19];
            if (GameDataManager.Instance.housesOutbuildingsBuilt[nonUniqueBuildingsIndex])
            {
                if (materialsNeededSection.activeInHierarchy) materialsNeededSection.SetActive(false);
                if (devStatusLabel.text != strings[17]) devStatusLabel.text = strings[17];
            }
            else
            {
                if (!materialsNeededSection.activeInHierarchy) materialsNeededSection.SetActive(true);
                if (GameDataManager.Instance.resClay != cachedMaterialQuantities[0] ||
                    GameDataManager.Instance.resLumber != cachedMaterialQuantities[1] ||
                    GameDataManager.Instance.resOre != cachedMaterialQuantities[2] ||
                    GameDataManager.Instance.resBricks != cachedMaterialQuantities[3] ||
                    GameDataManager.Instance.resPlanks != cachedMaterialQuantities[4] ||
                    GameDataManager.Instance.resMetal != cachedMaterialQuantities[5])
                {
                    cachedMaterialQuantities = new int[] {GameDataManager.Instance.resClay, GameDataManager.Instance.resLumber, GameDataManager.Instance.resOre,
                                                                  GameDataManager.Instance.resBricks, GameDataManager.Instance.resPlanks, GameDataManager.Instance.resMetal};
                    RefreshResourceCounters(TownBuilding.houseOutbuildingCosts);
                    if (GameDataManager.Instance.CheckMaterialAvailability(TownBuilding.houseOutbuildingCosts))
                    {
                        if (devStatusLabel.text != strings[16]) devStatusLabel.text = strings[16];
                    }
                    else if (devStatusLabel.text != strings[15]) devStatusLabel.text = strings[15];
                }
            }
        }
    }

    private void UpdateProcessing_UpgradableBuildings (ref bool pendingUpgrade, ref int pendingUpgradeTimer, ref int baseBuildingLv, int nameStringIndex, Func<int, int[]> costsLookupFunction)
    {
        if (baseBuildingLv != buildingLevelCached)
        {
            buildingLevelCached = baseBuildingLv;
            devTypeLabel.text = strings[nameStringIndex] + strings[baseBuildingLv];
            RefreshResourceCounters(costsLookupFunction(baseBuildingLv));
        }
        if (baseBuildingLv >= TownBuilding.buildingTypeMaxLevels[(int)buildingType])
        {
            if (cachedDevStatus != DevStatusOptions.AllGood)
            {
                cachedDevStatus = DevStatusOptions.AllGood;
                devStatusLabel.text = strings[17];
            }
            if (materialsNeededSection.activeInHierarchy) materialsNeededSection.SetActive(false);
            if (pendingUpgradeTimerSection.activeInHierarchy) pendingUpgradeTimerSection.SetActive(false);
        }
        else if (pendingUpgrade)
        {
            if (cachedDevStatus != DevStatusOptions.UpgradeInProgress)
            {
                cachedDevStatus = DevStatusOptions.UpgradeInProgress;
                devStatusLabel.text = strings[13];
            }
            if (materialsNeededSection.activeInHierarchy) materialsNeededSection.SetActive(false);
            if (!pendingUpgradeTimerSection.activeInHierarchy && baseBuildingLv < TownBuilding.buildingTypeMaxLevels[(int)buildingType]) pendingUpgradeTimerSection.SetActive(true);
            if (buildingUpgradeTimerCached != pendingUpgradeTimer)
            {
                buildingUpgradeTimerCached = pendingUpgradeTimer;
                RefreshTimer(pendingUpgradeTimer, GameDataManager.Instance.buildingLv_Docks + 1);
            }
        }
        else
        {
            if (GameDataManager.Instance.resClay != cachedMaterialQuantities[0] ||
                GameDataManager.Instance.resLumber != cachedMaterialQuantities[1] ||
                GameDataManager.Instance.resOre != cachedMaterialQuantities[2] ||
                GameDataManager.Instance.resBricks != cachedMaterialQuantities[3] ||
                GameDataManager.Instance.resPlanks != cachedMaterialQuantities[4] ||
                GameDataManager.Instance.resMetal != cachedMaterialQuantities[5])
            {
                cachedMaterialQuantities = new int[] {GameDataManager.Instance.resClay, GameDataManager.Instance.resLumber, GameDataManager.Instance.resOre,
                                                                  GameDataManager.Instance.resBricks, GameDataManager.Instance.resPlanks, GameDataManager.Instance.resMetal};
                if (GameDataManager.Instance.CheckMaterialAvailability(costsLookupFunction(baseBuildingLv)))
                {
                    if (cachedDevStatus != DevStatusOptions.UpgradeReady)
                    {
                        cachedDevStatus = DevStatusOptions.UpgradeReady;
                        devStatusLabel.text = strings[12];
                    }

                }
                else if (cachedDevStatus != DevStatusOptions.InsufficientResources)
                {
                    cachedDevStatus = DevStatusOptions.InsufficientResources;
                    devStatusLabel.text = strings[15];
                }
            }
            if (!materialsNeededSection.activeInHierarchy && baseBuildingLv < TownBuilding.buildingTypeMaxLevels[(int)buildingType]) materialsNeededSection.SetActive(true);
            if (pendingUpgradeTimerSection.activeInHierarchy) pendingUpgradeTimerSection.SetActive(false);
        }
    }
}