using UnityEngine;
using System.Collections;

/// <summary>
/// IF I HAD POINTERS THIS WOULDN'T EVEN EXIST
/// </summary>
public class TopLevelMenuSystem : MonoBehaviour
{
    public PopupMenu clayPitPopup;
    public PopupMenu docksPopup;
    public PopupMenu forgePopup;
    public PopupMenu masonPopup;
    public PopupMenu minesPopup;
    public PopupMenu loadingPopup;
    public PopupMenu palacePopup;
    public PopupMenu sawmillPopup;
    public PopupMenu smithPopup;
    public PopupMenu woodlandsPopup;

	// Use this for initialization
	void Update ()
    {
        if (LevelLoadManager.Instance != null && LevelLoadManager.Instance.topLevelMenuSystem != this) LevelLoadManager.Instance.topLevelMenuSystem = this; // that's it, we just use this thing to find child objects of the prefab
	}
	
}
