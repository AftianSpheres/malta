using UnityEngine;
using UnityEngine.UI;
using System;

[Flags]
public enum MessageFlags
{
    None = 0,
    LaunchAdventureOnClose = 1 << 0
}

public struct Message
{
    public string title;
    public string body;
    public MessageFlags flags;

    public Message (TextAsset msg, MessageFlags[] flagsArray)
    {
        string[] strings = Util.GetLinesFrom(msg, 2);
        title = strings[0];
        body = strings[1];
        flags = MessageFlags.None;
        for (int i = 0; i < flagsArray.Length; i++) flags |= flagsArray[i];
    }
}

public class MessagePopup : MonoBehaviour
{
    public PopupMenu shell;
    public Text title;
    public Text body;
    public MessageFlags flags;
    public ScreenChanger advScreenChanger;
	
    public void PopOpenOn (Message msg)
    {
        title.text = msg.title;
        body.text = msg.body;
        flags = msg.flags;
        shell.Open();
    }

    public void Close ()
    {
        if (HasFlag(MessageFlags.LaunchAdventureOnClose)) advScreenChanger.Activate();
        shell.Close();
    }

    bool HasFlag(MessageFlags f)
    {
        return (flags & f) == f;
    }
}
