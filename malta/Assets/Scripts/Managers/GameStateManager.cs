using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

[Serializable]
public class GameStateManager : Manager<GameStateManager>
{
    public int sessionFingerprint;
    public bool popupHasFocus { get; private set; }
    private Stack<PopupMenu> controllingPopupsStack;

    /// <summary>
    /// MonoBehavious.Start()
    /// </summary>
    void Start ()
    {
        sessionFingerprint = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
	}
	
    /// <summary>
    /// MonoBehaviour.Update()
    /// </summary>
	void Update ()
    {
        if (popupHasFocus && !controllingPopupsStack.Peek().isActive)
        {
            if (controllingPopupsStack.Count > 1)
            {
                controllingPopupsStack.Pop().Close();
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

    public void GiveFocusToPopup (PopupMenu popup)
    {
        popupHasFocus = true;
        controllingPopupsStack.Push(popup);
    }

    public bool PopupHasFocus (PopupMenu popup)
    {
        bool result = false;
        PopupMenu topPopup = controllingPopupsStack.Peek();
        if (popup == topPopup) result = true;
        else if (topPopup.focusSharers != null)
        {
            for (int i = 0; i < topPopup.focusSharers.Length; i++)
            {
                if (topPopup.focusSharers[i] == popup) result = true;
            }
        }
        return result;
    }

}
