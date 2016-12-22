using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is without a doubt the prettiest piece of code in this entire project, for obvious reasons.
/// </summary>
public class UI_Eyeshade : MonoBehaviour
{
    public Image image;
	
	// Update is called once per frame
	void Update ()
    {
	    if (GameStateManager.Instance != null)
        {
            if (GameStateManager.Instance.somethingHasFocus != image.enabled) image.enabled = GameStateManager.Instance.somethingHasFocus;
        }
	}
}
