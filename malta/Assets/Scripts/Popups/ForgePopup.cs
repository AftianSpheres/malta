using UnityEngine;
using UnityEngine.UI;

public class ForgePopup : MonoBehaviour
{
    public PopupMenu shell;
    public BoxCollider2D bowmanButtonCollider;
    public BoxCollider2D footmanButtonCollider;
    public BoxCollider2D sageButtonCollider;
    public BoxCollider2D wizardButtonCollider;
    public GameObject adventurerArea;
    public GameObject adventurerButton;
    public GameObject bowmanButton;
    public GameObject explainArea;
    public GameObject footmanButton;
    public GameObject sageButton;
    public GameObject taskmasterArea;
    public GameObject taskmasterButton;
    public GameObject wizardButton;
    public PopupMenu insufficientResourcesPopup;
    public Text[] bowmanButtonReqsLabels;
    public Text[] footmanButtonReqsLabels;
    public Text[] sageButtonReqsLabels;
    public Text[] wizardButtonReqsLabels;
    public Text mysticUpgradeText;
    public Text warriorUpgradeText;
    public TextAsset stringsResource;
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

        }
	}

    public void BuildAdventurerQuartersButtonInteraction ()
    {
        int[] costs = TownBuilding.GetUpgradeCost_Forge();
        if (GameDataManager.Instance.SpendResourcesIfPossible(costs))
        {
            GameDataManager.Instance.unlock_forgeOutbuilding = true;
            GameDataManager.Instance.unlock_Taskmaster = false;
            if (GameDataManager.Instance.forgeAdventurer == null)
            {
                GameDataManager.Instance.forgeAdventurer = ScriptableObject.CreateInstance<Adventurer>();
                GameDataManager.Instance.forgeAdventurer.Reroll(GameDataManager.Instance.warriorClassUnlock, AdventurerSpecies.Human, false, new int[] { 0, 0, 0, 0 });
            }
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }
    
    public void BuildTaskmasterQuartersButtonInteraction ()
    {
        int[] costs = TownBuilding.GetUpgradeCost_Forge();
        if (GameDataManager.Instance.SpendResourcesIfPossible(costs))
        {
            GameDataManager.Instance.unlock_forgeOutbuilding = true;
            GameDataManager.Instance.unlock_Taskmaster = true;
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    public void MysticToSageButtonInteraction ()
    {
        int[] costs = TownBuilding.GetUpgradeCost_Forge();
        if (GameDataManager.Instance.SpendResourcesIfPossible(costs)) GameDataManager.Instance.PromoteMysticsTo(AdventurerClass.Sage);
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    public void MysticToWizardButtonInteraction ()
    {
        int[] costs = TownBuilding.GetUpgradeCost_Forge();
        if (GameDataManager.Instance.SpendResourcesIfPossible(costs)) GameDataManager.Instance.PromoteMysticsTo(AdventurerClass.Wizard);
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    public void WarriorToBowmanButtonInteraction ()
    {
        int[] costs = TownBuilding.GetUpgradeCost_Forge();
        if (GameDataManager.Instance.SpendResourcesIfPossible(costs)) GameDataManager.Instance.PromoteWarriorsTo(AdventurerClass.Bowman);
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    public void WarriorToFootmanButtonInteraction ()
    {
        int[] costs = TownBuilding.GetUpgradeCost_Forge();
        if (GameDataManager.Instance.SpendResourcesIfPossible(costs)) GameDataManager.Instance.PromoteWarriorsTo(AdventurerClass.Footman);
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }
}
