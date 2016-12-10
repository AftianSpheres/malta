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
    public PortalStatusPanel portalStatus;
    public SovereignInfoPanel sovereignInfo;
    public PopupMenu shell;
    public PopupMenu docksPopup;
    public PopupMenu forgePopup;
    public PopupMenu masonPopup;
    public PopupMenu sawmillPopup;
    public PopupMenu smithPopup;
    public TextAsset stringsResource;
    public GameObject wtSiteButton;
    private int cachedRateBrick = -1;
    private int cachedRateClay = -1;
    private int cachedRateMetal = -1;
    private int cachedRateOre = -1;
    private int cachedRatePlanks = -1;
    private int cachedRateWood = -1;
    private string[] strings;

	// Use this for initialization
	void Awake ()
    {
        strings = portalStatus.strings = sovereignInfo.strings = stringsResource.text.Split('\n');
    }

    // Update is called once per frame
    void Update ()
    {
	    if (GameDataManager.Instance != null)
        {
            UpdateProcessing_IncomeArea();
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

    private void UpdateProcessing_IncomeArea ()
    {
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Bricks), ref cachedRateBrick, incomeBrick);
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Clay), ref cachedRateClay, incomeClay);
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Metal), ref cachedRateMetal, incomeMetal);
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Ore), ref cachedRateOre, incomeOre);
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Planks), ref cachedRatePlanks, incomePlanks);
        _in_UpdateProcessing_IncomeArea(GameDataManager.Instance.GetResourceGainRate(ResourceType.Lumber), ref cachedRateWood, incomeWood);
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
