using UnityEngine;
using UnityEngine.UI;

public class TowerPopup : MonoBehaviour
{
    public PopupMenu shell;
    public PopupMenu insufficientResourcesPopup;
    public Text headerLabel;
    public Text infoLabel;
    public Text resManaNo;
    public TextAsset stringsResource;
    public GameObject upgradeButton;
    private int cachedWizardsTowerLv = -1;
    private string[] strings;
    private const int baseResGainStringIndex = 0;

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
            if (upgradeButton.activeInHierarchy != (GameDataManager.Instance.dataStore.buildingLv_WizardsTower < TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Tower]))
            {
                upgradeButton.SetActive(GameDataManager.Instance.dataStore.buildingLv_WizardsTower < TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Tower]);
            }
            if (cachedWizardsTowerLv != GameDataManager.Instance.dataStore.buildingLv_WizardsTower)
            {
                cachedWizardsTowerLv = GameDataManager.Instance.dataStore.buildingLv_WizardsTower;
                headerLabel.text = strings[0] + cachedWizardsTowerLv.ToString();
                infoLabel.text = strings[baseResGainStringIndex + cachedWizardsTowerLv]; 
                resManaNo.text = TownBuilding.GetUpgradeCost_WizardsTower(cachedWizardsTowerLv).ToString();
            }
        }
	}

    public void UpgradeButtonInteraction ()
    {
        int cost = TownBuilding.GetUpgradeCost_WizardsTower(GameDataManager.Instance.dataStore.buildingLv_WizardsTower);
        if (GameDataManager.Instance.SpendManaIfPossible(cost))
        {
            if (GameDataManager.Instance.dataStore.buildingLv_WizardsTower < TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Tower]) GameDataManager.Instance.dataStore.buildingLv_WizardsTower++;
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }
}
