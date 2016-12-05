﻿using UnityEngine;
using UnityEngine.UI;

public class AdventurerWatcher : MonoBehaviour
{
    public bool isForgeAdventurer;
    public int houseAdventurerIndex;
    public GameObject interior;
    public Text adventurerName;
    public Text adventurerTitle;
    public Text adventurerStats;
    public Text adventurerAttacks;
    public Text adventurerSpecial;
    private Adventurer adventurer;
    public TextAsset stringsResource;
    private int[] cachedAdventurerStats = { -1, -1, -1, -1 };
    private AdventurerAttack[] cachedAdventurerAttacks = { AdventurerAttack.None, AdventurerAttack.None };
    private AdventurerSpecial cachedAdventurerSpecial = AdventurerSpecial.LoseBattle;
    private string[] strings;

	// Use this for initialization
	void Start ()
    {
        strings = stringsResource.text.Split('\n');
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (adventurer == null)
        {
            if (isForgeAdventurer)
            {
                if (GameDataManager.Instance.forgeAdventurer != null && GameDataManager.Instance.unlock_forgeOutbuilding && !GameDataManager.Instance.unlock_Taskmaster) adventurer = GameDataManager.Instance.forgeAdventurer;
            }             
            else if (GameDataManager.Instance.houseAdventurers[houseAdventurerIndex] != null && GameDataManager.Instance.housesBuilt[houseAdventurerIndex]) adventurer = GameDataManager.Instance.houseAdventurers[houseAdventurerIndex];
        }
        if (adventurer != null && adventurer.initialized)
        {
            if (!interior.activeInHierarchy) interior.SetActive(true);
            if (adventurerName.text != adventurer.fullName) adventurerName.text = adventurer.fullName;
            if (adventurerTitle.text != adventurer.title) adventurerTitle.text = adventurer.title;
            if (cachedAdventurerStats[0] != adventurer.HP || cachedAdventurerStats[1] != adventurer.Martial || cachedAdventurerStats[2] != adventurer.Magic || cachedAdventurerStats[3] != adventurer.Speed)
            {
                cachedAdventurerStats = new int[] { adventurer.HP, adventurer.Martial, adventurer.Magic, adventurer.Speed };
                adventurerStats.text =  strings[0] + cachedAdventurerStats[0].ToString() + strings[1] + cachedAdventurerStats[1].ToString() + strings[2] +
                    cachedAdventurerStats[2].ToString() + strings[3] + cachedAdventurerStats[3].ToString();
            }
            if (cachedAdventurerSpecial != adventurer.special)
            {
                cachedAdventurerSpecial = adventurer.special;
                adventurerSpecial.text = Adventurer.GetSpecialDescription(cachedAdventurerSpecial);
            }
            if (cachedAdventurerAttacks[0] != adventurer.attacks[0] || cachedAdventurerAttacks[1] != adventurer.attacks[1])
            {
                cachedAdventurerAttacks = adventurer.attacks.Clone() as AdventurerAttack[];
                string[] attacksStrings = { "", "" };
                for (int i = 0; i < cachedAdventurerAttacks.Length && i < 2; i++)
                {
                    if (cachedAdventurerAttacks[i] != AdventurerAttack.None)
                    {
                        attacksStrings[i] = Adventurer.GetAttackName(cachedAdventurerAttacks[i]);
                    }
                    else
                    {
                        attacksStrings[i] = "";
                    }
                }
                adventurerAttacks.text = attacksStrings[0] + System.Environment.NewLine + attacksStrings[1];
            }

        }
        else if (interior.activeInHierarchy) interior.SetActive(false);
	}
}
