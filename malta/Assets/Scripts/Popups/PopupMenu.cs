using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MovementEffects;

public class PopupMenu : MonoBehaviour
{
    public bool isActive
    {
        get { if (contents != null) return contents.activeInHierarchy; else return false; } // ugly hack, keeps loading popup from breaking shit
    }
    public GameObject contents;
    public Image dark;
    public PopupMenu[] focusSharers;
    public Button[] buttons;
    public Scrollbar[] scrollbars;
    private Button[] activeButtons;
    private Scrollbar[] activeScrollbars;
    public bool[] closeFocusSharersWhenClosingSelf;
    private bool surrenderedFocus;
	
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
            if (buttons[i] == null) continue;
            if (buttons[i].interactable)
            {
                activeButtons[i] = buttons[i];
                activeButtons[i].interactable = false;
            }
        }
        activeScrollbars = new Scrollbar[scrollbars.Length];
        for (int i = 0; i < scrollbars.Length; i++)
        {
            if (scrollbars[i] == null) continue;
            if (scrollbars[i].interactable)
            {
                activeScrollbars[i] = scrollbars[i];
                activeScrollbars[i].interactable = false;
            }
        }
        surrenderedFocus = true;
    }

    public void EraseSave ()
    {
        GameDataManager.Instance.EraseSave();
        Close();
    }

    public void EndGame ()
    {
        Timing.RunCoroutine(_in_EndGame());
    }

    public static string GetTimerReadout(float timer, float level)
    {
        string timerReadout;
        float trueSeconds = timer / level;
        int minutes = Mathf.FloorToInt(trueSeconds / 60);
        int seconds = Mathf.CeilToInt(trueSeconds - (60 * minutes));
        string secString;
        if (seconds > 9) secString = seconds.ToString();
        else secString = "0" + seconds.ToString();
        timerReadout = minutes.ToString() + ":" + secString;
        return timerReadout;
    }

    IEnumerator<float> _in_EndGame ()
    {
        GameStateManager.Instance.GiveFocusToCutscene(); // it's not a real cutscene, but close enough
        GameDataManager.Instance.Save();
        Close();
        yield return Timing.WaitUntilDone(Timing.RunCoroutine(FadeScreenToBlack(5)));
        GameStateManager.Instance.CutsceneHasCededFocus();
        LevelLoadManager.Instance.EnterLevel(SceneIDType.TitleScene);
    }

    IEnumerator<float> FadeScreenToBlack (float time)
    {
        dark.gameObject.SetActive(true);
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            dark.color = Color.Lerp(Color.clear, Color.black, t / time);
            yield return 0f;
        }
    }
}
