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
    public static AdventureSubstage[][] prebuiltAdventures;
    public const int randomAdventureBaseLevel = 3;

    // Use this for initialization
    void Awake()
    {
        if (prebuiltAdventures == null)
        {
            populateAdventure0Structs();
            prebuiltAdventures = new AdventureSubstage[][] { adventure0Substages };
        }
    }

    private void populateAdventure0Structs ()
    {
        adventure0_0 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Warrior }, new AdventurerSpecies[] { AdventurerSpecies.Human }, new bool[] { false });
        adventure0_1 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Warrior, AdventurerClass.Warrior }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false });
        adventure0_2 = new AdventureSubstage(new AdventurerClass[] { AdventurerClass.Warrior, AdventurerClass.Warrior, AdventurerClass.Sage }, new AdventurerSpecies[] { AdventurerSpecies.Human, AdventurerSpecies.Human, AdventurerSpecies.Human }, new bool[] { false, false, true });
        adventure0Substages = new AdventureSubstage[] { adventure0_0, adventure0_1, adventure0_2 };
    }
}
