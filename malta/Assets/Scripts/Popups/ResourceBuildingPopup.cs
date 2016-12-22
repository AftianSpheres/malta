using UnityEngine;
using UnityEngine.UI;

public class ResourceBuildingPopup : MonoBehaviour
{
    public PopupMenu shell;
    public BuildingType buildingType;
    public GameObject upgradeButton;
    public Text capUpButtonText;
    public Text headerArea;
    public Text infoArea;
    public Text timerArea;
    public Text upgradeButtonText;
    public PopupMenu insufficientResourcesPopup;
    public TextAsset stringsResource;
    private bool pendingUpgrade
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.Mason:
                    return GameDataManager.Instance.dataStore.pendingUpgrade_Mason;
                case BuildingType.Smith:
                    return GameDataManager.Instance.dataStore.pendingUpgrade_Smith;
                case BuildingType.Sawmill:
                    return GameDataManager.Instance.dataStore.pendingUpgrade_Sawmill;
                case BuildingType.ClayPit:
                    return GameDataManager.Instance.dataStore.pendingUpgrade_ClayPit;
                case BuildingType.Mine:
                    return GameDataManager.Instance.dataStore.pendingUpgrade_Mine;
                case BuildingType.Woodlands:
                    return GameDataManager.Instance.dataStore.pendingUpgrade_Woodlands;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
        }
    }
    private int buildingLv { get
        {
            switch (buildingType)
            {
                case BuildingType.Mason:
                    return GameDataManager.Instance.dataStore.buildingLv_Mason;
                case BuildingType.Smith:
                    return GameDataManager.Instance.dataStore.buildingLv_Smith;
                case BuildingType.Sawmill:
                    return GameDataManager.Instance.dataStore.buildingLv_Sawmill;
                case BuildingType.ClayPit:
                    return GameDataManager.Instance.dataStore.harvestLv_ClayPit;
                case BuildingType.Mine:
                    return GameDataManager.Instance.dataStore.harvestLv_Mine;
                case BuildingType.Woodlands:
                    return GameDataManager.Instance.dataStore.harvestLv_Woodlands;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
        }
    }
    private int buildingNameStringIndex
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.Mason:
                    return 0;
                case BuildingType.Smith:
                    return 1;
                case BuildingType.Sawmill:
                    return 2;
                case BuildingType.ClayPit:
                    return 3;
                case BuildingType.Mine:
                    return 4;
                case BuildingType.Woodlands:
                    return 5;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
        }
    }
    private int buildingUpgradeStringIndex
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.Mason:
                    return 15;
                case BuildingType.Smith:
                    return 16;
                case BuildingType.Sawmill:
                    return 17;
                case BuildingType.ClayPit:
                    return 18;
                case BuildingType.Mine:
                    return 19;
                case BuildingType.Woodlands:
                    return 20;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
        }
    }
    private int capUpgrades
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.Mason:
                    return GameDataManager.Instance.dataStore.resBricks_maxUpgrades;
                case BuildingType.Smith:
                    return GameDataManager.Instance.dataStore.resMetal_maxUpgrades;
                case BuildingType.Sawmill:
                    return GameDataManager.Instance.dataStore.resPlanks_maxUpgrades;
                case BuildingType.ClayPit:
                    return GameDataManager.Instance.dataStore.resClay_maxUpgrades;
                case BuildingType.Mine:
                    return GameDataManager.Instance.dataStore.resOre_maxUpgrades;
                case BuildingType.Woodlands:
                    return GameDataManager.Instance.dataStore.resLumber_maxUpgrades;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
        }
    }
    private int pendingUpgradeTimer
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.Mason:
                    return GameDataManager.Instance.dataStore.pendingUpgradeTimer_Mason;
                case BuildingType.Smith:
                    return GameDataManager.Instance.dataStore.pendingUpgradeTimer_Smith;
                case BuildingType.Sawmill:
                    return GameDataManager.Instance.dataStore.pendingUpgradeTimer_Sawmill;
                case BuildingType.ClayPit:
                    return GameDataManager.Instance.dataStore.pendingUpgradeTimer_ClayPit;
                case BuildingType.Mine:
                    return GameDataManager.Instance.dataStore.pendingUpgradeTimer_Mine;
                case BuildingType.Woodlands:
                    return GameDataManager.Instance.dataStore.pendingUpgradeTimer_Woodlands;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
        }
    }
    private int resourceCap
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.Mason:
                    return GameDataManager.Instance.dataStore.resBricks_max;
                case BuildingType.Smith:
                    return GameDataManager.Instance.dataStore.resMetal_max;
                case BuildingType.Sawmill:
                    return GameDataManager.Instance.dataStore.resPlanks_max;
                case BuildingType.ClayPit:
                    return GameDataManager.Instance.dataStore.resClay_max;
                case BuildingType.Mine:
                    return GameDataManager.Instance.dataStore.resOre_max;
                case BuildingType.Woodlands:
                    return GameDataManager.Instance.dataStore.resLumber_max;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
        }
    }
    private int resourceCount
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.Mason:
                    return GameDataManager.Instance.dataStore.resBricks;
                case BuildingType.Smith:
                    return GameDataManager.Instance.dataStore.resMetal;
                case BuildingType.Sawmill:
                    return GameDataManager.Instance.dataStore.resPlanks;
                case BuildingType.ClayPit:
                    return GameDataManager.Instance.dataStore.resClay;
                case BuildingType.Mine:
                    return GameDataManager.Instance.dataStore.resOre;
                case BuildingType.Woodlands:
                    return GameDataManager.Instance.dataStore.resLumber;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
        }
    }
    private ResourceType resource
    {
        get
        {
            switch (buildingType)
            {
                case BuildingType.Mason:
                    return ResourceType.Bricks;
                case BuildingType.Smith:
                    return ResourceType.Metal;
                case BuildingType.Sawmill:
                    return ResourceType.Planks;
                case BuildingType.ClayPit:
                    return ResourceType.Clay;
                case BuildingType.Mine:
                    return ResourceType.Ore;
                case BuildingType.Woodlands:
                    return ResourceType.Lumber;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
        }
    }
    private string buildingCapUpStringLine2
    {
        get
        {
            int[] costs;
            string line1;
            switch (buildingType)
            {
                case BuildingType.Mason:
                case BuildingType.ClayPit:
                    costs = TownBuilding.GetCapUpCost_MasonClaypit(buildingLv);
                    if (costs[3] == 0) // no brick cost
                    {
                        line1 = strings[21] + costs[0].ToString() + strings[22];
                    }
                    else if (costs[0] == 0)
                    {
                        line1 = strings[21] + costs[3].ToString() + strings[28];
                    }
                    else
                    {
                        line1 = strings[21] + costs[0].ToString() + strings[25] + costs[3].ToString() + strings[28];
                    }
                    break;
                case BuildingType.Smith:
                case BuildingType.Mine:
                    costs = TownBuilding.GetCapUpCost_SmithMines(buildingLv);
                    if (costs[5] == 0)
                    {
                        line1 = strings[21] + costs[2].ToString() + strings[23];
                    }
                    else if (costs[2] == 0)
                    {
                        line1 = strings[21] + costs[5].ToString() + strings[29];
                    }
                    else
                    {
                        line1 = strings[21] + costs[2].ToString() + strings[26] + costs[5].ToString() + strings[29];
                    }
                    break;
                case BuildingType.Sawmill:
                case BuildingType.Woodlands:
                    costs = TownBuilding.GetCapUpCost_SawmillWoodlands(buildingLv);
                    if (costs[4] == 0)
                    {
                        line1 = strings[21] + costs[1].ToString() + strings[24];
                    }
                    else if (costs[1] == 0)
                    {
                        line1 = strings[21] + costs[4].ToString() + strings[30];
                    }
                    else
                    {
                        line1 = strings[21] + costs[1].ToString() + strings[27] + costs[4].ToString() + strings[30];
                    }
                    break;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
            return line1;
        }
    }
    private string buildingUpgradeStringLine1
    {
        get
        {
            int[] costs;
            string line1;
            switch (buildingType)
            {
                case BuildingType.Mason:
                case BuildingType.ClayPit:
                    costs = TownBuilding.GetUpgradeCost_MasonClaypit(buildingLv);
                    if (costs[3] == 0) // no brick cost
                    {
                        line1 = strings[21] + costs[0].ToString() + strings[22];
                    }
                    else if (costs[0] == 0)
                    {
                        line1 = strings[21] + costs[3].ToString() + strings[28];
                    }
                    else
                    {
                        line1 = strings[21] + costs[0].ToString() + strings[25] + costs[3].ToString() + strings[28];
                    }
                    break;
                case BuildingType.Smith:
                case BuildingType.Mine:
                    costs = TownBuilding.GetUpgradeCost_SmithMines(buildingLv);
                    if (costs[5] == 0)
                    {
                        line1 = strings[21] + costs[2].ToString() + strings[23];
                    }
                    else if (costs[2] == 0)
                    {
                        line1 = strings[21] + costs[5].ToString() + strings[29];
                    }
                    else
                    {
                        line1 = strings[21] + costs[2].ToString() + strings[26] + costs[5].ToString() + strings[29];
                    }
                    break;
                case BuildingType.Sawmill:
                case BuildingType.Woodlands:
                    costs = TownBuilding.GetUpgradeCost_SawmillWoodlands(buildingLv);
                    if (costs[4] == 0)
                    {
                        line1 = strings[21] + costs[1].ToString() + strings[24];
                    }
                    else if (costs[1] == 0)
                    {
                        line1 = strings[21] + costs[4].ToString() + strings[30];
                    }
                    else
                    {
                        line1 = strings[21] + costs[1].ToString() + strings[27] + costs[4].ToString() + strings[30];
                    }
                    break;
                default:
                    throw new System.Exception("Opened resource building popup with bad buildingtype: " + buildingType.ToString());
            }
            return line1;
        }
    }
    private string[] strings;
    private bool cachedPendingUpgradeStatus = false;
    private int cachedLv = -1;
    private int cachedRate = -1;
    private int cachedPendingUpgradeTimer = -1;
    private int cachedUpgrades = -1;

    // Use this for initialization
    void Awake ()
    {
        strings = stringsResource.text.Split('\n');
    }

    // Update is called once per frame
    void Update ()
    {
        if (GameDataManager.Instance != null)
        {
            if (pendingUpgrade)
            {
                if (upgradeButton.activeInHierarchy) upgradeButton.SetActive(false);
                if (!timerArea.gameObject.activeInHierarchy) timerArea.gameObject.SetActive(true);
                if (cachedPendingUpgradeTimer != pendingUpgradeTimer)
                {
                    cachedPendingUpgradeTimer = pendingUpgradeTimer;
                    timerArea.text = PopupMenu.GetTimerReadout(pendingUpgradeTimer, GameDataManager.Instance.dataStore.buildingLv_Docks + 1);
                }
            }
            else if (timerArea.gameObject.activeInHierarchy) timerArea.gameObject.SetActive(false);
            if (cachedLv != buildingLv || cachedUpgrades != capUpgrades || cachedRate != GameDataManager.Instance.GetResourceGainRate(resource) || cachedPendingUpgradeStatus != pendingUpgrade)
            {
                cachedLv = buildingLv;
                cachedUpgrades = capUpgrades;
                cachedPendingUpgradeStatus = pendingUpgrade;
                cachedRate = GameDataManager.Instance.GetResourceGainRate(resource);
                headerArea.text = strings[buildingNameStringIndex] + strings[6] + buildingLv.ToString();
                string line0;
                if (pendingUpgrade) line0 = strings[9];
                else if (buildingLv >= TownBuilding.buildingTypeMaxLevels[(int)buildingType]) line0 = strings[8];
                else line0 = strings[7];
                infoArea.text = line0 + System.Environment.NewLine +
                                strings[10] + cachedRate.ToString() + strings[11] + System.Environment.NewLine +
                                strings[12] + resourceCap.ToString() + strings[13] + capUpgrades.ToString() + strings[14];
                if (buildingLv < TownBuilding.buildingTypeMaxLevels[(int)buildingType])
                {
                    if (!upgradeButton.activeInHierarchy) upgradeButton.SetActive(true);
                    upgradeButtonText.text = strings[buildingUpgradeStringIndex] + System.Environment.NewLine + buildingUpgradeStringLine1;
                }
                else if (upgradeButton.activeInHierarchy) upgradeButton.SetActive(false);
                capUpButtonText.text = strings[31] + System.Environment.NewLine + strings[32] + System.Environment.NewLine + buildingCapUpStringLine2;
            }
        }
	}

    public void CapUpButtonInteraction ()
    {
        int[] costs;
        switch (buildingType)
        {
            case BuildingType.Mason:
            case BuildingType.ClayPit:
                costs = TownBuilding.GetCapUpCost_MasonClaypit(buildingLv);
                break;
            case BuildingType.Smith:
            case BuildingType.Mine:
                costs = TownBuilding.GetCapUpCost_SmithMines(buildingLv);
                break;
            case BuildingType.Sawmill:
            case BuildingType.Woodlands:
                costs = TownBuilding.GetCapUpCost_SawmillWoodlands(buildingLv);
                break;
            default:
                throw new System.Exception(buildingType.ToString() + " isn't a resource building!");
        }
        if (GameDataManager.Instance.SpendResourcesIfPossible(costs))
        {
            GameDataManager.Instance.UpgradeResourceCap(resource);
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    public void UpgradeButtonInteraction ()
    {
        if (buildingLv < TownBuilding.buildingTypeMaxLevels[(int)buildingType]) // this prevents a probable crash that would otherwise be possible for exactly one frame
        {
            int[] costs;
            switch (buildingType)
            {
                case BuildingType.Mason:
                case BuildingType.ClayPit:
                    costs = TownBuilding.GetUpgradeCost_MasonClaypit(buildingLv);
                    break;
                case BuildingType.Smith:
                case BuildingType.Mine:
                    costs = TownBuilding.GetUpgradeCost_SmithMines(buildingLv);
                    break;
                case BuildingType.Sawmill:
                case BuildingType.Woodlands:
                    costs = TownBuilding.GetUpgradeCost_SawmillWoodlands(buildingLv);
                    break;
                default:
                    throw new System.Exception(buildingType.ToString() + " isn't a resource building!");
            }
            if (GameDataManager.Instance.SpendResourcesIfPossible(costs))
            {
                GameDataManager.Instance.SetBuildingUpgradePending(buildingType);
            }
            else
            {
                shell.SurrenderFocus();
                insufficientResourcesPopup.Open();
            }
        }
    }
}