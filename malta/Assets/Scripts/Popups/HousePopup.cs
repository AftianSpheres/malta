using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HousePopup : MonoBehaviour
{
    public RectTransform scrollAreaRect;
    public GameObject advPanelPrototype;
    public GameObject advPanelsParent;
    public AdventurerWatcher[] houseAdvWatchers;
    public Adventurer inspectedAdventurer;
    public int advIndex;
    const float advPanelHeight = 60.6f;
    const float advPanelsSpacing = 4;
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
    private int houseLvCached;
    private AdventurerAttack[] adventurerAttacksCached;
    private AdventurerSpecial adventurerSpecialCached = AdventurerSpecial.UninitializedValue;
    private AdventurerMugshot cachedAdventurerMugshot;
    private string cachedName;
    private string[] strings;
    private bool housingUnitUpgraded { get { return GameDataManager.Instance.dataStore.housingUnitUpgrades[advIndex]; } set { GameDataManager.Instance.dataStore.housingUnitUpgrades[advIndex] = value; } }

	// Use this for initialization
	void Start ()
    {
        strings = stringsResource.text.Split('\n');    
        houseAdvWatchers = new AdventurerWatcher[GameDataManager.Instance.dataStore.houseAdventurers.Length];
        Vector3 panelPos = advPanelPrototype.transform.position;
        Button[] sb = new Button[shell.buttons.Length + houseAdvWatchers.Length];
        shell.buttons.CopyTo(sb, 0);
        for (int i = 0; i < GameDataManager.Instance.dataStore.houseAdventurers.Length; i++)
        {
            GameObject go = (GameObject)Instantiate(advPanelPrototype, transform);
            go.name = "advPanel" + i;
            houseAdvWatchers[i] = go.GetComponent<AdventurerWatcher>();
            houseAdvWatchers[i].houseAdventurerIndex = i;
            sb[shell.buttons.Length + i] = houseAdvWatchers[i].selfButton;
        }
        shell.buttons = sb;
        advPanelPrototype.SetActive(false);
        RecalcScrollRectSize();
        inspectedAdventurer = GameDataManager.Instance.dataStore.houseAdventurers[GameDataManager.Instance.dataStore.lastInspectedAdventurerIndex];
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (GameDataManager.Instance.dataStore.housingLevel != houseLvCached) RecalcScrollRectSize();
        if (outbuildingButton != null)
        {
            if (housingUnitUpgraded && outbuildingButton.IsActive()) outbuildingButton.gameObject.SetActive(false);
            else if (!housingUnitUpgraded && !outbuildingButton.IsActive()) outbuildingButton.gameObject.SetActive(true);
        }
        if (inspectedAdventurer.fullName != cachedName)
        {
            cachedName = inspectedAdventurer.fullName;
            nameLabel.text = strings[4] + inspectedAdventurer.fullName;
        }
        if (cachedAdventurerMugshot != inspectedAdventurer.mugshot)
        {
            cachedAdventurerMugshot = inspectedAdventurer.mugshot;
            mugshot.sprite = inspectedAdventurer.GetMugshotGraphic();
        }
        if (inspectedAdventurer.title != titleLabel.text) titleLabel.text = inspectedAdventurer.title;
        if (adventurerHPCached != inspectedAdventurer.HP || adventurerMartialCached != inspectedAdventurer.Martial
        || adventurerMagicCached != inspectedAdventurer.Magic || adventurerSpeedCached != inspectedAdventurer.Speed)
        {
            adventurerHPCached = inspectedAdventurer.HP;
            adventurerMartialCached = inspectedAdventurer.Martial;
            adventurerMagicCached = inspectedAdventurer.Magic;
            adventurerSpeedCached = inspectedAdventurer.Speed;
            statsLabel.text = strings[0] + adventurerHPCached.ToString() + strings[1] + adventurerMartialCached.ToString() + strings[2] + adventurerMagicCached.ToString() + strings[3] + adventurerSpeedCached.ToString(); 
        }
        bool attacksChanged = false;
        if (adventurerAttacksCached == null || adventurerAttacksCached.Length != inspectedAdventurer.attacks.Length) attacksChanged = true;
        else
        {
#pragma warning disable 162 // VS reports a false-alarm unreachable code warning on the next line because of the compound loop conditional, so we silence that - and, yes, it is a false alarm
            for (int i = 0; (i < adventurerAttacksCached.Length && i < inspectedAdventurer.attacks.Length); i++)
#pragma warning restore 162 // because we _do_ want to know if unreachable code is detected, it's just not actually present there
            {
                attacksChanged = true;
                break;
            }
        }
        if (attacksChanged)
        {
            string[] attacksStrings = { "", "", "" };
            adventurerAttacksCached = inspectedAdventurer.attacks;
            for (int i = 0; i < adventurerAttacksCached.Length && i < 3; i++)
            {
                if (inspectedAdventurer.attacks[i] != AdventurerAttack.None)
                {
                    attacksStrings[i] = Adventurer.GetAttackName(inspectedAdventurer.attacks[i]);
                }
                else
                {
                    attacksStrings[i] = "";
                }
            }
            attacksLabel.text = attacksStrings[0] + '\n' + attacksStrings[1] + '\n' + attacksStrings[2];
        }
        if (adventurerSpecialCached != inspectedAdventurer.special)
        {
            adventurerSpecialCached = inspectedAdventurer.special;
            specialLabel.text = Adventurer.GetSpecialDescription(adventurerSpecialCached);
        }
        if (awakenButton != null)
        {
            if (inspectedAdventurer.isElite)
            {
                if ((inspectedAdventurer.awakened == awakenButton.gameObject.activeSelf))
                {
                    awakenButton.gameObject.SetActive(!inspectedAdventurer.awakened);
                    awakenedLabel.gameObject.SetActive(!awakenButton.gameObject.activeSelf);
                }
            }
            else if (awakenButton.gameObject.activeSelf || awakenedLabel.gameObject.activeSelf)
            {
                awakenButton.gameObject.SetActive(false);
                awakenedLabel.gameObject.SetActive(false);
            }
        }
        if (biography.text != inspectedAdventurer.bioText) biography.text = inspectedAdventurer.bioText;
	}

    public void OpenRetrainPopup()
    {
        retrainPopup.shell.Open();
        retrainPopup.associatedAdventurer = inspectedAdventurer;
        shell.SurrenderFocus();
    }

    public void OpenEvictPopup()
    {
        evictPopup.shell.Open();
        evictPopup.associatedAdventurer = inspectedAdventurer;
        shell.SurrenderFocus();
    }

    public void AddOutbuilding()
    {
        if (GameDataManager.Instance.SpendResourcesIfPossible(20, 20, 20))
        {
            housingUnitUpgraded = true;
            inspectedAdventurer.Promote();
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    public void AwakenAdventurer ()
    {
        if (!inspectedAdventurer.awakened)
        {
            if (GameDataManager.Instance.SpendResourcesIfPossible(Adventurer.awakeningCosts))
            {
                inspectedAdventurer.Awaken();
            }
            else
            {
                shell.SurrenderFocus();
                insufficientResourcesPopup.Open();
            }
        }
    }

    void RecalcScrollRectSize ()
    {
        for (int i = 0; i < houseAdvWatchers.Length; i++) houseAdvWatchers[i].transform.SetParent(transform, true);
        houseLvCached = GameDataManager.Instance.dataStore.housingLevel;
        scrollAreaRect.sizeDelta = new Vector2(scrollAreaRect.sizeDelta.x, ((GameDataManager.Instance.dataStore.housingLevel) * advPanelHeight) + ((GameDataManager.Instance.dataStore.housingLevel - 1) * advPanelsSpacing));
        scrollAreaRect.anchoredPosition = Vector3.zero;
        for (int i = 0; i < houseAdvWatchers.Length; i++)
        {
            RectTransform rt = houseAdvWatchers[i].transform as RectTransform;
            rt.SetParent(advPanelsParent.transform, true);
            rt.anchoredPosition = new Vector2(0, (Mathf.Abs(rt.sizeDelta.y) / 2) - (i * (advPanelHeight + advPanelsSpacing))); // I don't pretend to understand this. rects are a mystery. why can't you be nice and clunky low-level stuff, rects?
        }
    }
}
