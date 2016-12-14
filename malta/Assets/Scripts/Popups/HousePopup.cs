using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HousePopup : MonoBehaviour
{
    public TownBuilding associatedHouse;
    public PopupMenu shell;
    public PopupMenu insufficientResourcesPopup;
    public RetrainPopup retrainPopup;
    public EvictPopup evictPopup;
    public Button outbuildingButton;
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
    private AdventurerAttack[] adventurerAttacksCached;
    private AdventurerSpecial adventurerSpecialCached = AdventurerSpecial.UninitializedValue;
    private string cachedName;
    private string[] strings;

	// Use this for initialization
	void Start ()
    {
        strings = stringsResource.text.Split('\n');
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (outbuildingButton != null)
        {
            if (associatedHouse.hasOutbuilding && outbuildingButton.IsActive()) outbuildingButton.gameObject.SetActive(false);
            else if (!associatedHouse.hasOutbuilding && !outbuildingButton.IsActive()) outbuildingButton.gameObject.SetActive(true);
        }
        if (associatedHouse.associatedAdventurer.fullName != cachedName)
        {
            cachedName = associatedHouse.associatedAdventurer.fullName;
            nameLabel.text = strings[4] + associatedHouse.associatedAdventurer.fullName;
        }
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
        bool attacksChanged = false;
        if (adventurerAttacksCached == null || adventurerAttacksCached.Length != associatedHouse.associatedAdventurer.attacks.Length) attacksChanged = true;
        else
        {
            for (int i = 0; (i < adventurerAttacksCached.Length && i < associatedHouse.associatedAdventurer.attacks.Length); i++)
            {
                attacksChanged = true;
                break;
            }
        }
        if (attacksChanged)
        {
            string[] attacksStrings = { "", "", "" };
            adventurerAttacksCached = associatedHouse.associatedAdventurer.attacks;
            for (int i = 0; i < adventurerAttacksCached.Length && i < 3; i++)
            {
                if (associatedHouse.associatedAdventurer.attacks[i] != AdventurerAttack.None)
                {
                    attacksStrings[i] = Adventurer.GetAttackName(associatedHouse.associatedAdventurer.attacks[i]);
                }
                else
                {
                    attacksStrings[i] = "";
                }
            }
            attacksLabel.text = attacksStrings[0] + '\n' + attacksStrings[1] + '\n' + attacksStrings[2];
        }
        if (adventurerSpecialCached != associatedHouse.associatedAdventurer.special)
        {
            adventurerSpecialCached = associatedHouse.associatedAdventurer.special;
            specialLabel.text = Adventurer.GetSpecialDescription(adventurerSpecialCached);
        }
	}

    public void OpenRetrainPopup()
    {
        retrainPopup.shell.Open();
        retrainPopup.associatedAdventurer = associatedHouse.associatedAdventurer;
        shell.SurrenderFocus();
    }

    public void OpenEvictPopup()
    {
        evictPopup.shell.Open();
        evictPopup.associatedAdventurer = associatedHouse.associatedAdventurer;
        shell.SurrenderFocus();
    }

    public void AddOutbuilding()
    {
        if (GameDataManager.Instance.SpendResourcesIfPossible(0, 0, 0, 20, 20, 20))
        {
            associatedHouse.BuildOutbuilding();
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }
}
