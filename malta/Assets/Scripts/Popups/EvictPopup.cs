using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EvictPopup : MonoBehaviour
{
    public Adventurer associatedAdventurer;
    public Button faeButton;
    public Button orcButton;
    public PopupMenu shell;
    public Text inquiryLabel;
    public TextAsset stringsResource;
    private string[] strings;
    private string cachedName = "Rock \"The Dwayne\" Johnson";

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
            if (associatedAdventurer != null && cachedName != associatedAdventurer.fullName)
            {
                cachedName = associatedAdventurer.fullName;
                inquiryLabel.text = strings[0] + cachedName + strings[1];
            }
            if (faeButton.gameObject.activeInHierarchy && !GameDataManager.Instance.dataStore.unlock_raceFae) faeButton.gameObject.SetActive(false);
            else if (!faeButton.gameObject.activeInHierarchy && GameDataManager.Instance.dataStore.unlock_raceFae) faeButton.gameObject.SetActive(true);
            if (orcButton.gameObject.activeInHierarchy && !GameDataManager.Instance.dataStore.unlock_raceOrc) orcButton.gameObject.SetActive(false);
            else if (!orcButton.gameObject.activeInHierarchy && GameDataManager.Instance.dataStore.unlock_raceOrc) orcButton.gameObject.SetActive(true);
        }
	}

    public void RerollAsHuman () // yes there have to be three of these - ugly af but Unity can't invoke functions with args from UI buttons
    {
        associatedAdventurer.Reroll(associatedAdventurer.advClass, AdventurerSpecies.Human, associatedAdventurer.isElite, Adventurer.GetRandomStatPoint());
        shell.Close();
    }

    public void RerollAsFae ()
    {
        associatedAdventurer.Reroll(associatedAdventurer.advClass, AdventurerSpecies.Fae, associatedAdventurer.isElite, Adventurer.GetRandomStatPoint());
        shell.Close();
    }

    public void RerollAsOrc ()
    {
        associatedAdventurer.Reroll(associatedAdventurer.advClass, AdventurerSpecies.Orc, associatedAdventurer.isElite, Adventurer.GetRandomStatPoint());
        shell.Close();
    }
}
