using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

[Serializable]
public class GameStateManager : Manager<GameStateManager>
{
    public int sessionFingerprint;
    public bool somethingHasFocus { get { return popupHasFocus || cutsceneHasFocus; } }
    private bool popupHasFocus;
    private bool cutsceneHasFocus;
    private Stack<PopupMenu> controllingPopupsStack;

    /// <summary>
    /// MonoBehavious.Start()
    /// </summary>
    void Start ()
    {
        sessionFingerprint = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        controllingPopupsStack = new Stack<PopupMenu>();
	}
	
    /// <summary>
    /// MonoBehaviour.Update()
    /// </summary>
	void Update ()
    {
        if (!cutsceneHasFocus)
        {
            if (popupHasFocus && !controllingPopupsStack.Peek().isActive)
            {
                controllingPopupsStack.Pop();
                if (controllingPopupsStack.Count == 0) popupHasFocus = false;
            }
        }

    }

    /// <summary>
    /// Generates a new session fingerprint value.
    /// </summary>
    public void RerollSessionFingerprint ()
    {
        int r = sessionFingerprint;
        while (r == sessionFingerprint)
        {
            r = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
        sessionFingerprint = r;
    }

    public void GiveFocusToCutscene ()
    {
        cutsceneHasFocus = true;
    }

    public void CutsceneHasCededFocus ()
    {
        cutsceneHasFocus = false;
    }

    public void GiveFocusToPopup (PopupMenu popup)
    {
        popupHasFocus = true;
        controllingPopupsStack.Push(popup);
    }

    public bool PopupHasFocus (PopupMenu popup)
    {
        bool result = false;
        PopupMenu topPopup = controllingPopupsStack.Peek();
        if (!cutsceneHasFocus)
        {
            if (popup == topPopup) result = true;
            else if (topPopup.focusSharers != null)
            {
                for (int i = 0; i < topPopup.focusSharers.Length; i++)
                {
                    if (topPopup.focusSharers[i] == popup) result = true;
                }
            }
        }
        return result;
    }

}
