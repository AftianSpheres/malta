using UnityEngine;
using UnityEngine.UI;

public class ButtonLocker : MonoBehaviour
{
    public Button btn;
    public ProgressionFlags requiredFlag;
	
	// Update is called once per frame
	void Update ()
    {
	    if (!GameDataManager.Instance.HasFlag(requiredFlag))
        {
            if (btn.interactable) btn.interactable = false;
        }
        else
        {
            btn.interactable = true;
            Destroy(this); // kill the locker so that other scripts can regain control of the button's interactable state
        }
	}
}
