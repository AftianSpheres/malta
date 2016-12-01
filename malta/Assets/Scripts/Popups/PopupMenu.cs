using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupMenu : MonoBehaviour
{
    public bool isActive
    {
        get { return contents.activeInHierarchy; }
    }
    public GameObject contents;
    public PopupMenu[] focusSharers;
    public Button[] buttons;
    public Scrollbar[] scrollbars;
    private Button[] activeButtons;
    private Scrollbar[] activeScrollbars;
    public bool[] closeFocusSharersWhenClosingSelf;
    private bool surrenderedFocus;

    // Use this for initialization
    void Start ()
    {

	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (GameStateManager.Instance != null)
        {
            if (surrenderedFocus && GameStateManager.Instance.PopupHasFocus(this))
            {
                for (int i = 0; i < activeButtons.Length; i++)
                {
                    if (activeButtons[i] != null) activeButtons[i].interactable = true;
                }
                for (int i = 0; i < activeScrollbars.Length; i++)
                {
                    if (activeScrollbars[i] != null) activeScrollbars[i].interactable = true;
                }
                surrenderedFocus = false;
            }
        }
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

    public void SurrenderFocus ()
    {
        activeButtons = new Button[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].interactable)
            {
                activeButtons[i] = buttons[i];
                activeButtons[i].interactable = false;
            }
        }
        activeScrollbars = new Scrollbar[scrollbars.Length];
        for (int i = 0; i < scrollbars.Length; i++)
        {
            if (scrollbars[i].interactable)
            {
                activeScrollbars[i] = scrollbars[i];
                activeScrollbars[i].interactable = false;
            }
        }
        surrenderedFocus = true;
    }
}
