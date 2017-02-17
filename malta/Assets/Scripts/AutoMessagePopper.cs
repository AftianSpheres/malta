using UnityEngine;
using System;

public class AutoMessagePopper : MonoBehaviour
{
    public MessagePopup msgPopup;
    public ProgressionFlags[] requiredFlags;
    public ProgressionFlags[] activatedFlags;
    public TextAsset[] textAssets;
    public TextAsset[] flagSets;
    [NonSerialized] public Message[] messages;

	// Use this for initialization
	void Start ()
    {
        messages = new Message[textAssets.Length];
	    for (int i = 0; i < textAssets.Length; i++)
        {
            MessageFlags[] flags;
            if (i < flagSets.Length && flagSets[i] != null)
            {
                string[] flagNames = Util.GetLinesFrom(flagSets[i]);
                flags = new MessageFlags[flagNames.Length];
                for (int i2 = 0; i2 < flags.Length; i2++) flags[i2] = strToMessageFlag(flagNames[i2]);
            }
            else flags = new MessageFlags[0];
            messages[i] = new Message(textAssets[i], flags);
        }    
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

    MessageFlags strToMessageFlag (string s)
    {
        switch (s)
        {
            case ("LaunchAdventureOnClose"):
                return MessageFlags.LaunchAdventureOnClose;
            default:
                throw new Exception("Yo, fuckface, " + s + " ain't a valid MessageFlags! Or it's not parsable yet! Whichever.");
        }
    }
}
