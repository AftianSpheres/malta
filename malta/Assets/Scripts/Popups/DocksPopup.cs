using UnityEngine;
using UnityEngine.UI;

public class DocksPopup : MonoBehaviour
{
    public PopupMenu shell;
    public GameObject upgradeButton;
    public Text headerArea;
    public Text infoArea;
    public Text timerArea;
    public Text bricksCount;
    public Text metalCount;
    public Text planksCount;
    public PopupMenu insufficientResourcesPopup;
    public TextAsset stringsResource;
    private string[] strings;
    private bool cachedPendingUpgradeStatus = false;
    private int cachedLv = -1;
    private int cachedPendingUpgradeTimer = -1;

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
            if (GameDataManager.Instance.dataStore.pendingUpgrade_Docks)
            {
                if (upgradeButton.activeInHierarchy) upgradeButton.SetActive(false);
                if (!timerArea.gameObject.activeInHierarchy) timerArea.gameObject.SetActive(true);
                if (cachedPendingUpgradeTimer != GameDataManager.Instance.dataStore.pendingUpgradeTimer_Docks)
                {
                    cachedPendingUpgradeTimer = GameDataManager.Instance.dataStore.pendingUpgradeTimer_Docks;
                    timerArea.text = PopupMenu.GetTimerReadout(GameDataManager.Instance.dataStore.pendingUpgradeTimer_Docks, GameDataManager.Instance.dataStore.buildingLv_Docks + 1);
                }
            }
            else if (timerArea.gameObject.activeInHierarchy) timerArea.gameObject.SetActive(false);
            if (cachedLv != GameDataManager.Instance.dataStore.buildingLv_Docks || cachedPendingUpgradeStatus != GameDataManager.Instance.dataStore.pendingUpgrade_Docks)
            {
                cachedLv = GameDataManager.Instance.dataStore.buildingLv_Docks;
                cachedPendingUpgradeStatus = GameDataManager.Instance.dataStore.pendingUpgrade_Docks;
                headerArea.text = strings[0] + GameDataManager.Instance.dataStore.buildingLv_Docks.ToString();
                string line0;
                if (GameDataManager.Instance.dataStore.pendingUpgrade_Docks) line0 = strings[4];
                else if (GameDataManager.Instance.dataStore.buildingLv_Docks > TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Docks]) line0 = strings[3];
                else line0 = strings[2];
                switch (GameDataManager.Instance.dataStore.buildingLv_Docks)
                {
                    case 0:
                        infoArea.text = line0 + System.Environment.NewLine + strings[5];
                        break;
                    case 1:
                        infoArea.text = line0 + System.Environment.NewLine + strings[6];
                        break;
                    case 2:
                        infoArea.text = line0 + System.Environment.NewLine + strings[7];
                        break;
                    case 3:
                        infoArea.text = line0 + System.Environment.NewLine + strings[8];
                        break;
                    case 4:
                        infoArea.text = line0 + System.Environment.NewLine + strings[9];
                        break;
                    case 5:
                        infoArea.text = line0 + System.Environment.NewLine + strings[10];
                        break;
                    case 6:
                        infoArea.text = line0 + System.Environment.NewLine + strings[11];
                        break;
                    case 7:
                        infoArea.text = line0 + System.Environment.NewLine + strings[12];
                        break;
                    case 8:
                        infoArea.text = line0 + System.Environment.NewLine + strings[13];
                        break;
                    case 9:
                        infoArea.text = line0 + System.Environment.NewLine + strings[14];
                        break;
                    case 10:
                        infoArea.text = line0 + System.Environment.NewLine + strings[15];
                        break;
                }
                if (GameDataManager.Instance.dataStore.buildingLv_Docks < TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Docks])
                {
                    if (!upgradeButton.activeInHierarchy) upgradeButton.SetActive(true);
                    int[] costs = TownBuilding.GetUpgradeCost_Docks(GameDataManager.Instance.dataStore.buildingLv_Docks);
                    bricksCount.text = costs[0].ToString();
                    planksCount.text = costs[1].ToString();
                    metalCount.text = costs[2].ToString();
                }
                else if (upgradeButton.activeInHierarchy) upgradeButton.SetActive(false);
            }
        }
    }

    public void UpgradeButtonInteraction()
    {
        if (GameDataManager.Instance.dataStore.buildingLv_Docks < TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Docks])
        {
            int[] costs = TownBuilding.GetUpgradeCost_Docks(GameDataManager.Instance.dataStore.buildingLv_Docks);
            if (GameDataManager.Instance.SpendResourcesIfPossible(costs))
            {
                GameDataManager.Instance.SetBuildingUpgradePending(BuildingType.Docks);
            }
            else
            {
                shell.SurrenderFocus();
                insufficientResourcesPopup.Open();
            }
        }
    }
}
