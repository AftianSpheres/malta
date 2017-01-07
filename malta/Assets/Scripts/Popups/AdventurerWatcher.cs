using UnityEngine;
using UnityEngine.UI;

public class AdventurerWatcher : MonoBehaviour
{
    public int houseAdventurerIndex;
    public Color defaultColor;
    public Color activeColor;
    public Button selfButton;
    public GameObject interior;
    public Image mugshot;
    public Image panelBG;
    public Text adventurerName;
    public Text adventurerTitle;
    public Text adventurerStats;
    public Text adventurerAttacks;
    public Text adventurerSpecial;
    private Adventurer adventurer;
    public TextAsset stringsResource;
    public HousePopup housePopup;
    public SwapAdventurerPopup swapPopup;
    private int[] cachedAdventurerStats = { -1, -1, -1, -1 };
    private AdventurerAttack[] cachedAdventurerAttacks = { AdventurerAttack.None, AdventurerAttack.None };
    private AdventurerSpecial cachedAdventurerSpecial = AdventurerSpecial.LoseBattle;
    private AdventurerMugshot cachedAdventurerMugshot = AdventurerMugshot.None;
    private string[] strings;

	// Use this for initialization
	void Start ()
    {
        strings = stringsResource.text.Split('\n');
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (adventurer == null)
        {     
            if (GameDataManager.Instance.dataStore.houseAdventurers[houseAdventurerIndex] != null && GameDataManager.Instance.dataStore.housingLevel > houseAdventurerIndex) adventurer = GameDataManager.Instance.dataStore.houseAdventurers[houseAdventurerIndex];
        }
        if (adventurer != null && adventurer.initialized)
        {
            if (panelBG != null)
            {
                if (housePopup != null)
                {
                    if (housePopup.inspectedAdventurer == adventurer) panelBG.color = activeColor;
                    else panelBG.color = defaultColor;
                }
                else if (swapPopup != null)
                {
                    if (swapPopup.partyMemberIndexInMainArray == houseAdventurerIndex) panelBG.color = activeColor;
                    else if (houseAdventurerIndex == GameDataManager.Instance.dataStore.partyAdventurer0Index || houseAdventurerIndex == GameDataManager.Instance.dataStore.partyAdventurer1Index || houseAdventurerIndex == GameDataManager.Instance.dataStore.partyAdventurer2Index)
                    {
                        panelBG.color = Color.clear;
                    }
                    else panelBG.color = defaultColor;
                }

            }
            if (selfButton != null)
            {
                if (housePopup != null)
                {
                    selfButton.interactable = (housePopup.inspectedAdventurer != adventurer);
                }
                else if (swapPopup != null)
                {
                    selfButton.interactable = swapPopup.partyMemberIndexInMainArray == houseAdventurerIndex || (houseAdventurerIndex != GameDataManager.Instance.dataStore.partyAdventurer0Index && houseAdventurerIndex != GameDataManager.Instance.dataStore.partyAdventurer1Index && houseAdventurerIndex != GameDataManager.Instance.dataStore.partyAdventurer2Index) ;
                }
            }
            if (interior != null)
            {
                if (!interior.activeInHierarchy) interior.SetActive(true);
            }

            if (adventurerName.text != adventurer.fullName) adventurerName.text = adventurer.fullName;
            if (adventurerTitle.text != adventurer.title) adventurerTitle.text = adventurer.title;
            if (cachedAdventurerMugshot != adventurer.mugshot)
            {
                cachedAdventurerMugshot = adventurer.mugshot;
                mugshot.sprite = adventurer.GetMugshotGraphic();
            }
            if (adventurerStats != null)
            {
                if (cachedAdventurerStats[0] != adventurer.HP || cachedAdventurerStats[1] != adventurer.Martial || cachedAdventurerStats[2] != adventurer.Magic || cachedAdventurerStats[3] != adventurer.Speed)
                {
                    cachedAdventurerStats = new int[] { adventurer.HP, adventurer.Martial, adventurer.Magic, adventurer.Speed };
                    adventurerStats.text = strings[0] + cachedAdventurerStats[0].ToString() + strings[1] + cachedAdventurerStats[1].ToString() + strings[2] +
                        cachedAdventurerStats[2].ToString() + strings[3] + cachedAdventurerStats[3].ToString();
                }
            }
            if (adventurerSpecial != null)
            {
                if (cachedAdventurerSpecial != adventurer.special)
                {
                    cachedAdventurerSpecial = adventurer.special;
                    adventurerSpecial.text = Adventurer.GetSpecialDescription(cachedAdventurerSpecial);
                }
            }
            if (adventurerAttacks != null)
            {
                if (cachedAdventurerAttacks[0] != adventurer.attacks[0] || cachedAdventurerAttacks[1] != adventurer.attacks[1])
                {
                    cachedAdventurerAttacks = adventurer.attacks.Clone() as AdventurerAttack[];
                    string[] attacksStrings = { "", "" };
                    for (int i = 0; i < cachedAdventurerAttacks.Length && i < 2; i++)
                    {
                        if (cachedAdventurerAttacks[i] != AdventurerAttack.None)
                        {
                            attacksStrings[i] = Adventurer.GetAttackName(cachedAdventurerAttacks[i]);
                        }
                        else
                        {
                            attacksStrings[i] = "";
                        }
                    }
                    adventurerAttacks.text = attacksStrings[0] + System.Environment.NewLine + attacksStrings[1];
                }
            }
        }
        else if (interior != null && interior.activeInHierarchy) interior.SetActive(false);
	}

    public void TakeAdvFocus ()
    {
        housePopup.inspectedAdventurer = adventurer;
        housePopup.advIndex = houseAdventurerIndex;
        GameDataManager.Instance.dataStore.lastInspectedAdventurerIndex = houseAdventurerIndex;
    }

    public void SwapAdventurer ()
    {
        swapPopup.SwapFor(houseAdventurerIndex);
        swapPopup.shell.Close();
    }

    public void ChangeDisplayedAdventurer (int _advIndex)
    {
        houseAdventurerIndex = _advIndex;
        adventurer = GameDataManager.Instance.dataStore.houseAdventurers[houseAdventurerIndex];
    }
}
