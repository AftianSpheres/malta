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
    public Button awakenButton;
    public Button outbuildingButton;
    public Image mugshot;
    public Text awakenedLabel;
    public Text biography;
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
    private AdventurerMugshot cachedAdventurerMugshot;
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
        if (cachedAdventurerMugshot != associatedHouse.associatedAdventurer.mugshot)
        {
            cachedAdventurerMugshot = associatedHouse.associatedAdventurer.mugshot;
            mugshot.sprite = associatedHouse.associatedAdventurer.GetMugshotGraphic();
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
#pragma warning disable 162 // VS reports a false-alarm unreachable code warning on the next line because of the compound loop conditional, so we silence that - and, yes, it is a false alarm
            for (int i = 0; (i < adventurerAttacksCached.Length && i < associatedHouse.associatedAdventurer.attacks.Length); i++)
#pragma warning restore 162 // because we _do_ want to know if unreachable code is detected, it's just not actually present there
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
        if (awakenButton != null)
        {
            if (associatedHouse.associatedAdventurer.isElite)
            {
                if ((associatedHouse.associatedAdventurer.awakened == awakenButton.gameObject.activeSelf))
                {
                    awakenButton.gameObject.SetActive(!associatedHouse.associatedAdventurer.awakened);
                    awakenedLabel.gameObject.SetActive(!awakenButton.gameObject.activeSelf);
                }
            }
            else if (awakenButton.gameObject.activeSelf || awakenedLabel.gameObject.activeSelf)
            {
                awakenButton.gameObject.SetActive(false);
                awakenedLabel.gameObject.SetActive(false);
            }
        }
        if (biography.text != associatedHouse.associatedAdventurer.bioText) biography.text = associatedHouse.associatedAdventurer.bioText;
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

    public void AwakenAdventurer ()
    {
        if (!associatedHouse.associatedAdventurer.awakened)
        {
            if (GameDataManager.Instance.SpendResourcesIfPossible(Adventurer.awakeningCosts))
            {
                associatedHouse.associatedAdventurer.Awaken();
            }
            else
            {
                shell.SurrenderFocus();
                insufficientResourcesPopup.Open();
            }
        }
    }
}
