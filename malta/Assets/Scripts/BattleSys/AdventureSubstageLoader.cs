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

    private void populateAdventure0Structs ()
    {
        adventure0_0 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Warrior }, new AdventurerSpecies[] { AdventurerSpecies.Human }, new bool[] { false }, false, BattleBGMType.GARBO_typing);
        adventure0_1 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Warrior, AdventurerClass.Warrior }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false }, false, BattleBGMType.GARBO_dreamchaser);
        adventure0_2 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Warrior, AdventurerClass.Warrior, AdventurerClass.Sage }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false, true }, false, BattleBGMType.GARBO_murder);
        adventure0Substages = new AdventureSubstage[] { adventure0_0, adventure0_1, adventure0_2 };
    }

    private void populateAdventure1Structs ()
    {
        adventure1_0 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Footman, AdventurerClass.Footman }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false }, false, BattleBGMType.GARBO_typing);
        adventure1_1 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Footman, AdventurerClass.Footman, AdventurerClass.Sage }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false, false }, false, BattleBGMType.GARBO_dreamchaser);
        adventure1_2 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Wizard, AdventurerClass.Wizard, AdventurerClass.Bowman }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false, false }, false, BattleBGMType.GARBO_murder);
        adventure1Substages = new AdventureSubstage[] { adventure1_0, adventure1_1, adventure1_2 };
    }

    private void populateAdventure2Structs ()
    {
        adventure2_0 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Footman, AdventurerClass.Footman }, new AdventurerSpecies[] { AdventurerSpecies.Orc, AdventurerSpecies.Orc }, new bool[] { false, false }, false, BattleBGMType.GARBO_typing);
        adventure2_1 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Footman, AdventurerClass.Bowman, AdventurerClass.Mystic }, new AdventurerSpecies[] { AdventurerSpecies.Orc, AdventurerSpecies.Human, AdventurerSpecies.Fae }, new bool[] { false, false, false }, false, BattleBGMType.GARBO_dreamchaser);
        adventure2_2 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Footman, AdventurerClass.Wizard, AdventurerClass.Wizard }, new AdventurerSpecies[] { AdventurerSpecies.Orc, AdventurerSpecies.Human, AdventurerSpecies.Fae }, new bool[] { true, true, true }, false, BattleBGMType.GARBO_murder);
        adventure2Substages = new AdventureSubstage[] { adventure2_0, adventure2_1, adventure2_2 };
    }
}
