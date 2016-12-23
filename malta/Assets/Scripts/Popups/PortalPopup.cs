using UnityEngine;
using UnityEngine.UI;

public class PortalPopup : MonoBehaviour
{
    public PopupMenu shell;
    public Button winButton;
    public PortalStatusPanel portalStatus;
    public TextAsset stringsResource;
    public Text portalText;
    public CutscenePlayer cutscenePlayer;
    private string[] strings;
    private const int overworldCutscenePlayerEndingIndex = 0;

	// Use this for initialization
	void Awake ()
    {
        strings = portalStatus.strings = stringsResource.text.Split('\n');
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (GameDataManager.Instance.dataStore.buildingLv_WizardsTower >= TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Tower])
        {
            winButton.gameObject.SetActive(true);
            if (portalText.text != strings[22]) portalText.text = strings[22];
        }
        else if (portalText.text != strings[21]) portalText.text = strings[21];
	}

    public void Win ()
    {
        cutscenePlayer.StartCutscene(overworldCutscenePlayerEndingIndex);
        shell.Close();
    }
}
