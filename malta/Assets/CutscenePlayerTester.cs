using UnityEngine;
using System.Collections;

public class CutscenePlayerTester : MonoBehaviour
{
    public CutscenePlayer cutscenePlayer;
    public bool activate;
	
	// Update is called once per frame
	void Update ()
    {
	    if (activate)
        {
            cutscenePlayer.StartCutscene("demoEnding");
            DestroyImmediate(gameObject);
        }
	}
}
