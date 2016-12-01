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
        flankingText.text = Adventurer.GetAttackDescription(AdventurerAttack.Flanking);
        getBehindMeText.text = Adventurer.GetAttackDescription(AdventurerAttack.GetBehindMe);
	}
	
    public void FlankingButtonInteraction ()
    {
        GameDataManager.Instance.SetSovereignTactic(AdventurerAttack.Flanking);
        shell.Close();
    }

    public void GetBehindMeButtonInteraction ()
    {
        GameDataManager.Instance.SetSovereignTactic(AdventurerAttack.GetBehindMe);
        shell.Close();
    }
}
