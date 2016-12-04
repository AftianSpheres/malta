using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup for fake towns. 
/// </summary>
public class FakeTownPopup : MonoBehaviour
{
    public PopupMenu shell;
    public Text bricksText;
    public Text fakeTownName;
    public Text metalText;
    public Text planksText;
    private int cachedLv_ClayPit = -1;
    private int cachedLv_Mines = -1;
    private int cachedLv_Woodlands = -1;
    private int currentFakeTownIndex = -1;

    /// <summary>
    /// Updates fake town popup. Since this is a real popup it requires real updates, despite the fakeness of the town.
    /// </summary>
    void Update()
    {
        if (GameDataManager.Instance != null)
        {
            if (cachedLv_ClayPit != GameDataManager.Instance.harvestLv_ClayPit || cachedLv_Mines != GameDataManager.Instance.harvestLv_Mine || cachedLv_Woodlands != GameDataManager.Instance.harvestLv_Woodlands)
            {
                RefreshFakeNumbersFakelyForFakeTown();
            }
        }
    }

    public void Open (int index)
    {
        shell.Open();
        currentFakeTownIndex = index;
        fakeTownName.text = GameDataManager.Instance.fakeTownNames[currentFakeTownIndex];
        RefreshFakeNumbersFakelyForFakeTown();
    }

    /// <summary>
    /// Figures resource gain/contribution rates, turns them into strings, and immediately throws them away because fake towns.
    /// </summary>
    private void RefreshFakeNumbersFakelyForFakeTown ()
    {
        cachedLv_ClayPit = GameDataManager.Instance.harvestLv_ClayPit;
        cachedLv_Mines = GameDataManager.Instance.harvestLv_Mine;
        cachedLv_Woodlands = GameDataManager.Instance.harvestLv_Woodlands;
        int fakeNo;
        fakeNo = GameDataManager.Instance.GetResourceGainRate(cachedLv_ClayPit - 1, 4);
        bricksText.text = fakeNo.ToString() + " / " + (fakeNo / 4).ToString();
        fakeNo = GameDataManager.Instance.GetResourceGainRate(cachedLv_Mines - 1, 4);
        metalText.text = fakeNo.ToString() + " / " + (fakeNo / 4).ToString();
        fakeNo = GameDataManager.Instance.GetResourceGainRate(cachedLv_Woodlands - 1, 4);
        planksText.text = fakeNo.ToString() + " / " + (fakeNo / 4).ToString();
    }
}
