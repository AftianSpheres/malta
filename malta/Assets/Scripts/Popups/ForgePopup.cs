using UnityEngine;
using UnityEngine.UI;

enum ForgeStatus
{
    Uninitialized,
    NotReadyForOutbuilding,
    ReadyForOutbuilding,
    Outbuilding_Taskmaster
}

public class ForgePopup : MonoBehaviour
{
    public PopupMenu shell;
    public GameObject explainArea;
    public GameObject taskmasterArea;
    public GameObject taskmasterButton;
    public Button wpnBtn0;
    public Button wpnBtn1;
    public PopupMenu insufficientResourcesPopup;
    public Text[] taskmasterButtonReqsLabels;
    public Text explainAreaText;
    public Text explainAreaWarning;
    public Text wpnBtn0Text;
    public Text wpnBtn0TypeLabel;
    public Text[] wpnBtn0StdMatsReqs;
    public Text wpnBtn0ManaReq;
    public Text[] wpnBtn1StdMatsReqs;
    public Text wpnBtn1ManaReq;
    public Text wpnBtn1Text;
    public Text wpnBtn1TypeLabel;
    public TextAsset stringsResource;
    public TextAsset wpnsStringsResource;
    public TownBuilding townBuilding;
    private SovereignWpn cWpn0;
    private SovereignWpn cWpn1;
    private string[] strings;
    private string[] wpnsStrings;
    private float explainAreaExpiryTimer = -1;
    private const float explainAreaExpiryTime = 0.5f;
    private ForgeStatus status;
    const int lv0cost = 15;
    const int lv1cost = 30;
    const int lv2cost = 20;

