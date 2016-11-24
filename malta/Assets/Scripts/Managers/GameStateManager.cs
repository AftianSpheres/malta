using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

[Serializable]
public class GameStateManager : Manager<GameStateManager>
{
    public int SessionFingerprint;

    /// <summary>
    /// MonoBehavious.Start()
    /// </summary>
    void Start ()
    {
        SessionFingerprint = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
	}
	
    /// <summary>
    /// MonoBehaviour.Update()
    /// </summary>
	void Update ()
    {

    }

    /// <summary>
    /// Generates a new session fingerprint value.
    /// </summary>
    public void RerollSessionFingerprint ()
    {
        int r = SessionFingerprint;
        while (r == SessionFingerprint)
        {
            r = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
        SessionFingerprint = r;
    }
}
