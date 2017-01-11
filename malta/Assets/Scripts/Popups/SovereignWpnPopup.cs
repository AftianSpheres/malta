using UnityEngine;
using UnityEngine.UI;
using System;

public class SovereignWpnPopup : MonoBehaviour
{
    public PopupMenu shell;
    public Text maceArea;
    public Text staffArea;
    public Text knivesArea;
    public TextAsset stringsResource;
    private SovereignWpn cachedMace;
    private SovereignWpn cachedStaff;
    private SovereignWpn cachedKnives;
    private string[] strings;


	// Use this for initialization
	void Start ()
    {
	    strings = stringsResource.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (cachedMace != GameDataManager.Instance.dataStore.sovWpn_Mace) SovereignInfoPanel.UpdateButtonWithWpnInfo(GameDataManager.Instance.dataStore.sovWpn_Mace, ref cachedMace, ref strings, ref maceArea);
        if (cachedStaff != GameDataManager.Instance.dataStore.sovWpn_Staff) SovereignInfoPanel.UpdateButtonWithWpnInfo(GameDataManager.Instance.dataStore.sovWpn_Staff, ref cachedStaff, ref strings, ref staffArea);
        if (cachedKnives != GameDataManager.Instance.dataStore.sovWpn_Knives) SovereignInfoPanel.UpdateButtonWithWpnInfo(GameDataManager.Instance.dataStore.sovWpn_Knives, ref cachedKnives, ref strings, ref knivesArea);
	}

    public void SetMace ()
    {
        GameDataManager.Instance.ChangeSetSovWpn(WpnType.Mace);
        shell.Close();
    }

    public void SetStaff ()
    {
        GameDataManager.Instance.ChangeSetSovWpn(WpnType.Staff);
        shell.Close();
    }

    public void SetKnives ()
    {
        GameDataManager.Instance.ChangeSetSovWpn(WpnType.Knives);
        shell.Close();
    }
}
