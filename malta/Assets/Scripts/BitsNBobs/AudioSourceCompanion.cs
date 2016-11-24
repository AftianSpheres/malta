using UnityEngine;
using System.Collections;

public class AudioSourceCompanion : MonoBehaviour
{
    public bool isMusic = false;
    public AudioSource source;
    private float origVolume;
    private float volumeSettingBuffer = float.MaxValue; // this is specifically "something out of range"
    private PlayerSettingsManager playerSettingsManager;

	// Use this for initialization
	void Awake ()
    {
        origVolume = source.volume;
        Mate();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (playerSettingsManager == null)
        {
            Mate();
        }
        if (playerSettingsManager != null)
        {
            if (isMusic == true)
            {
                if (playerSettingsManager.MusicVolume != volumeSettingBuffer)
                {
                    volumeSettingBuffer = playerSettingsManager.MusicVolume;
                    source.volume = origVolume * playerSettingsManager.MusicVolume;
                }
            }
            else
            {
                if (playerSettingsManager.SFXVolume != volumeSettingBuffer)
                {
                    volumeSettingBuffer = playerSettingsManager.SFXVolume;
                    source.volume = origVolume * playerSettingsManager.SFXVolume;
                }
            }
        }
	}

    void Mate()
    {
        GameObject psmObj = GameObject.Find("Universe/PlayerSettingsManager");
        if (psmObj != null)
        {
            playerSettingsManager = psmObj.GetComponent<PlayerSettingsManager>();
        }
    }
}
