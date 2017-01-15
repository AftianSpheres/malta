using UnityEngine;
using UnityEngine.UI;

public class MessagePopup : MonoBehaviour
{
    public PopupMenu shell;
    public Text title;
    public Text body;
	
    public void PopOpenOn (TextAsset msg)
    {
        string[] strings = msg.text.Split(new string[] { "\r\n", "\n" }, 2, System.StringSplitOptions.None);
        title.text = strings[0];
        body.text = strings[1];
        shell.Open();
    }
}
