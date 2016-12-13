using UnityEngine;
using System.Collections;

/// <summary>
/// Doesn't actually -load- anything right now, just builds some structs with shitty hardcoded logic.
/// </summary>
public class AdventureSubstageLoader : MonoBehaviour
{
    private static AdventureSubstage adventure0_0;
    private static AdventureSubstage adventure0_1;
    private static AdventureSubstage adventure0_2;
    private static AdventureSubstage[] adventure0Substages;
    private static AdventureSubstage adventure1_0;
    private static AdventureSubstage adventure1_1;
    private static AdventureSubstage adventure1_2;
    private static AdventureSubstage[] adventure1Substages;
    private static AdventureSubstage adventure2_0;
    private static AdventureSubstage adventure2_1;
    private static AdventureSubstage adventure2_2;
    private static AdventureSubstage[] adventure2Substages;
    private static AdventureSubstage adventureR_0;
    private static AdventureSubstage adventureR_1;
    private static AdventureSubstage adventureR_2;
    private static AdventureSubstage[] randomAdventureSubstages;
    public static AdventureSubstage[] randomAdventure { get { return _NewRandomAdventure(); } }
    public static AdventureSubstage[][] prebuiltAdventures;
    public const int randomAdventureBaseLevel = 3;

    // Use this for initialization
    void Awake()
    {
        if (prebuiltAdventures == null)
        {
            populateAdventure0Structs();
            populateAdventure1Structs();
            populateAdventure2Structs();
            prebuiltAdventures = new AdventureSubstage[][] { adventure0Substages, adventure1Substages, adventure2Substages };
        }
    }

    private static AdventureSubstage[] _NewRandomAdventure ()
    {
        populateRandomAdventureStructs();
        return randomAdventureSubstages;
    }

