using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RetrainPopup : MonoBehaviour
{
    public Adventurer associatedAdventurer;
    public PopupMenu shell;
    public Text inquiryLabel;
    public Text reclassButtonLabel;
    public TextAsset stringsResource;
    private string[] strings;
    private AdventurerClass cachedClass;
    private string cachedName = "Rock \"The Dwayne\" Johnson"; // this is literally anything namegen won't throw at you
    private static AdventurerClass[] warriorClassEvolutions = { AdventurerClass.Warrior, AdventurerClass.Bowman, AdventurerClass.Footman };
    private static AdventurerClass[] mysticClassEvolutions = { AdventurerClass.Mystic, AdventurerClass.Sage, AdventurerClass.Wizard };

    // Use this for initialization
    void Start ()
    {
        strings = stringsResource.text.Split('\n');
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (associatedAdventurer != null)
        {
            if (cachedName != associatedAdventurer.fullName)
            {
                cachedName = associatedAdventurer.fullName;
                inquiryLabel.text = strings[0] + cachedName + strings[1];
            }
            for (int i = 0; i < warriorClassEvolutions.Length; i++)
            {
                if (associatedAdventurer.advClass == warriorClassEvolutions[i])
                {
                    if (cachedClass != GameDataManager.Instance.mysticClassUnlock)
                    {
                        cachedClass = GameDataManager.Instance.mysticClassUnlock;
                        reclassButtonLabel.text = strings[2] + Adventurer.GetClassName(GameDataManager.Instance.mysticClassUnlock);
                    }
                    break;
                }
                else if (associatedAdventurer.advClass == mysticClassEvolutions[i])
                {
                    if (cachedClass != GameDataManager.Instance.warriorClassUnlock)
                    {
                        cachedClass = GameDataManager.Instance.warriorClassUnlock;
                        reclassButtonLabel.text = strings[2] + Adventurer.GetClassName(GameDataManager.Instance.warriorClassUnlock);
                    }
                    break;
                }
            }
        }
	}

    public void ConfirmedRetrain ()
    {
        associatedAdventurer.Reclass(cachedClass);
        shell.Close();
    }
}
