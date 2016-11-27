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
    private int[] cachedMaterialQuantities = { 0, 0, 0, 0, 0, 0 };
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
                case BuildingType.Docks:
                    UpdateProcessing_UpgradableBuildings(ref GameDataManager.Instance.pendingUpgrade_Docks, ref GameDataManager.Instance.pendingUpgradeTimer_Docks, 
                        ref GameDataManager.Instance.buildingLv_Docks, 22, TownBuilding.GetUpgradeCost_Docks);
                    break;
                case BuildingType.Mason:
                    UpdateProcessing_UpgradableBuildings(ref GameDataManager.Instance.pendingUpgrade_Mason, ref GameDataManager.Instance.pendingUpgradeTimer_Mason,
                        ref GameDataManager.Instance.buildingLv_Mason, 23, TownBuilding.GetUpgradeCost_Mason);
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
        float trueSeconds = timer / level;
        int minutes = Mathf.FloorToInt(trueSeconds / 60);
        int seconds = Mathf.CeilToInt(trueSeconds % 60);
        string secString;
        if (seconds > 9) secString = seconds.ToString();
        else secString = "0" + seconds.ToString();
        timerCounter.text = minutes.ToString() + ":" + secString;
    }

    private void UpdateProcessing_UpgradableBuildings (ref bool pendingUpgrade, ref int pendingUpgradeTimer, ref int baseBuildingLv, int nameStringIndex, Func<int[]> costsLookupFunction)
    {
        if (baseBuildingLv != buildingLevelCached)
        {
            buildingLevelCached = baseBuildingLv;
            devTypeLabel.text = strings[nameStringIndex] + strings[baseBuildingLv];
            RefreshResourceCounters(costsLookupFunction());
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
                if (GameDataManager.Instance.CheckMaterialAvailability(costsLookupFunction()))
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
                    devStatusLabel.text = strings[14];
                }
            }
            if (!materialsNeededSection.activeInHierarchy && baseBuildingLv < TownBuilding.buildingTypeMaxLevels[(int)buildingType]) materialsNeededSection.SetActive(true);
            if (pendingUpgradeTimerSection.activeInHierarchy) pendingUpgradeTimerSection.SetActive(false);
        }
    }
}
