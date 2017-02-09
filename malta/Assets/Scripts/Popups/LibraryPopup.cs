using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class LibraryPopup : MonoBehaviour
{
    public PopupMenu shell;
    public PopupMenu insufficientResourcesPopup;
    public GameObject bookPanelPrototype;
    public RectTransform scrollAreaRect;
    public TextAsset stringsResource;
    public Sprite untranslatedIcon;
    public Sprite translatedIcon;
    private List<LibraryPopup_BookPanel> bookPanels;
    private WarriorPromotes cachedWp;
    private MysticPromotes cachedMp;
    private string[] strings;
    private float basicSize;

	// Use this for initialization
	void Start ()
    {
        basicSize = scrollAreaRect.sizeDelta.y;
        bookPanelPrototype.gameObject.SetActive(false);
        InitialPanelGen();
        strings = Util.GetLinesFrom(stringsResource);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if ((cachedWp & GameDataManager.Instance.dataStore.unlockedWarriorPromotes) != cachedWp || (cachedMp & GameDataManager.Instance.dataStore.unlockedMysticPromotes) != cachedMp) PopulateScrollArea();
	}

    void InitialPanelGen ()
    {
        bookPanelPrototype.SetActive(false);
        int bi = shell.buttons.Length;
        Button[] b = new Button[bi + 4];
        shell.buttons.CopyTo(b, 0);
        bookPanels = new List<LibraryPopup_BookPanel>();
        const int panelCnt = 4;
        for (int i = 0; i < panelCnt; i++)
        {
            LibraryPopup_BookPanel bp = ((GameObject)Instantiate(bookPanelPrototype, scrollAreaRect)).GetComponent<LibraryPopup_BookPanel>();
            bp.gameObject.name = "uninitialized panel - index " + i;
            bookPanels.Add(bp);
            b[bi + i] = bp.button;
        }
        shell.buttons = b;
    }

    void PopulateScrollArea ()
    {
        cachedWp = GameDataManager.Instance.dataStore.unlockedWarriorPromotes;
        cachedMp = GameDataManager.Instance.dataStore.unlockedMysticPromotes;
        int localIndex = 0;
        for (int i = 1; i > 1 << 31;)
        {
            if ((cachedWp & (WarriorPromotes)i) == (WarriorPromotes)i)
            {
                bookPanels[localIndex].gameObject.SetActive(true);
                _processTranslatedBookPanel(bookPanels[localIndex], Adventurer._warriorPromoteToAdvClass((WarriorPromotes)i));
                localIndex++;
                i = i << 1;
            }
        }
        for (int i = 1; i > 1 << 31;)
        {
            if ((cachedMp & (MysticPromotes)i) == (MysticPromotes)i)
            {
                bookPanels[localIndex].gameObject.SetActive(true);
                _processTranslatedBookPanel(bookPanels[localIndex], Adventurer._mysticPromoteToAdvClass((MysticPromotes)i));
                localIndex++;
                i = i << 1;
            }
        }
        if (GameDataManager.Instance.dataStore.nextPromoteUnlockBattles == 0)
        {
            LibraryPopup_BookPanel bp = bookPanels[localIndex];
            bp.gameObject.SetActive(true);
            bp.nameLabel.text = strings[0];
            bp.gameObject.name = "untranslated panel";
            if (GameDataManager.Instance.dataStore.nextWarriorPromote != WarriorPromotes.None) bp.classDesc.text = strings[2];
            else if (GameDataManager.Instance.dataStore.nextMysticPromote != MysticPromotes.None) bp.classDesc.text = strings[3];
            else bp.classDesc.text = strings[9];
            bp.button.gameObject.SetActive(true);
            bp.brickCnt.text = GameDataManager.Instance.dataStore.nextPromoteUnlockCosts[0].ToString();
            bp.plankCnt.text = GameDataManager.Instance.dataStore.nextPromoteUnlockCosts[1].ToString();
            bp.metalCnt.text = GameDataManager.Instance.dataStore.nextPromoteUnlockCosts[2].ToString();
            bp.icon.sprite = untranslatedIcon;
            localIndex++;
        }
        scrollAreaRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, localIndex * basicSize);
    }

    void _processTranslatedBookPanel (LibraryPopup_BookPanel bp, AdventurerClass advClass)
    {
        bp.button.gameObject.SetActive(false);
        bp.nameLabel.text = Adventurer.GetClassName(advClass) + strings[1];
        bp.classDesc.text = _bookFlavorTextForClass(advClass);
        bp.icon.sprite = translatedIcon;
        bp.gameObject.name = "translated panel - " + bp.nameLabel.text;
    }

    public void AttemptToPayForTranslation ()
    {
        if (GameDataManager.Instance.SpendResourcesIfPossible(GameDataManager.Instance.dataStore.nextPromoteUnlockCosts))
        {
            GameDataManager.Instance.UnlockNextPromote();
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    string _bookFlavorTextForClass (AdventurerClass advClass)
    {
        switch (advClass)
        {
            case AdventurerClass.Footman:
                return strings[4];
            case AdventurerClass.Bowman:
                return strings[5];
            case AdventurerClass.Sage:
                return strings[6];
            case AdventurerClass.Wizard:
                return strings[7];
            default:
                return strings[8];
        }
    }
}
