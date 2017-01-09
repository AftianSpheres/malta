using UnityEngine;
using UnityEngine.UI;

public class SovereignTacticPopup : MonoBehaviour
{
    public PopupMenu shell;
    public Text flankingText;
    public Text getBehindMeText;

	// Use this for initialization
	void Start ()
    {
        flankingText.text = Adventurer.GetAttackDescription(BattlerAction.Flanking);
        getBehindMeText.text = Adventurer.GetAttackDescription(BattlerAction.GetBehindMe);
	}
	
    public void FlankingButtonInteraction ()
    {
        GameDataManager.Instance.SetSovereignTactic(BattlerAction.Flanking);
        shell.Close();
    }

    public void GetBehindMeButtonInteraction ()
    {
        GameDataManager.Instance.SetSovereignTactic(BattlerAction.GetBehindMe);
        shell.Close();
    }
}
