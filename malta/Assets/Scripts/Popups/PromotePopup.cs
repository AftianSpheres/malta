using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PromotePopup : MonoBehaviour
{
    public PopupMenu shell;
    public PopupMenu insufficientResourcesPopup;
    public TextAsset stringsResource;
    public RectTransform dropdownOptsParent;
    public RectTransform scrollRectTransform;
    private Adventurer adv;
    public Text promoteText;
    public GameObject dropdownOptPrototype;
    private List<AdventurerClass> advClasses;
    private List<GameObject> ddOpts;
    private List<Text> unlockedClassLabels;
    private List<Toggle> toggles;
    private int validItems;
    public AdventurerClass setClass;
    private string[] strings;
    const int manaCost = 10;
    const float scrollRectSizePerItem = 28;
    private Vector3 dropdownOptSize;

	// Use this for initialization
	void Awake ()
    {
        strings = Util.GetLinesFrom(stringsResource);
        ddOpts = new List<GameObject>();
        //dropdownOptPrototype.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void OpenOn (Adventurer _adv)
    {
        adv = _adv;
        shell.Open();
        if (_adv.advClass == AdventurerClass.Warrior) PopulateDropdownForWarrior();
        else if (_adv.advClass == AdventurerClass.Mystic) PopulateDropdownForMystic();
        else throw new System.Exception("Opened promote popup on unpromotable unit of class " + adv.advClass.ToString());
    }

    GameObject _makeOption ()
    {
        return (GameObject)Instantiate(dropdownOptPrototype, dropdownOptsParent);
    }

    void PopulateDropdownForWarrior ()
    {
        List<WarriorPromotes> l = new List<WarriorPromotes>();
        for (int i = 1; i < 1 << 31; )
        {
            if (GameDataManager.Instance.WarriorPromoteUnlocked((WarriorPromotes)i))
            {
                l.Add((WarriorPromotes)i);
            }
            i = i << 1;
        }
        validItems = 0;
        for (int i = 0; i < l.Count; i++)
        {
            if (i >= ddOpts.Count)
            {
                ddOpts.Add(_makeOption());
                unlockedClassLabels.Add(ddOpts[i].transform.Find("Item Label").GetComponent<Text>());
                toggles.Add(ddOpts[i].GetComponent<Toggle>());
                advClasses.Add(AdventurerClass.None);
            }
            advClasses[i] = _warriorPromoteToAdvClass(l[i]);
            unlockedClassLabels[i].text = Adventurer.GetClassName(advClasses[i]);
            ddOpts[i].name = "ddOpt: " + unlockedClassLabels[i].text;
            ddOpts[i].SetActive(true);
            validItems++;
        }
        for (int i = l.Count; i < ddOpts.Count; i++)
        {
            ddOpts[i].name = "ddOpt: dead";
            ddOpts[i].SetActive(false);
        }
        RecalcAndPopulateScrollRect();
    }

    void PopulateDropdownForMystic ()
    {
        List<MysticPromotes> l = new List<MysticPromotes>();
        for (int i = 1; i < 1 << 31;)
        {
            if (GameDataManager.Instance.MysticPromoteUnlocked((MysticPromotes)i))
            {
                l.Add((MysticPromotes)i);
            }
            i = i << 1;
        }
        validItems = 0;
        for (int i = 0; i < l.Count; i++)
        {
            if (i >= ddOpts.Count)
            {
                ddOpts.Add(_makeOption());
                unlockedClassLabels.Add(ddOpts[i].transform.Find("Item Label").GetComponent<Text>());
                toggles.Add(ddOpts[i].GetComponent<Toggle>());
                advClasses.Add(AdventurerClass.None);
            }
            advClasses[i] = _mysticPromoteToAdvClass(l[i]);
            unlockedClassLabels[i].text = Adventurer.GetClassName(advClasses[i]);
            ddOpts[i].name = "ddOpt: " + unlockedClassLabels[i].text;
            ddOpts[i].SetActive(true);
            validItems++;
        }
        for (int i = l.Count; i < ddOpts.Count; i++)
        {
            ddOpts[i].name = "ddOpt: dead";
            ddOpts[i].SetActive(false);
        }
        RecalcAndPopulateScrollRect();
    }

    void RecalcAndPopulateScrollRect ()
    {
        for (int i = 0; i < validItems; i++) ddOpts[i].transform.SetParent(transform, true);
        dropdownOptsParent.SetParent(transform, true);
        scrollRectTransform.sizeDelta = dropdownOptsParent.sizeDelta = new Vector2(dropdownOptsParent.sizeDelta.x, scrollRectSizePerItem * validItems);
        scrollRectTransform.anchoredPosition = dropdownOptsParent.anchoredPosition = Vector2.zero;
        dropdownOptsParent.SetParent(scrollRectTransform, true);
        for (int i = 0; i < validItems; i++)
        {
            RectTransform rt = ddOpts[i].transform as RectTransform;
            rt.SetParent(dropdownOptsParent, true);
            rt.anchoredPosition = new Vector2(0, (Mathf.Abs(rt.sizeDelta.y) / 2) - (i * scrollRectSizePerItem));
        }
    }

    private AdventurerClass _warriorPromoteToAdvClass (WarriorPromotes wp)
    {
        switch (wp)
        {
            case WarriorPromotes.Bowman:
                return AdventurerClass.Bowman;
            case WarriorPromotes.Footman:
                return AdventurerClass.Footman;
        }
        throw new System.Exception(wp.ToString() + " doesn't correspond to a valid AdventurerClass. Which might just mean you need to add it to the lookup func???");
    }

    private AdventurerClass _mysticPromoteToAdvClass (MysticPromotes mp)
    {
        switch (mp)
        {
            case MysticPromotes.Sage:
                return AdventurerClass.Sage;
            case MysticPromotes.Wizard:
                return AdventurerClass.Wizard;
        }
        throw new System.Exception(mp.ToString() + "ain't a thing, etc. etc.");
    }

    public void SetClassSelectionBasedOnGameObjectRef (GameObject caller)
    {
        for (int i = 0; i < ddOpts.Count; i++)
        {
            if (ddOpts[i] == caller)
            {
                SetClassSelection(advClasses[i]);
                break;
            }
            if (i == ddOpts.Count) throw new System.Exception("Couldn't tie " + caller.name + " to a class..."); // if we get to the end of the search loop and it's not there...
        }
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
