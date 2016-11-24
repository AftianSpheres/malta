using UnityEngine;
using System;
using System.Collections;

public class PlayerSettingsManager : Manager<PlayerSettingsManager>
{
    public static float VolumeMin = 0.0f;
    public static float VolumeMax = 1.0f;
    public float MusicVolume = 1f;
    public float SFXVolume = 1f;

	// Use this for initialization
	void Awake ()
    {
        LoadFromPlayerPrefs();
	}
	
    void LoadFromPlayerPrefs()
    {
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
    }

    public void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
    }

	// Update is called once per frame
	void Update ()
    {
	}

}
