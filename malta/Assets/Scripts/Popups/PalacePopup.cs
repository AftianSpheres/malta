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
    public PopupMenu shell;
    public TextAsset stringsResource;
    private string cachedSovereignName = "¡²¤€¼½¾‘’¥×äåé®®þüúíó«»áß";
    private int cachedRateBrick = -1;
    private int cachedRateClay = -1;
    private int cachedRateMetal = -1;
    private int cachedRateOre = -1;
    private int cachedRatePlanks = -1;
    private int cachedRateWood = -1;
    private string[] strings;

	// Use this for initialization
	void Start ()
    {
        strings = stringsResource.text.Split('\n');
    }

    // Update is called once per frame
    void Update ()
    {
	    if (GameDataManager.Instance != null)
        {
            UpdateProcessing_IncomeArea();
            UpdateProcessing_SovereignInfo();
        }
	}

    public void OpenDocksPopup ()
    {
        Debug.Log("Not yet implemented");
    }

    public void OpenMasonPopup ()
    {
        Debug.Log("Not yet implemented");
    }

    public void OpenSawmillPopup ()
    {
        Debug.Log("Not yet implemented");
    }

    public void OpenSmithPopup ()
    {
        Debug.Log("Not yet implemented");
    }

    public void OpenClayPitPopup ()
    {
        Debug.Log("ffs we don't even have a peninsular map scene yet");
    }

    public void OpenMinesPopup()
    {
        Debug.Log("ffs we don't even have a peninsular map scene yet");
    }

    public void OpenWoodlandsPopup()
    {
        Debug.Log("ffs we don't even have a peninsular map scene yet");
    }

    public void OpenHousePopup (int index)
    {
        shell.Close();
        houses[index].OpenPopupOnBuilding();
    }

    private void UpdateProcessing_SovereignInfo ()
    {
        if (cachedSovereignName != GameDataManager.Instance.sovereignName)
        {
            cachedSovereignName = GameDataManager.Instance.sovereignName;
            sovereignNameLabel.text = strings[0] + GameDataManager.Instance.sovereignName;
        }
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
            UItext.text = cached.ToString() + strings[3];
        }
    }
}
