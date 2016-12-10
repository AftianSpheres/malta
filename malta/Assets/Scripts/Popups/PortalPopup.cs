using UnityEngine;
using UnityEngine.UI;

public class PortalPopup : MonoBehaviour
{
    public Button winButton;
    public PortalStatusPanel portalStatus;
    public TextAsset stringsResource;
    public Text portalText;
    private string[] strings;

	// Use this for initialization
	void Awake ()
    {
        strings = portalStatus.strings = stringsResource.text.Split('\n');
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (GameDataManager.Instance.buildingLv_WizardsTower >= TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Tower])
        {
            winButton.gameObject.SetActive(true);
            if (portalText.text != strings[22]) portalText.text = strings[22];
        }
        else if (portalText.text != strings[21]) portalText.text = strings[21];
	}
}
