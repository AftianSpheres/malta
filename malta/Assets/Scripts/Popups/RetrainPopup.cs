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

    // Use this for initialization
    void Start ()
    {
        strings = stringsResource.text.Split('\n');
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (associatedAdventurer != null && !associatedAdventurer.isPromoted)
        {
            if (cachedName != associatedAdventurer.fullName)
            {
                cachedName = associatedAdventurer.fullName;
                inquiryLabel.text = strings[0] + cachedName + strings[1];
            }
            if (cachedClass != associatedAdventurer.baseClass)
            {
                cachedClass = associatedAdventurer.baseClass;
                AdventurerClass advC;
                if (cachedClass == AdventurerClass.Warrior) advC = AdventurerClass.Mystic;
                else advC = AdventurerClass.Warrior;
                reclassButtonLabel.text = strings[2] + Adventurer.GetClassName(advC);
            }
        }
	}

    public void ConfirmedRetrain ()
    {
        if (cachedClass == AdventurerClass.Warrior) associatedAdventurer.Reclass(AdventurerClass.Mystic);
        else associatedAdventurer.Reclass(AdventurerClass.Warrior);
        shell.Close();
    }
}
