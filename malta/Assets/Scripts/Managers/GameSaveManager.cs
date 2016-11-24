using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class GameSaveManager : Manager<GameSaveManager>
{
    public static string saveFilePath = "hammersave*.dat";
    public string[] saveFiles;
    public string saveFileID;
    private GameStateManager gameStateManager;
    private PlayerSettingsManager playerSettingsManager;
    private HardwareInterfaceManager hardwareInterfaceManager;


	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void GetFileList ()
    {
        saveFiles = Directory.GetFiles(saveFilePath, saveFilePath, SearchOption.TopDirectoryOnly);
    }
}
