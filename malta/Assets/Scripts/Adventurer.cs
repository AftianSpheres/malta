using UnityEngine;
using System.Collections;

public class Adventurer : MonoBehaviour
{
    public string firstName;
    public string lastName;
    public AdventurerClass advClass;
    public AdventurerSpecies species;


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static int[] GetSpeciesStatMods (AdventurerSpecies species)
    {
        int[] mods = { 0, 0, 0 };
        switch (species)
        {
            case AdventurerSpecies.Fae:
                mods = new int[] { -1, 2, -1 };
                break;
            case AdventurerSpecies.Orc:
                mods = new int[] { 1, -2, 1};
                break;
            case AdventurerSpecies.Aeon:
                mods = new int[] { 5, 5, 5 };
                break;
        }
        return mods;
    }

    public static string GetSpeciesTerm (AdventurerSpecies species, bool asAdjective = false)
    {
        string term = "DEFAULT";
        switch (species)
        {
            case AdventurerSpecies.Human:
                term = "Human";
                break;
            case AdventurerSpecies.Fae:
                if (asAdjective) term = "Fae";
                else term = "Faerie";
                break;
            case AdventurerSpecies.Orc:
                if (asAdjective) term = "Orcish";
                else term = "Orc";
                break;
            case AdventurerSpecies.Aeon:
                term = "Divine";
                break;
        }
        return term;
    }
}
