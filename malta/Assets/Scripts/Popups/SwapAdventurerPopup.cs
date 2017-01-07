using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// There are a lot of things here that are rapidly-hacked-together piles of shit! But this is the most rapid and the most shitty.
/// </summary>
public class SwapAdventurerPopup : MonoBehaviour
{
    public PopupMenu shell;
    public int partySlot;
    public int partyMemberIndexInMainArray;
    public RectTransform scrollAreaRect;
    public GameObject advPanelPrototype;
    public GameObject advPanelsParent;
    public AdventurerWatcher[] advWatchers;
    int houseLvCached = -1;
    const float advPanelHeight = 60.6f;
    const float advPanelsSpacing = 4;

    // Use this for initialization
    void Start ()
    {
        advWatchers = new AdventurerWatcher[GameDataManager.Instance.dataStore.houseAdventurers.Length];
        Button[] sb = new Button[shell.buttons.Length + advWatchers.Length];
        shell.buttons.CopyTo(sb, 0);
        for (int i = 0; i < GameDataManager.Instance.dataStore.houseAdventurers.Length; i++)
        {
            GameObject go = (GameObject)Instantiate(advPanelPrototype, transform);
            go.name = "advPanel" + i;
            advWatchers[i] = go.GetComponent<AdventurerWatcher>();
            advWatchers[i].houseAdventurerIndex = i;
            sb[shell.buttons.Length + i] = advWatchers[i].selfButton;
        }
        shell.buttons = sb;
        advPanelPrototype.SetActive(false);
        RecalcScrollRectSize();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (GameDataManager.Instance.dataStore.housingLevel != houseLvCached) RecalcScrollRectSize();
    }

    void RecalcScrollRectSize()
    {
        for (int i = 0; i < advWatchers.Length; i++) advWatchers[i].transform.SetParent(transform, true);
        houseLvCached = GameDataManager.Instance.dataStore.housingLevel;
        scrollAreaRect.sizeDelta = new Vector2(scrollAreaRect.sizeDelta.x, ((GameDataManager.Instance.dataStore.housingLevel) * advPanelHeight) + ((GameDataManager.Instance.dataStore.housingLevel - 1) * advPanelsSpacing));
        scrollAreaRect.anchoredPosition = Vector3.zero;
        for (int i = 0; i < advWatchers.Length; i++)
        {
            RectTransform rt = advWatchers[i].transform as RectTransform;
            rt.SetParent(advPanelsParent.transform, true);
            rt.anchoredPosition = new Vector2(0, (Mathf.Abs(rt.sizeDelta.y) / 2) - (i * (advPanelHeight + advPanelsSpacing))); // I don't pretend to understand this. rects are a mystery. why can't you be nice and clunky low-level stuff, rects?
        }
    }

    public void SwapFor (int adventurerIndex)
    {
        GameDataManager.Instance.SetPartyMember(partySlot, adventurerIndex);
    }
}