    private static void populateAdventure0Structs ()
    {
        adventure0_0 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Warrior }, new AdventurerSpecies[] { AdventurerSpecies.Human }, new bool[] { false }, false, BattleBGMType.GARBO_typing);
        adventure0_1 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Warrior, AdventurerClass.Warrior }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false }, false, BattleBGMType.GARBO_dreamchaser);
        adventure0_2 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Warrior, AdventurerClass.Warrior, AdventurerClass.Sage }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false, true }, false, BattleBGMType.GARBO_murder);
        adventure0Substages = new AdventureSubstage[] { adventure0_0, adventure0_1, adventure0_2 };
    }

    private static void populateAdventure1Structs ()
    {
        adventure1_0 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Footman, AdventurerClass.Footman }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false }, false, BattleBGMType.GARBO_typing);
        adventure1_1 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Footman, AdventurerClass.Footman, AdventurerClass.Sage }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false, false }, false, BattleBGMType.GARBO_dreamchaser);
        adventure1_2 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Wizard, AdventurerClass.Wizard, AdventurerClass.Bowman }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false, false }, false, BattleBGMType.GARBO_murder);
        adventure1Substages = new AdventureSubstage[] { adventure1_0, adventure1_1, adventure1_2 };
    }

    private static void populateAdventure2Structs ()
    {
        adventure2_0 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Footman, AdventurerClass.Footman }, new AdventurerSpecies[] { AdventurerSpecies.Orc, AdventurerSpecies.Orc }, new bool[] { false, false }, false, BattleBGMType.GARBO_typing);
        adventure2_1 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Footman, AdventurerClass.Bowman, AdventurerClass.Mystic }, new AdventurerSpecies[] { AdventurerSpecies.Orc, AdventurerSpecies.Human, AdventurerSpecies.Fae }, new bool[] { false, false, false }, false, BattleBGMType.GARBO_dreamchaser);
        adventure2_2 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Footman, AdventurerClass.Wizard, AdventurerClass.Wizard }, new AdventurerSpecies[] { AdventurerSpecies.Orc, AdventurerSpecies.Human, AdventurerSpecies.Fae }, new bool[] { true, true, true }, false, BattleBGMType.GARBO_murder);
        adventure2Substages = new AdventureSubstage[] { adventure2_0, adventure2_1, adventure2_2 };
    }

    private static void populateRandomAdventureStructs ()
    {
        AdventurerClass[] classArray_0 = new AdventurerClass[3];
        AdventurerSpecies[] speciesArray_0 = new AdventurerSpecies[3];
        for (int i = 0; i < 3; i++) _generateRandomChump(i, ref classArray_0, ref speciesArray_0);
        adventureR_0 = new AdventureSubstage(classArray_0, speciesArray_0, new bool[] { false, false, false }, true, BattleBGMType.GARBO_typing);
        AdventurerClass[] classArray_1 = new AdventurerClass[3];
        AdventurerSpecies[] speciesArray_1 = new AdventurerSpecies[3];
        _generateRandomHighTierOffense(0, ref classArray_1, ref speciesArray_1);
        _generateRandomHighTierSupport(1, ref classArray_1, ref speciesArray_1);
        int r = Random.Range(0, 2);
        if (r == 0) _generateRandomHighTierOffense(2, ref classArray_1, ref speciesArray_1);
        else _generateRandomHighTierSupport(2, ref classArray_1, ref speciesArray_1);
        adventureR_1 = new AdventureSubstage(classArray_1, speciesArray_1, new bool[] { false, false, true }, true, BattleBGMType.GARBO_dreamchaser);
        AdventurerClass[] classArray_2 = new AdventurerClass[3];
        AdventurerSpecies[] speciesArray_2 = new AdventurerSpecies[3];
        _generateRandomHighTierSupport(0, ref classArray_2, ref speciesArray_2);
        _generateRandomHighTierSupport(1, ref classArray_2, ref speciesArray_2);
        _generateRandomBoss(2, ref classArray_2, ref speciesArray_2);
        adventureR_2 = new AdventureSubstage(classArray_2, speciesArray_2, new bool[] { true, true, true }, true, BattleBGMType.GARBO_murder);
        randomAdventureSubstages = new AdventureSubstage[] { adventureR_0, adventureR_1, adventureR_2 };
    }

    private static void _generateRandomChump (int index, ref AdventurerClass[] cArray, ref AdventurerSpecies[] sArray)
    {
        AdventurerClass c;
        AdventurerSpecies s;
        int r = Random.Range(0, 9);
        if (r == 0) c = AdventurerClass.Bowman;
        else if (r == 1) c = AdventurerClass.Footman;
        else if (r == 2) c = AdventurerClass.Sage;
        else if (r == 3) c = AdventurerClass.Wizard;
        else if (r < 6) c = AdventurerClass.Mystic;
        else c = AdventurerClass.Warrior;
        r = Random.Range(0, 101);
        if (r < 33) s = AdventurerSpecies.Human;
        else if (r < 66) s = AdventurerSpecies.Fae;
        else if (r < 99) s = AdventurerSpecies.Orc;
        else s = AdventurerSpecies.Aeon; // 1%: you are dead, dead, dead 
        cArray[index] = c;
        sArray[index] = s;
    }

    private static void _generateRandomHighTierOffense (int index, ref AdventurerClass[] cArray, ref AdventurerSpecies[] sArray)
    {
        AdventurerClass c;
        AdventurerSpecies s;
        int r = Random.Range(0, 2);
        if (r == 0)
        {
            c = AdventurerClass.Wizard;
            s = AdventurerSpecies.Fae;
        }
        else
        {
            c = AdventurerClass.Bowman;
            s = AdventurerSpecies.Orc;
        }
        cArray[index] = c;
        sArray[index] = s;
    }

    private static void _generateRandomHighTierSupport (int index, ref AdventurerClass[] cArray, ref AdventurerSpecies[] sArray)
    {
        AdventurerClass c;
        AdventurerSpecies s;
        int r = Random.Range(0, 2);
        if (r == 0)
        {
            c = AdventurerClass.Sage;
            s = AdventurerSpecies.Fae;
        }
        else
        {
            c = AdventurerClass.Footman;
            s = AdventurerSpecies.Orc;
        }
        cArray[index] = c;
        sArray[index] = s;
    }

    private static void _generateRandomBoss(int index, ref AdventurerClass[] cArray, ref AdventurerSpecies[] sArray)
    {
        AdventurerClass c;
        AdventurerSpecies s;
        int r = Random.Range(0, 4);
        if (r == 0) c = AdventurerClass.Bowman;
        else if (r == 1) c = AdventurerClass.Footman;
        else if (r == 2) c = AdventurerClass.Sage;
        else c = AdventurerClass.Wizard;
        r = Random.Range(0, 25);
        if (r == 0) s = AdventurerSpecies.Aeon; // randomly-generated fuck-you boss!
        else s = AdventurerSpecies.Human;
        cArray[index] = c;
        sArray[index] = s;
    }
}
