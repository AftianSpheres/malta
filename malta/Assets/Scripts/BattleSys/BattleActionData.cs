﻿using UnityEngine;
using System.Collections.Generic;

public enum BattlerAction
{
    UninitializedVal = -99999,
    None = 0,
    MaceSwing,
    Siphon,
    Haste,
    SilencingShot,
    RainOfArrows,
    Bowshot,
    ShieldWall,
    ShieldBlock,
    SpearThrust,
    Barrier,
    VampiricWinds,
    BurstOfSpeed,
    Feedback,
    Inferno,
    Lightning,
    Protect,
    HammerBlow,
    GetBehindMe,
    Flanking,
    Rend,
    Bludgeon,
    ContinentSmash,
    Fortify,
    BloodOfSteel,
    WideSwing,
    CataclysmicImpact,
    Concuss,
    DreamlessSleep,
    QuickStab,
    Stiletto,
    ThroatSlit,
    BloodRites,
    BladeOfCrimson,
    FleetFeet,
    FlashStep,
    SecondSight,
    WithTheWind,
    HealingWind,
    BreathOfLife,
    Renewal,
    IceBullet,
    FrostSpear,
    CometStrike,
    ZoneOfSilence,
    SpeakNoEvil,
    AntiDrain,
    CovetNot,
    _CantMove_Silenced = 10000,
    _CantMove_Stunned = 10001
}

public enum BattlerActionInterruptType
{
    None,
    BattleStart,
    OnAllyDeathblow,
    OnAllyHit,
    OnHit
}

public enum BattlerActionAnim
{
    None,
    Hit,
    Cast,
    PhysBuff
}

[System.Flags]
public enum BattlerActionEffectFlags : ulong // these will never be serialized, so enum : ulong is okay, but note that if you tried to serialize this Unity would throw a shit fit
{
    None = 0,
    IsMagic = 1 << 0,
    NeedsTarget = 1 << 1,
    Silence = 1 << 2,
    ShieldBlock = 1 << 3,
    ShieldWall = 1 << 4,
    Drain = 1 << 5,
    Barrier = 1 << 6,
    Healing = 1 << 7,
    Encore = 1 << 8,
    Bodyguard = 1 << 9,
    Melee = 1 << 10,
    MarBoost = 1 << 11,
    MagBoost = 1 << 12,
    SpeBoost = 1 << 13,
    StunBuff = 1 << 14,
    Sacrifice = 1 << 15,
    null_16 = 1 << 16,
    FStep = 1 << 17,
    null_18 = 1 << 18,
    DodgeBuff = 1 << 19,
    ForbidOnTurn1 = 1 << 20,
    DrainBlock = 1 << 21,
    ForbidAfterTurn1 = 1 << 22,
    MaxValue = ulong.MaxValue
}

public enum BattlerActionTarget
{
    None,
    TargetWeakestEnemy,
    RandomTarget,
    RandomTarget_ButOnlyCasters,
    TargetSelf,
    TargetOwnSide,
    TargetActingBattler,
    TargetEndangeredAlly,
    HitAll
}

/// <summary>
/// A magic struct.
/// Stores all the data defining what an in-battle action actually does,
/// and also seamlessly handles the process of loading the nasty CSV fils
/// in the background. So you don't need to do any messy setup!
/// Just BattlerAnimData.get(action) from any point during execution
/// and the dataset will automagically appear.
/// </summary>
public struct BattlerActionData
{
    private static BattlerActionData[] _get { get { if (data != null) return data; else return LoadData(); } }
    private static BattlerActionData[] data;
    readonly public BattlerAction actionID;
    readonly public BattlerActionAnim anim;
    readonly public BattlerActionInterruptType interruptType;
    readonly public BattlerActionEffectFlags flags;
    readonly public BattlerActionTarget target;
    readonly public string name;
    readonly public int effectPower;
    readonly public int cooldownTurns;
    readonly public int baseDamage;
    readonly public int numberOfSubtargets;
    public const int numberOfAnims = 4;

    /// <summary>
    /// Calls into BattlerActionData, gets a BattlerActionData entry from the dataset if it exists, makes it first if it doesn't.
    /// </summary>
    public static BattlerActionData get (BattlerAction a)
    {
        return _get[(int)a];
    }

    /// <summary>
    /// Checks to see if the flag is set in the action data.
    /// Pass a flag, get a bool.
    /// </summary>
    public bool HasEffectFlag(BattlerActionEffectFlags flag)
    {
        return (flags & flag) == flag;
    }

