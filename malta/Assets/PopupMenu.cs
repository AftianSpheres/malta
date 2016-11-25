using UnityEngine;
using System.Collections;

public class PopupMenu : MonoBehaviour
{
    public bool isActive
    {
        get { return contents.activeInHierarchy; }
    }
    public GameObject contents;
    public PopupMenu[] focusSharers;
    public bool[] closeFocusSharersWhenClosingSelf;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Open ()
    {
        contents.SetActive(true);
        GameStateManager.Instance.GiveFocusToPopup(this);
    }

    public void Close ()
    {
        contents.SetActive(false);
        for (int i = 0; i < focusSharers.Length && i < closeFocusSharersWhenClosingSelf.Length; i++)
        {
            if (closeFocusSharersWhenClosingSelf[i]) focusSharers[i].Close();
        }
    }
}
