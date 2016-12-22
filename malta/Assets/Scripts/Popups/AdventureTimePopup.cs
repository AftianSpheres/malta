﻿using UnityEngine;
using UnityEngine.UI;

public class AdventureTimePopup : MonoBehaviour
{
    public PopupMenu shell;
    public SovereignInfoPanel sovereignInfo;
    public GameObject matsNeededSection;
    public PopupMenu insufficientResourcesPopup;
    public Text adventureDestination;
    public Text adventureDetails;
    public Text numBricks;
    public Text numMetal;
    public Text numPlanks;
    public TextAsset stringsResource;
    public ScreenChanger screenChanger;
    private int cachedAdventureLv = -1;
    private string[] strings;

	// Use this for initialization
	void Start ()
    {
        strings = sovereignInfo.strings = stringsResource.text.Split('\n');
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if (GameDataManager.Instance != null)
        {
            if (cachedAdventureLv != GameDataManager.Instance.dataStore.adventureLevel)
            {
                cachedAdventureLv = GameDataManager.Instance.dataStore.adventureLevel;
                switch (cachedAdventureLv)
                {
                    case 0:
                        adventureDestination.text = strings[23];
                        adventureDetails.text = strings[24];
                        break;
                    case 1:
                        adventureDestination.text = strings[25];
                        adventureDetails.text = strings[26];
                        break;
                    case 2:
                        adventureDestination.text = strings[27];
                        adventureDetails.text = strings[28];
                        break;
                    default:
                        adventureDestination.text = strings[29] + (cachedAdventureLv - 2).ToString();
                        adventureDetails.text = strings[30];
                        break;
                }
                if (cachedAdventureLv < 3) matsNeededSection.SetActive(false);
                else
                {
                    matsNeededSection.SetActive(true);
                    numBricks.text = numMetal.text = numPlanks.text = GameDataManager.Instance.dataStore.nextRandomAdventureAnte.ToString();
                }
            }
        }
	}

    public void Depart ()
    {
        int[] costs = { 0, 0, 0, GameDataManager.Instance.dataStore.nextRandomAdventureAnte, GameDataManager.Instance.dataStore.nextRandomAdventureAnte, GameDataManager.Instance.dataStore.nextRandomAdventureAnte };
        if (GameDataManager.Instance.dataStore.adventureLevel >= AdventureSubstageLoader.randomAdventureBaseLevel)
        {
            if (GameDataManager.Instance.SpendResourcesIfPossible(costs))
            {
                shell.Close();
                screenChanger.Activate();
            }
            else insufficientResourcesPopup.Open();
        }
        else
        {
            shell.Close();
            screenChanger.Activate();
        }
    }
}
