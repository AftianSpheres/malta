using UnityEngine;
using UnityEngine.UI;

public class TowerPopup : MonoBehaviour
{
    public PopupMenu shell;
    public PopupMenu insufficientResourcesPopup;
    public Text headerLabel;
    public Text infoLabel;
    public Text resBrickNo;
    public Text resMetalNo;
    public Text resPlankNo;
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
            if (upgradeButton.activeInHierarchy != (GameDataManager.Instance.buildingLv_WizardsTower <= TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Tower]))
            {
                upgradeButton.SetActive(GameDataManager.Instance.buildingLv_WizardsTower < TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Tower]);
            }
            if (cachedWizardsTowerLv != GameDataManager.Instance.buildingLv_WizardsTower)
            {
                cachedWizardsTowerLv = GameDataManager.Instance.buildingLv_WizardsTower;
                headerLabel.text = strings[0] + cachedWizardsTowerLv.ToString();
                infoLabel.text = strings[baseResGainStringIndex + cachedWizardsTowerLv]; 
                int[] costs = TownBuilding.GetUpgradeCost_WizardsTower(cachedWizardsTowerLv);
                resBrickNo.text = costs[3].ToString();
                resPlankNo.text = costs[4].ToString();
                resMetalNo.text = costs[5].ToString();
            }
        }
	}

    public void UpgradeButtonInteraction ()
    {
        int[] costs = TownBuilding.GetUpgradeCost_WizardsTower(GameDataManager.Instance.buildingLv_WizardsTower);
        if (GameDataManager.Instance.SpendResourcesIfPossible(costs))
        {
            if (GameDataManager.Instance.buildingLv_WizardsTower < TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Tower]) GameDataManager.Instance.buildingLv_WizardsTower++;
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }
}
