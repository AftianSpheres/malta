using UnityEngine;
using UnityEngine.UI;

enum PortalNextSteps
{
    None,
    NoTower,
    TowerPreLv7_NotEnoughMana,
    TowerPreLv7_ReadyForUpgrade,
    r0,
    r1,
    r2,
    TowerLv7_NotEnoughMana,
    TowerLv7_ReadyForUpgrade,
    r3,
    r4,
    r5,
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
    public Text manaNeeded;
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
        int recs = TownBuilding.GetUpgradeCost_WizardsTower(GameDataManager.Instance.dataStore.buildingLv_WizardsTower);
        _in_UpdateProcessing_PortalArea_reqs(ref recs);
        if (GameDataManager.Instance.CheckManaAvailability(recs))
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerPreLv7_ReadyForUpgrade)
            {
                portalNextStepsArea.text = strings[13] + " " + strings[15];
                cachedPortalNextSteps = PortalNextSteps.TowerPreLv7_ReadyForUpgrade;
            }
        }
        else if (cachedPortalNextSteps != PortalNextSteps.TowerPreLv7_NotEnoughMana)
        {
            portalNextStepsArea.text = strings[13] + " " + strings[16];
            cachedPortalNextSteps = PortalNextSteps.TowerPreLv7_NotEnoughMana;
        }
    }

    private void _in_UpdateProcessing_PortalArea_postLv7()
    {
        int recs = TownBuilding.GetUpgradeCost_WizardsTower(GameDataManager.Instance.dataStore.buildingLv_WizardsTower);
        _in_UpdateProcessing_PortalArea_reqs(ref recs);
        if (GameDataManager.Instance.CheckManaAvailability(recs))
        {
            if (cachedPortalNextSteps != PortalNextSteps.TowerLv7_ReadyForUpgrade)
            {
                portalNextStepsArea.text = strings[14] + " " + strings[15];
                cachedPortalNextSteps = PortalNextSteps.TowerLv7_ReadyForUpgrade;
            }
        }
        else if (cachedPortalNextSteps != PortalNextSteps.TowerLv7_NotEnoughMana)
        {
            portalNextStepsArea.text = strings[14] + " " + strings[16];
            cachedPortalNextSteps = PortalNextSteps.TowerLv7_NotEnoughMana;
        }
    }

    private void _in_UpdateProcessing_PortalArea_reqs(ref int m)
    {
        manaNeeded.text = m.ToString();
    }
}