    // Use this for initialization
    void Start ()
    {
        strings = stringsResource.text.Split('\n');
        wpnsStrings = wpnsStringsResource.text.Split('\n');
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
            _HandleWpnButton(ref GameDataManager.Instance.dataStore.buyable0, ref cWpn0, ref wpnBtn0, ref wpnBtn0Text, ref wpnBtn0TypeLabel, ref wpnBtn0ManaReq, ref wpnBtn0StdMatsReqs, ref wpnsStrings, ref strings);
            _HandleWpnButton(ref GameDataManager.Instance.dataStore.buyable1, ref cWpn1, ref wpnBtn1, ref wpnBtn1Text, ref wpnBtn1TypeLabel, ref wpnBtn1ManaReq, ref wpnBtn1StdMatsReqs, ref wpnsStrings, ref strings);
            if (GameDataManager.Instance.HasFlag(ProgressionFlags.TaskmasterUnlock))
            {
                status = ForgeStatus.Outbuilding_Taskmaster;
                RefreshReqsLabels();
            }
            else if (GameDataManager.Instance.HasFlag(ProgressionFlags.FirstTier2WpnBought))
            {
                status = ForgeStatus.ReadyForOutbuilding;
                RefreshReqsLabels();
            }
            else
            {
                status = ForgeStatus.NotReadyForOutbuilding;
                RefreshReqsLabels();
            }
            switch (status)
            {
                case ForgeStatus.ReadyForOutbuilding:
                    if (!taskmasterButton.activeInHierarchy) taskmasterButton.SetActive(true);
                    break;
                case ForgeStatus.Outbuilding_Taskmaster:
                    if (taskmasterButton.activeInHierarchy) taskmasterButton.SetActive(false);
                    if (!taskmasterArea.activeInHierarchy) taskmasterArea.SetActive(true);
                    break;
            }
        }
	}

    static void _HandleWpnButton (ref SovereignWpn buyable, ref SovereignWpn cWpn, ref Button wpnBtn, ref Text wpnBtnText, ref Text typeLabel, ref Text wpnBtnManaReq, ref Text[] wpnBtnStdMatsReqs, ref string[] wpnsStrings, ref string[] strings)
    {
        if (buyable == null || buyable.wpnType == WpnType.None)
        {
            wpnBtn.interactable = false;
            cWpn = null;
            wpnBtnText.text = strings[0];
            typeLabel.text = "";
        }
        else if (buyable != cWpn)
        {
            wpnBtn.interactable = true;
            SovereignInfoPanel.UpdateTextFieldWithWpnInfo(buyable, ref cWpn, ref wpnsStrings, ref wpnBtnText);
            typeLabel.text = SovereignWpn.GetWpnTypeString(cWpn.wpnType);
            if (cWpn.wpnLevel > 1)
            {
                wpnBtnManaReq.text = lv2cost.ToString();
                wpnBtnManaReq.transform.parent.gameObject.SetActive(true);
                for (int i = 0; i < wpnBtnStdMatsReqs.Length; i++) wpnBtnStdMatsReqs[i].transform.parent.gameObject.SetActive(false);
            }
            else
            {
                wpnBtnManaReq.transform.parent.gameObject.SetActive(false);
                for (int i = 0; i < wpnBtnStdMatsReqs.Length; i++)
                {
                    if (cWpn.wpnLevel == 0) wpnBtnStdMatsReqs[i].text = lv0cost.ToString();
                    else wpnBtnStdMatsReqs[i].text = lv1cost.ToString();
                    wpnBtnStdMatsReqs[i].transform.parent.gameObject.SetActive(true);
                }
            }
        }
    }

    private void RefreshReqsLabels ()
    {
        int[] costs = TownBuilding.GetUpgradeCost_Forge();
        for (int i = 0; i < costs.Length; i++)
        {
            taskmasterButtonReqsLabels[i].text = costs[i].ToString();
        }
    }
    
    public void BuyWpnButtonInteraction (bool wpn2)
    {
        SovereignWpn buyable;
        if (wpn2) buyable = GameDataManager.Instance.dataStore.buyable1;
        else buyable = GameDataManager.Instance.dataStore.buyable0;
        if (buyable.wpnLevel > 1)
        {
            if (GameDataManager.Instance.SpendManaIfPossible(lv2cost))
            {
                GameDataManager.Instance.GiveSovereignBuyableWpn(wpn2);
                shell.Close();
            }
            else
            {
                shell.SurrenderFocus();
                insufficientResourcesPopup.Open();
            }
        }
        else
        {
            int[] costs;
            if (buyable.wpnLevel == 0) costs = new int[] { lv0cost, lv0cost, lv0cost };
            else costs = new int[] { lv1cost, lv1cost, lv1cost };
            if (GameDataManager.Instance.SpendResourcesIfPossible(costs))
            {
                GameDataManager.Instance.GiveSovereignBuyableWpn(wpn2);
                shell.Close();
            }
            else
            {
                shell.SurrenderFocus();
                insufficientResourcesPopup.Open();
            }
        }
    }

    public void BuyWpnButtonMouseover (bool wpn2)
    {
        SovereignWpn buyable;
        if (wpn2) buyable = GameDataManager.Instance.dataStore.buyable1;
        else buyable = GameDataManager.Instance.dataStore.buyable0;
        if (buyable != null && buyable.wpnType != WpnType.None)
        {
            switch (buyable.wpnType)
            {
                case WpnType.Mace:
                    SovereignInfoPanel.UpdateTextFieldWithWpnInfo(GameDataManager.Instance.dataStore.sovWpn_Mace, ref GameDataManager.Instance.dataStore.sovWpn_Mace, ref wpnsStrings, ref explainAreaText);
                    explainAreaWarning.text = strings[1];
                    break;
                case WpnType.Knives:
                    SovereignInfoPanel.UpdateTextFieldWithWpnInfo(GameDataManager.Instance.dataStore.sovWpn_Knives, ref GameDataManager.Instance.dataStore.sovWpn_Knives, ref wpnsStrings, ref explainAreaText);
                    explainAreaWarning.text = strings[2];
                    break;
                case WpnType.Staff:
                    SovereignInfoPanel.UpdateTextFieldWithWpnInfo(GameDataManager.Instance.dataStore.sovWpn_Staff, ref GameDataManager.Instance.dataStore.sovWpn_Staff, ref wpnsStrings, ref explainAreaText);
                    explainAreaWarning.text = strings[3];
                    break;
            }
            explainArea.SetActive(true);
            explainAreaExpiryTimer = float.PositiveInfinity;
        }
    }

    public void BuildTaskmasterQuartersButtonInteraction ()
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

    public void MouseoverExit()
    {
        explainAreaExpiryTimer = explainAreaExpiryTime;
    }


}