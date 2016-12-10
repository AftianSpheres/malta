using UnityEngine;
using UnityEngine.UI;

public class AdventureTimePopup : MonoBehaviour
{
    public PopupMenu shell;
    public SovereignInfoPanel sovereignInfo;
    public GameObject matsNeededSection;
    public Text adventureDestination;
    public Text adventureDetails;
    public Text numBricks;
    public Text numMetal;
    public Text numPlanks;
    public TextAsset stringsResource;
    public ScreenChanger screenChanger;
    private int cachedAdventureLv = -1;
    private string[] strings;

	// Use this for initialization
	void Start ()
    {
        strings = sovereignInfo.strings = stringsResource.text.Split('\n');
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if (GameDataManager.Instance != null)
        {
            if (cachedAdventureLv != GameDataManager.Instance.adventureLevel)
            {
                cachedAdventureLv = GameDataManager.Instance.adventureLevel;
                switch (cachedAdventureLv)
                {
                    case 0:
                        adventureDestination.text = strings[23];
                        adventureDetails.text = strings[24];
                        break;
                    case 1:
                        adventureDestination.text = strings[25];
                        adventureDetails.text = strings[26];
                        break;
                    case 2:
                        adventureDestination.text = strings[27];
                        adventureDetails.text = strings[28];
                        break;
                    default:
                        adventureDestination.text = strings[29] + (cachedAdventureLv - 2).ToString();
                        adventureDetails.text = strings[30];
                        break;
                }
                if (cachedAdventureLv < 3) matsNeededSection.SetActive(false);
                else
                {
                    matsNeededSection.SetActive(true);
                    numBricks.text = numMetal.text = numPlanks.text = GameDataManager.Instance.nextRandomAdventureAnte.ToString();
                }
            }
        }
	}

    public void Depart ()
    {
        shell.Close();
        screenChanger.Activate();
    }
}
