using UnityEngine;
using UnityEngine.UI;

public class BattleTheater : MonoBehaviour
{
    public BattleOverseer overseer;
    public BattleMessageBox messageBox;
    public bool processing { get; private set; }

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (messageBox.messageQueue.Count > 0) processing = true;
        else processing = false;
	}

    public void ProcessAction ()
    {
        for (int i = 0; i < overseer.allBattlers.Length; i++) if (overseer.allBattlers[i] != null) overseer.allBattlers[i].puppet.Respond();
    }
}
