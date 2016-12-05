using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PalacePopup : MonoBehaviour
{
    public TownBuilding[] houses;
    public Text incomeBrick;
    public Text incomeClay;
    public Text incomeMetal;
    public Text incomeOre;
    public Text incomePlanks;
    public Text incomeWood;
    public Text sovereignNameLabel;
    public Text sovereignSpecialDesc;
    public Text sovereignTitleArea;
    public Text sovereignAttacksArea;
    public Text sovereignStatsArea;
    public Text sovereignTacticDesc;
    public PortalStatusPanel portalStatus;
    public PopupMenu shell;
    public PopupMenu docksPopup;
    public PopupMenu forgePopup;
    public PopupMenu masonPopup;
    public PopupMenu sawmillPopup;
    public PopupMenu smithPopup;
    public PopupMenu sovereignSpecialPopup;
    public PopupMenu sovereignTacticsPopup;
    public TextAsset stringsResource;
    public GameObject wtSiteButton;
    private string cachedSovereignName = "¡²¤€¼½¾‘’¥×äåé®®þüúíó«»áß";
    private string cachedSovereignTitle = "¡²¤€¼½¾‘’¥×äåé®®þüúíó«»áß";
    private int cachedRateBrick = -1;
    private int cachedRateClay = -1;
    private int cachedRateMetal = -1;
    private int cachedRateOre = -1;
    private int cachedRatePlanks = -1;
    private int cachedRateWood = -1;
    private int[] cachedSovereignStats = { -1, -1, -1, -1 };
    private AdventurerAttack cachedSovereignTactic = AdventurerAttack.None;
    private AdventurerAttack[] cachedSovereignAttacks = { AdventurerAttack.None, AdventurerAttack.None };
    private AdventurerSpecial cachedSovereignSpecial = AdventurerSpecial.LoseBattle;
    private string[] strings;
    private const string dividerString = " | ";

	// Use this for initialization
	void Start ()
    {
        strings = portalStatus.strings = stringsResource.text.Split('\n');
    }

    // Update is called once per frame
    void Update ()
    {
	    if (GameDataManager.Instance != null)
        {
            UpdateProcessing_IncomeArea();
            UpdateProcessing_SovereignInfo();
            if ((GameDataManager.Instance.unlock_WizardsTower && (GameDataManager.Instance.buildingLv_WizardsTower < TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Tower])) != wtSiteButton.activeInHierarchy)
                wtSiteButton.SetActive(GameDataManager.Instance.unlock_WizardsTower && (GameDataManager.Instance.buildingLv_WizardsTower < TownBuilding.buildingTypeMaxLevels[(int)BuildingType.Tower]));
        }
	}

    public void OpenDocksPopup ()
    {
        shell.Close();
        docksPopup.Open();
    }

    public void OpenForgePopup ()
    {
        shell.Close();
        forgePopup.Open();
    }

    public void OpenMasonPopup ()
    {
        shell.Close();
        masonPopup.Open();
    }

    public void OpenSawmillPopup ()
    {
        shell.Close();
        sawmillPopup.Open();
    }

    public void OpenSmithPopup ()
    {
        shell.Close();
        smithPopup.Open();
    }

    public void OpenClayPitPopup ()
    {
        shell.Close();
        LevelLoadManager.Instance.EnterLevel(SceneIDType.OverworldScene, BuildingType.ClayPit);
    }

    public void OpenMinesPopup()
    {
        shell.Close();
        LevelLoadManager.Instance.EnterLevel(SceneIDType.OverworldScene, BuildingType.Mine);
    }

    public void OpenWoodlandsPopup()
    {
        shell.Close();
        LevelLoadManager.Instance.EnterLevel(SceneIDType.OverworldScene, BuildingType.Woodlands);
    }

    public void OpenWizardsTowerPopup()
    {
        shell.Close();
        LevelLoadManager.Instance.EnterLevel(SceneIDType.OverworldScene, BuildingType.Tower);
    }

    public void OpenHousePopup (int index)
    {
        shell.Close();
        houses[index].OpenPopupOnBuilding();
    }

    public void OpenSovereignSpecialPopup ()
    {
        shell.SurrenderFocus();
        sovereignSpecialPopup.Open();
    }

    public void OpenSovereignTacticsPopup ()
    {
        shell.SurrenderFocus();
        sovereignTacticsPopup.Open();
    }

    private void UpdateProcessing_IncomeArea ()
    {
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Bricks), ref cachedRateBrick, incomeBrick);
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Clay), ref cachedRateClay, incomeClay);
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Metal), ref cachedRateMetal, incomeMetal);
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Ore), ref cachedRateOre, incomeOre);
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Planks), ref cachedRatePlanks, incomePlanks);
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Lumber), ref cachedRateWood, incomeWood);
    }

    private void UpdateProcessing_SovereignInfo ()
    {
        if (cachedSovereignName != GameDataManager.Instance.sovereignName)
        {
            cachedSovereignName = GameDataManager.Instance.sovereignName;
            sovereignNameLabel.text = strings[0] + GameDataManager.Instance.sovereignName;
        }
        if (cachedSovereignTactic != GameDataManager.Instance.sovereignTactic)
        {
            cachedSovereignTactic = GameDataManager.Instance.sovereignTactic;
            sovereignTacticDesc.text = Adventurer.GetAttackDescription(GameDataManager.Instance.sovereignTactic);
        }
        if (cachedSovereignSpecial != GameDataManager.Instance.sovereignSkill)
        {
            cachedSovereignSpecial = GameDataManager.Instance.sovereignSkill;
            sovereignSpecialDesc.text = Adventurer.GetSpecialDescription(GameDataManager.Instance.sovereignSkill);
        }
        if (cachedSovereignTitle != GameDataManager.Instance.sovereignAdventurer.fullTitle)
        {
            cachedSovereignTitle = GameDataManager.Instance.sovereignAdventurer.fullTitle;
            sovereignTitleArea.text = cachedSovereignTitle;
        }
        if (cachedSovereignStats[0] != GameDataManager.Instance.sovereignAdventurer.HP ||
            cachedSovereignStats[1] != GameDataManager.Instance.sovereignAdventurer.Martial ||
            cachedSovereignStats[2] != GameDataManager.Instance.sovereignAdventurer.Magic ||
            cachedSovereignStats[3] != GameDataManager.Instance.sovereignAdventurer.Speed)
        {
            cachedSovereignStats = new int[] { GameDataManager.Instance.sovereignAdventurer.HP, GameDataManager.Instance.sovereignAdventurer.Martial,
                GameDataManager.Instance.sovereignAdventurer.Magic, GameDataManager.Instance.sovereignAdventurer.Speed };
            sovereignStatsArea.text = strings[2] + cachedSovereignStats[0] + dividerString + strings[3] + cachedSovereignStats[1] + dividerString +
                                      strings[4] + cachedSovereignStats[2] + dividerString + strings[5] + cachedSovereignStats[3];
        }
        if (cachedSovereignAttacks[0] != GameDataManager.Instance.sovereignAdventurer.attacks[0] ||
            cachedSovereignAttacks[1] != GameDataManager.Instance.sovereignAdventurer.attacks[1])
        {
            cachedSovereignAttacks = GameDataManager.Instance.sovereignAdventurer.attacks.Clone() as AdventurerAttack[];
            if (cachedSovereignAttacks[1] == AdventurerAttack.None) sovereignAttacksArea.text = Adventurer.GetAttackName(cachedSovereignAttacks[0]);
            else sovereignAttacksArea.text = Adventurer.GetAttackName(cachedSovereignAttacks[0]) + dividerString + Adventurer.GetAttackName(cachedSovereignAttacks[1]);
        }


    }

    private void _in_UpdateProcessing_IncomeArea (int rate, ref int cached, Text UItext)
    {
        if (cached != rate)
        {
            cached = rate;
            UItext.text = cached.ToString() + strings[1];
        }
    }
}