    private BattlerActionData (int index, string line)
    {
        actionID = (BattlerAction)index;
        string[] terms = line.Split(',');
        name = terms[0];
        cooldownTurns = int.Parse(terms[1]);
        effectPower = int.Parse(terms[2]);
        interruptType = ParseInterruptType(terms[3]);
        anim = ParseAnim(terms[4]);
        target = ParseTarget(terms[5]);
        baseDamage = int.Parse(terms[6]);
        numberOfSubtargets = int.Parse(terms[7]);
        flags = ParseListOfFlags(terms);
        if ((flags & BattlerActionEffectFlags.Healing) == BattlerActionEffectFlags.Healing) baseDamage = baseDamage * -1;
    }

    private static BattlerActionAnim ParseAnim (string animStr)
    {
        BattlerActionAnim a = BattlerActionAnim.None;
        switch (animStr)
        {
            case "None":
                break;
            case "Hit":
                a = BattlerActionAnim.Hit;
                break;
            case "Cast":
                a = BattlerActionAnim.Cast;
                break;
            case "PhysBuff":
                a = BattlerActionAnim.PhysBuff;
                break;
            default:
                throw new System.NotSupportedException("BattlerActionData entry has a bad anim type: " + animStr);
        }
        return a;
    }

    private static BattlerActionInterruptType ParseInterruptType(string interruptStr)
    {
        BattlerActionInterruptType interruptType = BattlerActionInterruptType.None;
        switch (interruptStr)
        {
            case "None":
                break;
            case "BattleStart":
                interruptType = BattlerActionInterruptType.BattleStart;
                break;
            case "OnAllyDeathblow":
                interruptType = BattlerActionInterruptType.OnAllyDeathblow;
                break;
            case "OnAllyHit":
                interruptType = BattlerActionInterruptType.OnAllyHit;
                break;
            case "OnHit":
                interruptType = BattlerActionInterruptType.OnHit;
                break;
            default:
                throw new System.NotSupportedException("BattlerActionData entry has a bad interupt type: " + interruptStr);
        }
        return interruptType;
    }

    private static BattlerActionEffectFlags ParseListOfFlags (string[] flagsList)
    {
        const int firstTermThatsActuallyAFlag = 8;
        BattlerActionEffectFlags flags = 0;
        for (int i = firstTermThatsActuallyAFlag; i < flagsList.Length; i++)
        {
            bool isValidFlag = false;
            for (ulong b = 1; b < (ulong)1 << 63;)
            {
                if (flagsList[i] == ((BattlerActionEffectFlags)b).ToString())
                {
                    flags |= (BattlerActionEffectFlags)b;
                    isValidFlag = true;
                    break;
                }
                b = b << 1;
            }
            if (isValidFlag == false) throw new System.NotSupportedException("BattlerActionData entry has a bad flag: " + flagsList[i]);
        }
        return flags;
    }

    private static BattlerActionTarget ParseTarget (string targetStr)
    {
        BattlerActionTarget t = BattlerActionTarget.None;
        switch (targetStr)
        {
            case "None":
                break;
            case "TargetWeakestEnemy":
                t = BattlerActionTarget.TargetWeakestEnemy;
                break;
            case "RandomTarget":
                t = BattlerActionTarget.RandomTarget;
                break;
            case "RandomTarget_ButOnlyCasters":
                t = BattlerActionTarget.RandomTarget_ButOnlyCasters;
                break;
            case "TargetSelf":
                t = BattlerActionTarget.TargetSelf;
                break;
            case "TargetOwnSide":
                t = BattlerActionTarget.TargetOwnSide;
                break;
            case "TargetActingBattler":
                t = BattlerActionTarget.TargetActingBattler;
                break;
            case "TargetEndangeredAlly":
                t = BattlerActionTarget.TargetEndangeredAlly;
                break;
            case "HitAll":
                t = BattlerActionTarget.HitAll;
                break;
            default:
                throw new System.NotSupportedException("BattlerActionData entry has a bad targeting type: " + targetStr);
        }
        return t;
    }

    private static BattlerActionData[] LoadData ()
    {
        List<BattlerActionData> dat = new List<BattlerActionData>();
        string[] lines = Resources.Load<TextAsset>("BattlerActionData").text.Split(new string[] {"\r\n", "\n" }, System.StringSplitOptions.None);
        for (int i = 0; i < lines.Length; i++) dat.Add(new BattlerActionData(i, lines[i]));
        data = dat.ToArray();
        return data;
    }
}
