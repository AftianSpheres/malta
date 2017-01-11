using UnityEngine;
using UnityEngine.UI;

public class SovereignInfoPanel : MonoBehaviour
{
    public PopupMenu shell;
    public PopupMenu sovereignSpecialPopup;
    public PopupMenu sovereignTacticsPopup;
    public PopupMenu sovereignWeaponPopup;
    public Image sovereignMugshot;
    public Text sovereignNameLabel;
    public Text sovereignSpecialDesc;
    public Text sovereignTitleArea;
    public Text sovereignTacticDesc;
    public Text sovereignWpnArea;
    public Text rowPlacementArea;
    public Button rowPlacementButton;
    public string[] strings;
    private string cachedSovereignName = "¡²¤€¼½¾‘’¥×äåé®®þüúíó«»áß";
    private string cachedSovereignTitle = "¡²¤€¼½¾‘’¥×äåé®®þüúíó«»áß";
    private BattlerAction cachedSovereignTactic = BattlerAction.None;
    private SovereignWpn cachedSovereignWpn = null;
    private bool cachedSovereignRowState;
    private AdventurerSpecial cachedSovereignSpecial = AdventurerSpecial.LoseBattle;
    private AdventurerMugshot cachedSovereignMugshot;
    private const string dividerString = " | ";

    private void Start()
    {
        cachedSovereignRowState = !GameDataManager.Instance.dataStore.sovereignOnBackRow;
    }

    // Update is called once per frame
    void Update ()
    {
        if (cachedSovereignName != GameDataManager.Instance.dataStore.sovereignName)
        {
            cachedSovereignName = GameDataManager.Instance.dataStore.sovereignName;
            sovereignNameLabel.text = strings[0] + GameDataManager.Instance.dataStore.sovereignName;
        }
        if (cachedSovereignMugshot != GameDataManager.Instance.dataStore.sovereignMugshot)
        {
            cachedSovereignMugshot = GameDataManager.Instance.dataStore.sovereignMugshot;
            sovereignMugshot.sprite = GameDataManager.Instance.dataStore.sovereignAdventurer.GetMugshotGraphic();
        }
        if (cachedSovereignTactic != GameDataManager.Instance.dataStore.sovereignTactic)
        {
            cachedSovereignTactic = GameDataManager.Instance.dataStore.sovereignTactic;
            sovereignTacticDesc.text = Adventurer.GetAttackDescription(GameDataManager.Instance.dataStore.sovereignTactic);
        }
        if (cachedSovereignSpecial != GameDataManager.Instance.dataStore.sovereignSkill)
        {
            cachedSovereignSpecial = GameDataManager.Instance.dataStore.sovereignSkill;
            sovereignSpecialDesc.text = Adventurer.GetSpecialDescription(GameDataManager.Instance.dataStore.sovereignSkill);
        }
        if (cachedSovereignTitle != GameDataManager.Instance.dataStore.sovereignAdventurer.fullTitle)
        {
            cachedSovereignTitle = GameDataManager.Instance.dataStore.sovereignAdventurer.fullTitle;
            sovereignTitleArea.text = cachedSovereignTitle;
        }
        if (cachedSovereignWpn != GameDataManager.Instance.dataStore.sovWpn_Set)
        {
            UpdateButtonWithWpnInfo(GameDataManager.Instance.dataStore.sovWpn_Set, ref cachedSovereignWpn, ref strings, ref sovereignWpnArea);
        }
        if (rowPlacementButton != null && rowPlacementButton.interactable != (GameDataManager.Instance.dataStore.sovereignEquippedWeaponType == WpnType.Knives)) rowPlacementButton.interactable = (GameDataManager.Instance.dataStore.sovereignEquippedWeaponType == WpnType.Knives);
        if (cachedSovereignRowState != GameDataManager.Instance.dataStore.sovereignOnBackRow)
        {
            cachedSovereignRowState = GameDataManager.Instance.dataStore.sovereignOnBackRow;
            if (cachedSovereignRowState)
            {
                rowPlacementArea.text = strings[35];
            }
            else rowPlacementArea.text = strings[34];
        }
    }

    public void OpenSovereignSpecialPopup()
    {
        shell.SurrenderFocus();
        sovereignSpecialPopup.Open();
    }

    public void OpenSovereignTacticsPopup()
    {
        shell.SurrenderFocus();
        sovereignTacticsPopup.Open();
    }

    public void OpenSovereignWeaponPopup()
    {
        shell.SurrenderFocus();
        sovereignWeaponPopup.Open();
    }

    public void SwitchRowPlacement ()
    {
        if (GameDataManager.Instance.dataStore.sovereignEquippedWeaponType == WpnType.Knives) GameDataManager.Instance.dataStore.sovereignOnBackRow = !GameDataManager.Instance.dataStore.sovereignOnBackRow;
    }

    public static void UpdateButtonWithWpnInfo (SovereignWpn o, ref SovereignWpn cachedSovereignWpn, ref string[] strings, ref Text sovereignWpnArea)
    {
        cachedSovereignWpn = o;
        string l0 = cachedSovereignWpn.wpnName + System.Environment.NewLine;
        string l1 = strings[2] + cachedSovereignWpn.HP + dividerString + strings[3] + cachedSovereignWpn.Martial + dividerString + strings[4] + cachedSovereignWpn.Magic + dividerString + strings[5] + cachedSovereignWpn.Speed + System.Environment.NewLine;
        string l2 = string.Empty;
        for (int i = 0; i < cachedSovereignWpn.attacks.Length; i++)
        {
            if (cachedSovereignWpn.attacks[i] != BattlerAction.UninitializedVal && cachedSovereignWpn.attacks[i] != BattlerAction.None)
            {
                l2 = l2 + BattlerActionData.get(cachedSovereignWpn.attacks[i]).name + System.Environment.NewLine;
            }
        }
        string l3 = string.Empty;
        switch (cachedSovereignWpn.wpnType)
        {
            case WpnType.Knives:
                l3 = strings[31];
                break;
            case WpnType.Mace:
                l3 = strings[32];
                break;
            case WpnType.Staff:
                l3 = strings[33];
                break;
        }
        sovereignWpnArea.text = l0 + l2 + l3;
    }
}
