using UnityEngine;
using UnityEngine.UI;

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

public class PortalStatusPanel : MonoBehaviour
{
    public GameObject matsNeededSection;
    public Text matsNeededBrick;
    public Text matsNeededClay;
    public Text matsNeededMetal;
    public Text matsNeededOre;
    public Text matsNeededPlanks;
    public Text matsNeededWood;
    public Text portalStatusArea;
    public Text portalNextStepsArea;
    public string[] strings;
    private PortalNextSteps cachedPortalNextSteps;
    private PortalStatus cachedPortalStatus;
	
	// Update is called once per frame
	void Update ()
    {
	    if (GameDataManager.Instance != null)
        {
            if (!GameDataManager.Instance.dataStore.unlock_WizardsTower)
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
                if (GameDataManager.Instance.dataStore.buildingLv_WizardsTower == 10)
                {
                    if (matsNeededSection.activeInHierarchy) matsNeededSection.SetActive(false);
                }
                else if (!matsNeededSection.activeInHierarchy) matsNeededSection.SetActive(true);
                switch (GameDataManager.Instance.dataStore.buildingLv_WizardsTower)
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
	}

    private void _in_UpdateProcessing_PortalArea_preLv7()
    {
        int[] recs = TownBuilding.GetUpgradeCost_WizardsTower(GameDataManager.Instance.dataStore.buildingLv_WizardsTower);
        _in_UpdateProcessing_PortalArea_reqs(ref recs);
        if (GameDataManager.Instance.CheckMaterialAvailability(recs))
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerPreLv7_ReadyForUpgrade)
            {
                portalNextStepsArea.text = strings[13] + " " + strings[15];
                cachedPortalNextSteps = PortalNextSteps.TowerPreLv7_ReadyForUpgrade;
            }
        }
        else if (GameDataManager.Instance.dataStore.resBricks >= recs[3] && GameDataManager.Instance.dataStore.resPlanks >= recs[4])
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerPreLv7_NotEnoughMetal)
            {
                portalNextStepsArea.text = strings[13] + " " + strings[18];
                cachedPortalNextSteps = PortalNextSteps.TowerPreLv7_NotEnoughMetal;
            }

        }
        else if (GameDataManager.Instance.dataStore.resMetal >= recs[5] && GameDataManager.Instance.dataStore.resPlanks >= recs[4])
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerPreLv7_NotEnoughBrick)
            {
                portalNextStepsArea.text = strings[13] + " " + strings[17];
                cachedPortalNextSteps = PortalNextSteps.TowerPreLv7_NotEnoughBrick;
            }
        }
        else if (GameDataManager.Instance.dataStore.resMetal >= recs[5] && GameDataManager.Instance.dataStore.resBricks >= recs[3])
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
        int[] recs = TownBuilding.GetUpgradeCost_WizardsTower(GameDataManager.Instance.dataStore.buildingLv_WizardsTower);
        _in_UpdateProcessing_PortalArea_reqs(ref recs);
        if (GameDataManager.Instance.CheckMaterialAvailability(recs))
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerLv7_ReadyForUpgrade)
            {
                portalNextStepsArea.text = strings[14] + " " + strings[15];
                cachedPortalNextSteps = PortalNextSteps.TowerLv7_ReadyForUpgrade;
            }
        }
        else if (GameDataManager.Instance.dataStore.resBricks >= recs[3] && GameDataManager.Instance.dataStore.resPlanks >= recs[4])
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerLv7_NotEnoughMetal)
            {
                portalNextStepsArea.text = strings[14] + " " + strings[18];
                cachedPortalNextSteps = PortalNextSteps.TowerLv7_NotEnoughMetal;
            }

        }
        else if (GameDataManager.Instance.dataStore.resMetal >= recs[5] && GameDataManager.Instance.dataStore.resPlanks >= recs[4])
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerLv7_NotEnoughBrick)
            {
                portalNextStepsArea.text = strings[14] + " " + strings[17];
                cachedPortalNextSteps = PortalNextSteps.TowerLv7_NotEnoughBrick;
            }
        }
        else if (GameDataManager.Instance.dataStore.resMetal >= recs[5] && GameDataManager.Instance.dataStore.resBricks >= recs[3])
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

    private void _in_UpdateProcessing_PortalArea_reqs(ref int[] reqs)
    {
        matsNeededClay.text = reqs[0].ToString();
        matsNeededWood.text = reqs[1].ToString();
        matsNeededOre.text = reqs[2].ToString();
        matsNeededBrick.text = reqs[3].ToString();
        matsNeededMetal.text = reqs[4].ToString();
        matsNeededPlanks.text = reqs[5].ToString();
    }
}