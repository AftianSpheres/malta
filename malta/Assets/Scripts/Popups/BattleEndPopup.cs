using UnityEngine;
using UnityEngine.UI;

public class BattleEndPopup : MonoBehaviour
{
    public PopupMenu shell;
    public BattleOverseer battleOverseer;
    public GameObject announcementPanel;
    public Text announcementText;
    public Text nextDestText;
    public Text successDegreeText;
    public TextAsset stringsResource;
    public ScreenChanger screenChanger;
    private string[] strings;
    private bool opened = false;

    // Use this for initialization
    void Start ()
    {
        strings = stringsResource.text.Split('\n');
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (opened == false)
        {
            opened = true;
            if (GameDataManager.Instance.adventureLevel - 1 < AdventureSubstageLoader.randomAdventureBaseLevel) // adventureLevel is already incremented, we want to get the adventure we were on
            {
                nextDestText.text = strings[GameDataManager.Instance.adventureLevel - 1];
            }
            else nextDestText.text = strings[AdventureSubstageLoader.randomAdventureBaseLevel];
            switch (battleOverseer.playerDeaths)
            {
                case 0:
                    successDegreeText.text = strings[8];
                    break;
                case 1:
                    successDegreeText.text = strings[9] + battleOverseer.lastDeadPlayerAdvName + strings[10];
                    break;
                case 2:
                    successDegreeText.text = strings[11];
                    break;
                case 3:
                    successDegreeText.text = strings[12];
                    break;
            }
            if (GameDataManager.Instance.adventureLevel > AdventureSubstageLoader.randomAdventureBaseLevel)
            {
                Debug.Log("this isn't a thing yet");
            }
            else if (GameDataManager.Instance.adventureLevel == 2)
            {
                announcementPanel.SetActive(true);
                announcementText.text = strings[4];
            }
        }
	}

    public void ReturnFromBattle ()
    {
        shell.Close();
        screenChanger.Activate();
    }
}
