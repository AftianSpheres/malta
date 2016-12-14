using UnityEngine;
using UnityEngine.UI;

enum ForgeStatus
{
    Uninitialized,
    NoUpgrades,
    AtLeastOneUpgrade,
    ReadyForOutbuilding,
    Outbuilding_Adventurer,
    Outbuilding_Taskmaster
}

public class ForgePopup : MonoBehaviour
{
    public PopupMenu shell;
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
    public Text[] adventurerButtonReqsLabels;
    public Text[] bowmanButtonReqsLabels;
    public Text[] footmanButtonReqsLabels;
    public Text[] sageButtonReqsLabels;
    public Text[] taskmasterButtonReqsLabels;
    public Text[] wizardButtonReqsLabels;
    public Text explainAreaText;
    public Text mysticUpgradeText;
    public Text warriorUpgradeText;
    public TextAsset stringsResource;
    public TownBuilding townBuilding;
    private string[] strings;
    private float explainAreaExpiryTimer = -1;
    private const float explainAreaExpiryTime = 0.5f;
    private ForgeStatus status;

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
            if (explainAreaExpiryTimer > 0)
            {
                explainAreaExpiryTimer -= Time.deltaTime;
            }
            if (explainAreaExpiryTimer <= 0 && explainArea.activeInHierarchy)
            {
                explainArea.SetActive(false);
            }
            switch (status) // this is gross. probably rip it out & do something cleaner later on. (probably the one thing that'd ever be made more elegant by switch fallthrough, lol)
            {
                case ForgeStatus.Uninitialized:
                    _in_FuckingAwfulSwitchStatement(3);
                    break;
                case ForgeStatus.NoUpgrades:
                    _in_FuckingAwfulSwitchStatement(2);
                    break;
                case ForgeStatus.AtLeastOneUpgrade:
                    ConformClassUpgradeButtonsToGameState();
                    _in_FuckingAwfulSwitchStatement(1);
                    break;
                case ForgeStatus.ReadyForOutbuilding:
                    ConformClassUpgradeButtonsToGameState();
                    if (!adventurerButton.activeInHierarchy) adventurerButton.SetActive(true);
                    if (!taskmasterButton.activeInHierarchy) taskmasterButton.SetActive(true);
                    _in_FuckingAwfulSwitchStatement(0);
                    break;
                case ForgeStatus.Outbuilding_Adventurer:
                    ConformClassUpgradeButtonsToGameState();
                    if (adventurerButton.activeInHierarchy) adventurerButton.SetActive(false);
                    if (taskmasterButton.activeInHierarchy) taskmasterButton.SetActive(false);
                    if (!adventurerArea.activeInHierarchy) adventurerArea.SetActive(true);
                    break;
                case ForgeStatus.Outbuilding_Taskmaster:
                    ConformClassUpgradeButtonsToGameState();
                    if (adventurerButton.activeInHierarchy) adventurerButton.SetActive(false);
                    if (taskmasterButton.activeInHierarchy) taskmasterButton.SetActive(false);
                    if (!taskmasterArea.activeInHierarchy) taskmasterArea.SetActive(true);
                    break;
            }
        }
	}

    /// <summary>
    /// This is still gross and needs to be ripped out entirely but at least this makes it maintainable.
    /// </summary>
    /// <param name="steps">How far down do we want to follow this piece of shit?</param>
    private void _in_FuckingAwfulSwitchStatement(int steps)
    {
        if (GameDataManager.Instance.unlock_forgeOutbuilding)
        {
            if (GameDataManager.Instance.unlock_Taskmaster) status = ForgeStatus.Outbuilding_Taskmaster;
            else status = ForgeStatus.Outbuilding_Adventurer;
            RefreshReqsLabels();
        }
        else if (GameDataManager.Instance.warriorClassUnlock != AdventurerClass.Warrior && GameDataManager.Instance.mysticClassUnlock != AdventurerClass.Mystic && steps > 0)
        {
            status = ForgeStatus.ReadyForOutbuilding;
            RefreshReqsLabels();
        }
        else if (GameDataManager.Instance.warriorClassUnlock != AdventurerClass.Warrior ^ GameDataManager.Instance.mysticClassUnlock != AdventurerClass.Mystic && steps > 1)
        {
            status = ForgeStatus.AtLeastOneUpgrade;
            RefreshReqsLabels();
        }
        else if (steps > 2)
        {
            status = ForgeStatus.NoUpgrades;
            RefreshReqsLabels();
        }
    }

    private void ConformClassUpgradeButtonsToGameState ()
    {
        if (GameDataManager.Instance.warriorClassUnlock != AdventurerClass.Warrior && (bowmanButton.activeInHierarchy || footmanButton.activeInHierarchy))
        {
            bowmanButton.SetActive(false);
            footmanButton.SetActive(false);
            warriorUpgradeText.gameObject.SetActive(true);
            if (GameDataManager.Instance.warriorClassUnlock == AdventurerClass.Bowman) warriorUpgradeText.text = strings[4];
            else warriorUpgradeText.text = strings[5];
        }
        if (GameDataManager.Instance.mysticClassUnlock != AdventurerClass.Mystic && (sageButton.activeInHierarchy || wizardButton.activeInHierarchy))
        {
            sageButton.SetActive(false);
            wizardButton.SetActive(false);
            mysticUpgradeText.gameObject.SetActive(true);
            if (GameDataManager.Instance.mysticClassUnlock == AdventurerClass.Sage) mysticUpgradeText.text = strings[6];
            else mysticUpgradeText.text = strings[7];
        }
    }

    private void RefreshReqsLabels ()
    {
        int[] costs = TownBuilding.GetUpgradeCost_Forge();
        for (int i = 0; i < costs.Length; i++)
        {
            adventurerButtonReqsLabels[i].text = bowmanButtonReqsLabels[i].text = footmanButtonReqsLabels[i].text = 
                sageButtonReqsLabels[i].text = taskmasterButtonReqsLabels[i].text = wizardButtonReqsLabels[i].text = costs[i].ToString();
        }
    }

    public void BuildAdventurerQuartersButtonInteraction ()
    {
        int[] costs = TownBuilding.GetUpgradeCost_Forge();
        if (GameDataManager.Instance.SpendResourcesIfPossible(costs))
        {
            townBuilding.BuildOutbuilding();
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
            townBuilding.BuildOutbuilding(true);
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    public void MouseoverExit()
    {
        explainAreaExpiryTimer = explainAreaExpiryTime;
    }

    public void MysticToSageButtonMouseoverEntry ()
    {
        if (GameStateManager.Instance.PopupHasFocus(shell))
        {
            if (!explainArea.activeInHierarchy) explainArea.SetActive(true);
            if (explainAreaText.text != strings[2]) explainAreaText.text = strings[2];
            explainAreaExpiryTimer = -1.0f;
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

    public void MysticToWizardButtonMouseoverEntry ()
    {
        if (GameStateManager.Instance.PopupHasFocus(shell))
        {
            if (!explainArea.activeInHierarchy) explainArea.SetActive(true);
            if (explainAreaText.text != strings[3]) explainAreaText.text = strings[3];
            explainAreaExpiryTimer = -1.0f;
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

    public void WarriorToBowmanButtonMouseoverEntry ()
    {
        if (GameStateManager.Instance.PopupHasFocus(shell))
        {
            if (!explainArea.activeInHierarchy) explainArea.SetActive(true);
            if (explainAreaText.text != strings[0]) explainAreaText.text = strings[0];
            explainAreaExpiryTimer = -1.0f;
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

    public void WarriorToFootmanButtonMouseoverEntry ()
    {
        if (GameStateManager.Instance.PopupHasFocus(shell))
        {
            if (!explainArea.activeInHierarchy) explainArea.SetActive(true);
            if (explainAreaText.text != strings[1]) explainAreaText.text = strings[1];
            explainAreaExpiryTimer = -1.0f;
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