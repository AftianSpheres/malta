using UnityEngine;
using System.Collections;

/// <summary>
/// Tiny struct that holds data for a single combat setup.
/// </summary>
public struct AdventureSubstage
{
    public AdventurerClass[] enemiesClasses;
    public AdventurerSpecies[] enemiesSpecies;
    public bool[] eliteStatuses;
    public bool applyBonusStats;

    public AdventureSubstage(AdventurerClass[] _enemiesClasses, AdventurerSpecies[] _enemiesSpecies, bool[] _eliteStatuses, bool _applyBonusStats = false)
    {
        enemiesClasses = _enemiesClasses;
        enemiesSpecies = _enemiesSpecies;
        eliteStatuses = _eliteStatuses;
        applyBonusStats = _applyBonusStats;
    }
}
