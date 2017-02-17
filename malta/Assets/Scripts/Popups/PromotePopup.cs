using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PromotePopup : MonoBehaviour
{
    public PopupMenu shell;
    public PopupMenu insufficientResourcesPopup;
    public TextAsset stringsResource;
    public Dropdown dd;
    private Adventurer adv;
    public Text promoteText;
    public Text reqsText;
    public Button confirmBtn;
    private List<AdventurerClass> advClasses;
    public AdventurerClass setClass;
    private string[] strings;
    private int cachedMana = -1;

	// Use this for initialization
	void Awake ()
    {
        strings = Util.GetLinesFrom(stringsResource);
        advClasses = new List<AdventurerClass>();
	}

    void Update ()
    {
        if (cachedMana != GameDataManager.Instance.dataStore.resMana)
        {
            int manaCost = Adventurer.GetPromoteCostForClass(setClass);
            cachedMana = GameDataManager.Instance.dataStore.resMana;
            reqsText.text = strings[3] + manaCost + strings[4];
            confirmBtn.interactable = (cachedMana >= manaCost);
        }
    }

    public void OpenOn (Adventurer _adv)
    {
        adv = _adv;
        shell.Open();
        if (_adv.advClass == AdventurerClass.Warrior) PopulateDropdownForWarrior();
        else if (_adv.advClass == AdventurerClass.Mystic) PopulateDropdownForMystic();
        else throw new System.Exception("Opened promote popup on unpromotable unit of class " + adv.advClass.ToString());
        if (dd.options.Count < 2) dd.interactable = false;
        else dd.interactable = true;
        SetClassSelectionBasedOnVal();
    }

    void PopulateDropdownForWarrior ()
    {
        dd.ClearOptions();
        List<Dropdown.OptionData> ddOpts = new List<Dropdown.OptionData>();
        List<WarriorPromotes> l = new List<WarriorPromotes>();
        for (int i = 1; i > 1 << 31; ) // whoops, bit shifting a signed int will do that, lol
        {
            if (GameDataManager.Instance.WarriorPromoteUnlocked((WarriorPromotes)i))
            {
                l.Add((WarriorPromotes)i);
            }
            i = i << 1;
        }
        for (int i = 0; i < l.Count; i++)
        {
            if (i >= ddOpts.Count)
            {
                advClasses.Add(AdventurerClass.None);
            }
            advClasses[i] = Adventurer._warriorPromoteToAdvClass(l[i]);
            ddOpts.Add(new Dropdown.OptionData(Adventurer.GetClassName(advClasses[i])));
        }
        dd.AddOptions(ddOpts);
    }

    void PopulateDropdownForMystic ()
    {
        dd.ClearOptions();
        List<Dropdown.OptionData> ddOpts = new List<Dropdown.OptionData>();
        List<MysticPromotes> l = new List<MysticPromotes>();
        for (int i = 1; i > 1 << 31;)
        {
            if (GameDataManager.Instance.MysticPromoteUnlocked((MysticPromotes)i))
            {
                l.Add((MysticPromotes)i);
            }
            i = i << 1;
        }
        for (int i = 0; i < l.Count; i++)
        {
            if (i >= ddOpts.Count)
            {
                advClasses.Add(AdventurerClass.None);
            }
            advClasses[i] = Adventurer._mysticPromoteToAdvClass(l[i]);
            ddOpts.Add(new Dropdown.OptionData(Adventurer.GetClassName(advClasses[i])));
        }
        dd.AddOptions(ddOpts);
    }

    public void SetClassSelectionBasedOnVal ()
    {
        SetClassSelection(advClasses[dd.value]);
    }

    void SetClassSelection (AdventurerClass _newClass)
    {
        setClass = _newClass;
        string className = Adventurer.GetClassName(setClass);
        string st2;
        if (Util.isVowel(className[0])) st2 = strings[2];
        else st2 = strings[1];
        promoteText.text = strings[0] + adv.fullName + st2 + className;
    }

    public void PromoteUnit ()
    {
        int manaCost = Adventurer.GetPromoteCostForClass(setClass);
        if (GameDataManager.Instance.SpendManaIfPossible(manaCost))
        {
            adv.PromoteToTier2(setClass);
            shell.Close();
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }
}
