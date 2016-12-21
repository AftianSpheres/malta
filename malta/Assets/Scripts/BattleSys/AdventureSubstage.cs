using UnityEngine;
using System.Collections;

public enum BattleBGMType
{
    None,
    GARBO_typing,
    GARBO_dreamchaser,
    GARBO_murder
}

internal static class BattleBGMLoader
{
    public static AudioClip LoadBGM (BattleBGMType bgm)
    {
        AudioClip clip = default(AudioClip);
        switch (bgm)
        {
            case BattleBGMType.GARBO_typing:
                clip = Resources.Load<AudioClip>("Audio/Music/typing");
                break;
            case BattleBGMType.GARBO_dreamchaser:
                clip = Resources.Load<AudioClip>("Audio/Music/dreamchaser");
                break;
            case BattleBGMType.GARBO_murder:
                clip = Resources.Load<AudioClip>("Audio/Music/longdistance");
                break;

        }
        return clip;
    }
}

/// <summary>
/// Tiny struct that holds data for a single combat setup.
/// </summary>
public struct AdventureSubstage
{
    public AdventurerClass[] enemiesClasses;
    public AdventurerSpecies[] enemiesSpecies;
    public bool[] eliteStatuses;
    public bool applyBonusStats;
    public AudioClip battleBGM;

    public AdventureSubstage(AdventurerClass[] _enemiesClasses, AdventurerSpecies[] _enemiesSpecies, bool[] _eliteStatuses, bool _applyBonusStats = false, BattleBGMType bgm = BattleBGMType.None)
    {
        enemiesClasses = _enemiesClasses;
        enemiesSpecies = _enemiesSpecies;
        eliteStatuses = _eliteStatuses;
        applyBonusStats = _applyBonusStats;
        battleBGM = BattleBGMLoader.LoadBGM(bgm);
    }
}
