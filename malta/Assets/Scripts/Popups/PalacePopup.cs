using UnityEngine;
using UnityEngine.UI;
using System.Collections;

enum PortalNextSteps
{
    None,
    NoTower,
    TowerPreLv7_NotEnoughAny,
    TowerPreLv7_ReadyForUpgrade,
    TowerPreLv7_NotEnoughBrick,
    TowerPreLv7_NotEnoughMetal,
    TowerPreLv7_NotEnoughPlanks,
    TowerLv7_NotEnoughAny,
    TowerLv7_ReadyForUpgrade,
    TowerLv7_NotEnoughBrick,
    TowerLv7_NotEnoughMetal,
    TowerLv7_NotEnoughPlanks,
    TowerDone
}

enum PortalStatus
{
    None,
    NoTower,
    TowerLv1,
    TowerLv2,
    TowerLv4,
    TowerLv7,
    TowerLv10
}

public class PalacePopup : MonoBehaviour
{
    public TownBuilding[] houses;
    public Text incomeBrick;
    public Text incomeClay;
    public Text incomeMetal;
    public Text incomeOre;
    public Text incomePlanks;
    public Text incomeWood;
    public Text portalNextStepsArea;
    public Text portalStatusArea;
    public Text sovereignNameLabel;
    public Text sovereignSpecialDesc;
    public Text sovereignTitleArea;
    public Text sovereignAttacksArea;
    public Text sovereignStatsArea;
    public Text sovereignTacticDesc;
    public Text matsNeededBrick;
    public Text matsNeededClay;
    public Text matsNeededMetal;
    public Text matsNeededOre;
    public Text matsNeededPlanks;
    public Text matsNeededWood;
    public GameObject matsNeededSection;
    public PopupMenu shell;
    public PopupMenu docksPopup;
    public PopupMenu masonPopup;
    public PopupMenu sawmillPopup;
    public PopupMenu smithPopup;
    public PopupMenu sovereignSpecialPopup;
    public PopupMenu sovereignTacticsPopup;
    public TextAsset stringsResource;
    private string cachedSovereignName = "¡²¤€¼½¾‘’¥×äåé®®þüúíó«»áß";
    private string cachedSovereignTitle = "¡²¤€¼½¾‘’¥×äåé®®þüúíó«»áß";
    private int cachedRateBrick = -1;
    private int cachedRateClay = -1;
    private int cachedRateMetal = -1;
    private int cachedRateOre = -1;
    private int cachedRatePlanks = -1;
    private int cachedRateWood = -1;
    private PortalNextSteps cachedPortalNextSteps;
    private PortalStatus cachedPortalStatus;
    private int[] cachedSovereignStats = { -1, -1, -1, -1 };
    private AdventurerAttack cachedSovereignTactic = AdventurerAttack.None;
    private AdventurerAttack[] cachedSovereignAttacks = { AdventurerAttack.None, AdventurerAttack.None };
    private AdventurerSpecial cachedSovereignSpecial = AdventurerSpecial.LoseBattle;
    private string[] strings;
    private const string dividerString = " | ";

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
            UpdateProcessing_PortalArea();
            UpdateProcessing_SovereignInfo();
        }
	}

    public void OpenDocksPopup ()
    {
        shell.Close();
        docksPopup.Open();
    }

    public void OpenForgePopup ()
    {
        Debug.Log("Not yet implemented");
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

    public void OpenWizardsTowerPopup()
    {
        Debug.Log("no");
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

    private void UpdateProcessing_PortalArea ()
    {
        if (!GameDataManager.Instance.unlock_WizardsTower)
        {
            if (cachedPortalStatus != PortalStatus.NoTower)
            {
                cachedPortalStatus = PortalStatus.NoTower;
                portalStatusArea.text = strings[6];
            }
            if (cachedPortalNextSteps != PortalNextSteps.NoTower)
            {
                cachedPortalNextSteps = PortalNextSteps.NoTower;
                portalNextStepsArea.text = strings[12];
            }
            if (matsNeededSection.activeInHierarchy) matsNeededSection.SetActive(false);
        }
        else
        {
            if (GameDataManager.Instance.buildingLv_WizardsTower == 10)
            {
                if (matsNeededSection.activeInHierarchy) matsNeededSection.SetActive(false);
            }
            else if (!matsNeededSection.activeInHierarchy) matsNeededSection.SetActive(true);
            switch (GameDataManager.Instance.buildingLv_WizardsTower)
            {
                case 1:
                    if (cachedPortalStatus != PortalStatus.TowerLv1)
                    {
                        cachedPortalStatus = PortalStatus.TowerLv1;
                        portalStatusArea.text = strings[7];
                    }
                    _in_UpdateProcessing_PortalArea_preLv7();
                    break;
                case 2:
                case 3:
                    if (cachedPortalStatus != PortalStatus.TowerLv2)
                    {
                        cachedPortalStatus = PortalStatus.TowerLv2;
                        portalStatusArea.text = strings[8];
                    }
                    _in_UpdateProcessing_PortalArea_preLv7();
                    break;
                case 4:
                case 5:
                case 6:
                    if (cachedPortalStatus != PortalStatus.TowerLv4)
                    {
                        cachedPortalStatus = PortalStatus.TowerLv4;
                        portalStatusArea.text = strings[9];
                    }
                    _in_UpdateProcessing_PortalArea_preLv7();
                    break;
                case 7:
                case 8:
                case 9:
                    if (cachedPortalStatus != PortalStatus.TowerLv7)
                    {
                        cachedPortalStatus = PortalStatus.TowerLv7;
                        portalStatusArea.text = strings[10];
                    }
                    _in_UpdateProcessing_PortalArea_postLv7();
                    break;
                case 10:
                    if (cachedPortalStatus != PortalStatus.TowerLv10)
                    {
                        cachedPortalStatus = PortalStatus.TowerLv10;
                        portalStatusArea.text = strings[11];
                    }
                    if (cachedPortalNextSteps != PortalNextSteps.TowerDone)
                    {
                        cachedPortalNextSteps = PortalNextSteps.TowerDone;
                        portalNextStepsArea.text = strings[20];
                    }
                    break;
            }
        }
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

    private void _in_UpdateProcessing_PortalArea_reqs (ref int[] reqs)
    {
        matsNeededClay.text = reqs[0].ToString();
        matsNeededWood.text = reqs[1].ToString();
        matsNeededOre.text = reqs[2].ToString();
        matsNeededBrick.text = reqs[3].ToString();
        matsNeededMetal.text = reqs[4].ToString();
        matsNeededPlanks.text = reqs[5].ToString();
    }

    private void _in_UpdateProcessing_PortalArea_preLv7 ()
    {
        int[] recs = TownBuilding.GetUpgradeCost_WizardsTower(GameDataManager.Instance.buildingLv_WizardsTower);
        _in_UpdateProcessing_PortalArea_reqs(ref recs);
        if (GameDataManager.Instance.CheckMaterialAvailability(recs))
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerPreLv7_ReadyForUpgrade)
            {
                portalNextStepsArea.text = strings[13] + " " + strings[15];
                cachedPortalNextSteps = PortalNextSteps.TowerPreLv7_ReadyForUpgrade;
            }
        }
        else if (GameDataManager.Instance.resBricks >= recs[3] && GameDataManager.Instance.resPlanks >= recs[4])
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerPreLv7_NotEnoughMetal)
            {
                portalNextStepsArea.text = strings[13] + " " + strings[18];
                cachedPortalNextSteps = PortalNextSteps.TowerPreLv7_NotEnoughMetal;
            }

        }
        else if (GameDataManager.Instance.resMetal >= recs[5] && GameDataManager.Instance.resPlanks >= recs[4])
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerPreLv7_NotEnoughBrick)
            {
                portalNextStepsArea.text = strings[13] + " " + strings[17];
                cachedPortalNextSteps = PortalNextSteps.TowerPreLv7_NotEnoughBrick;
            }
        }
        else if (GameDataManager.Instance.resMetal >= recs[5] && GameDataManager.Instance.resBricks >= recs[3])
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerPreLv7_NotEnoughPlanks)
            {
                portalNextStepsArea.text = strings[13] + " " + strings[19];
                cachedPortalNextSteps = PortalNextSteps.TowerPreLv7_NotEnoughPlanks;
            }

        }
        else if (cachedPortalNextSteps != PortalNextSteps.TowerPreLv7_NotEnoughAny)
        {
            portalNextStepsArea.text = strings[13] + " " + strings[16];
            cachedPortalNextSteps = PortalNextSteps.TowerPreLv7_NotEnoughAny;
        }
    }

    private void _in_UpdateProcessing_PortalArea_postLv7()
    {
        int[] recs = TownBuilding.GetUpgradeCost_WizardsTower(GameDataManager.Instance.buildingLv_WizardsTower);
        _in_UpdateProcessing_PortalArea_reqs(ref recs);
        if (GameDataManager.Instance.CheckMaterialAvailability(recs))
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerLv7_ReadyForUpgrade)
            {
                portalNextStepsArea.text = strings[14] + " " + strings[15];
                cachedPortalNextSteps = PortalNextSteps.TowerLv7_ReadyForUpgrade;
            }
        }
        else if (GameDataManager.Instance.resBricks >= recs[3] && GameDataManager.Instance.resPlanks >= recs[4])
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerLv7_NotEnoughMetal)
            {
                portalNextStepsArea.text = strings[14] + " " + strings[18];
                cachedPortalNextSteps = PortalNextSteps.TowerLv7_NotEnoughMetal;
            }

        }
        else if (GameDataManager.Instance.resMetal >= recs[5] && GameDataManager.Instance.resPlanks >= recs[4])
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerLv7_NotEnoughBrick)
            {
                portalNextStepsArea.text = strings[14] + " " + strings[17];
                cachedPortalNextSteps = PortalNextSteps.TowerLv7_NotEnoughBrick;
            }
        }
        else if (GameDataManager.Instance.resMetal >= recs[5] && GameDataManager.Instance.resBricks >= recs[3])
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerLv7_NotEnoughPlanks)
            {
                portalNextStepsArea.text = strings[14] + " " + strings[19];
                cachedPortalNextSteps = PortalNextSteps.TowerLv7_NotEnoughPlanks;
            }

        }
        else if (cachedPortalNextSteps != PortalNextSteps.TowerLv7_NotEnoughAny)
        {
            portalNextStepsArea.text = strings[14] + " " + strings[16];
            cachedPortalNextSteps = PortalNextSteps.TowerLv7_NotEnoughAny;
        }
    }
}
