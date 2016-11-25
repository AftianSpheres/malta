using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HousePopup : MonoBehaviour
{
    public TownBuilding associatedHouse;
    public Text nameLabel;
    public Text titleLabel;
    public Text attacksLabel;
    public Text specialLabel;
    public Text statsLabel;
    public TextAsset stringsResource;
    private int adventurerHPCached;
    private int adventurerMartialCached;
    private int adventurerMagicCached;
    private int adventurerSpeedCached;
    private string[] strings;

	// Use this for initialization
	void Start ()
    {
        strings = stringsResource.text.Split('\n');
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (associatedHouse.associatedAdventurer.fullName != nameLabel.text) nameLabel.text = associatedHouse.associatedAdventurer.fullName;
        if (associatedHouse.associatedAdventurer.title != titleLabel.text) titleLabel.text = associatedHouse.associatedAdventurer.title;
        if (adventurerHPCached != associatedHouse.associatedAdventurer.HP || adventurerMartialCached != associatedHouse.associatedAdventurer.Martial
        || adventurerMagicCached != associatedHouse.associatedAdventurer.Magic || adventurerSpeedCached != associatedHouse.associatedAdventurer.Speed)
        {
            adventurerHPCached = associatedHouse.associatedAdventurer.HP;
            adventurerMartialCached = associatedHouse.associatedAdventurer.Martial;
            adventurerMagicCached = associatedHouse.associatedAdventurer.Magic;
            adventurerSpeedCached = associatedHouse.associatedAdventurer.Speed;
            statsLabel.text = strings[0] + adventurerHPCached.ToString() + strings[1] + adventurerMartialCached.ToString() + strings[2] + adventurerMagicCached.ToString() + strings[3] + adventurerSpeedCached.ToString(); 
        }
	}
}
