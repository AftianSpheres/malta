using UnityEngine;
using System.Collections;

public class AutoMessagePopper : MonoBehaviour
{
    public MessagePopup msgPopup;
    public ProgressionFlags[] requiredFlags;
    public ProgressionFlags[] activatedFlags;
    public TextAsset[] messages;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (GameStateManager.Instance != null)
        {
            if (!GameStateManager.Instance.somethingHasFocus)
            {
                for (int i = 0; i < messages.Length; i++)
                {
                    if (requiredFlags[i] == ProgressionFlags.None || GameDataManager.Instance.HasFlag(requiredFlags[i]))
                    {
                        if (!GameDataManager.Instance.HasFlag(activatedFlags[i]))
                        {
                            GameDataManager.Instance.SetFlag(activatedFlags[i]);
                            msgPopup.PopOpenOn(messages[i]);
                            break;
                        }
                    }
                }
            }
        }
	}
}
