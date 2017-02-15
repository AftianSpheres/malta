using UnityEngine;
using UnityEngine.UI;

enum PeddlerLocalState
{
    PeddlerPresent,
    PeddlerPaid,
    PeddlerDone
}

public class PeddlerPopup : MonoBehaviour
{
    public PopupMenu shell;
    public PopupMenu insufficientResourcesPopup;
    public Text btnText;
    public Text peddlerDialogue;
    public TextAsset stringsResource;
    public GameObject manaBtn;
    private PeddlerLocalState lState;
    private string[] strings;
    private int[] costs;
    private int finalOutput;

    // Use this for initialization
    void Start()
    {
        strings = Util.GetLinesFrom(stringsResource);
        int c = GameDataManager.Instance.dataStore.peddlerPrice * 2;
        costs = new int[] { c, c, c }; // we can precalculate this because the peddler never gets to change their price without leaving the town scene anyway
        finalOutput = GameDataManager.Instance.GetManaFromResourceCosts(GameDataManager.Instance.dataStore.peddlerPrice, GameDataManager.Instance.dataStore.peddlerPrice, GameDataManager.Instance.dataStore.peddlerPrice);
        btnText.text = strings[3] + c.ToString() + strings[4] + c.ToString() + strings[5] + c.ToString() + strings[6] + finalOutput.ToString() + strings[7]; // achievement unlocked: the worst line of code
	}

    void ChangeLocalState (PeddlerLocalState _lState)
    {
        lState = _lState;
        peddlerDialogue.text = strings[(int)lState];
        manaBtn.SetActive(false); // we never actually change local state TO PeddlerPresent but if you do that you'll actually have to check this instead of just going "nah, button's dead"
    }

    public void ManaButtonInteraction ()
    {
        if (GameDataManager.Instance.SpendResourcesIfPossible(costs))
        {
            GameDataManager.Instance.AwardMana(finalOutput);
            ChangeLocalState(PeddlerLocalState.PeddlerPaid);
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

    public void SpClose ()
    {
        if (lState == PeddlerLocalState.PeddlerPaid) ChangeLocalState(PeddlerLocalState.PeddlerDone);
        shell.Close();
    }
}
