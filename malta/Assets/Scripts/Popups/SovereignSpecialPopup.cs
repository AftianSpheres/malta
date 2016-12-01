using UnityEngine;
using UnityEngine.UI;

public class SovereignSpecialPopup : MonoBehaviour
{
    public PopupMenu shell;
    public GameObject calledShotsCosts;
    public GameObject calledShotsNewLabel;
    public GameObject hammerSmashCosts;
    public GameObject hammerSmashNewLabel;
    public GameObject protectCosts;
    public GameObject protectNewLabel;
    public Text calledShotsText;
    public Text hammerSmashText;
    public Text protectText;
    public PopupMenu insufficientResourcesPopup;

    void Start()
    {
        calledShotsText.text = Adventurer.GetSpecialDescription(AdventurerSpecial.CalledShots);
        hammerSmashText.text = Adventurer.GetSpecialDescription(AdventurerSpecial.HammerSmash);
        protectText.text = Adventurer.GetSpecialDescription(AdventurerSpecial.Protect);
    }

    // Update is called once per frame
    void Update ()
    {
	    if (GameDataManager.Instance != null)
        {
            if (GameDataManager.Instance.unlock_sovSpe_CalledShots)
            {
                if (calledShotsCosts.activeInHierarchy) calledShotsCosts.SetActive(false);
                if (calledShotsNewLabel.activeInHierarchy) calledShotsNewLabel.SetActive(false);
            }
            else
            {
                if (!calledShotsCosts.activeInHierarchy) calledShotsCosts.SetActive(true);
                if (!calledShotsNewLabel.activeInHierarchy) calledShotsNewLabel.SetActive(true);
            }
            if (GameDataManager.Instance.unlock_sovSpe_HammerSmash)
            {
                if (hammerSmashCosts.activeInHierarchy) hammerSmashCosts.SetActive(false);
                if (hammerSmashNewLabel.activeInHierarchy) hammerSmashNewLabel.SetActive(false);
            }
            else
            {
                if (!hammerSmashCosts.activeInHierarchy) hammerSmashCosts.SetActive(true);
                if (!hammerSmashNewLabel.activeInHierarchy) hammerSmashNewLabel.SetActive(true);
            }
            if (GameDataManager.Instance.unlock_sovSpe_Protect)
            {
                if (protectCosts.activeInHierarchy) protectCosts.SetActive(false);
                if (protectNewLabel.activeInHierarchy) protectNewLabel.SetActive(false);
            }
            else
            {
                if (!protectCosts.activeInHierarchy) protectCosts.SetActive(true);
                if (!protectNewLabel.activeInHierarchy) protectNewLabel.SetActive(true);
            }
        }
	}

    public void CalledShotsButtonInteraction ()
    {
        if (GameDataManager.Instance.unlock_sovSpe_CalledShots)
        {
            GameDataManager.Instance.SetSovereignSpecial(AdventurerSpecial.CalledShots);
            shell.Close();
        }
        else if (GameDataManager.Instance.SpendResourcesIfPossible(5, 5, 5, 5, 5, 5))
        {
            GameDataManager.Instance.unlock_sovSpe_CalledShots = true;
            GameDataManager.Instance.SetSovereignSpecial(AdventurerSpecial.CalledShots);
            shell.Close();
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    public void HammerSmashButtonInteraction()
    {
        if (GameDataManager.Instance.unlock_sovSpe_HammerSmash)
        {
            GameDataManager.Instance.SetSovereignSpecial(AdventurerSpecial.HammerSmash);
            shell.Close();
        }
        else if (GameDataManager.Instance.SpendResourcesIfPossible(5, 5, 5, 5, 5, 5))
        {
            GameDataManager.Instance.unlock_sovSpe_HammerSmash = true;
            GameDataManager.Instance.SetSovereignSpecial(AdventurerSpecial.HammerSmash);
            shell.Close();
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    public void ProtectButtonInteraction()
    {
        if (GameDataManager.Instance.unlock_sovSpe_Protect)
        {
            GameDataManager.Instance.SetSovereignSpecial(AdventurerSpecial.Protect);
            shell.Close();
        }
        else if (GameDataManager.Instance.SpendResourcesIfPossible(5, 5, 5, 5, 5, 5))
        {
            GameDataManager.Instance.unlock_sovSpe_Protect = true;
            GameDataManager.Instance.SetSovereignSpecial(AdventurerSpecial.Protect);
            shell.Close();
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }
}
