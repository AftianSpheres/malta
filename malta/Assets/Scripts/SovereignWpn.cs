using UnityEngine;
using System;
using System.Collections.Generic;

public enum WpnType
{
    None,
    Mace,
    Knives,
    Staff
}

public enum WpnClass
{
    None,
    Simple_0,
    Wooden_0,
    Carved_0,
    Warlord_1,
    Royal_1,
    Rugged_1,
    Master_2,
    Empyrean_2,
    Abyss_2
}

internal enum WpnMetaclass
{
    None,
    Basic,
    Balanced,
    Technical
}

[Serializable]
public class SovereignWpn
{
    public int HP;
    public int Martial;
    public int Magic;
    public int Speed;
    public BattlerAction[] attacks;
    public WpnType wpnType;
    public WpnClass wpnClass;
    public string wpnName;
    public int wpnLevel;
    readonly private static WpnClass[] basicWpnClasses = { WpnClass.Simple_0, WpnClass.Warlord_1, WpnClass.Master_2 };
    readonly private static WpnClass[] balancedWpnClasses = { WpnClass.Wooden_0, WpnClass.Royal_1, WpnClass.Empyrean_2 };
    readonly private static WpnClass[] technicalWpnClasses = { WpnClass.Carved_0, WpnClass.Rugged_1, WpnClass.Abyss_2 };
    readonly private static WpnClass[][] wpnClassesLookup = { basicWpnClasses, balancedWpnClasses, technicalWpnClasses };
    readonly private static int[] tier0BasicBaseStats = { 10, 3, 2, 0 }; // this is baseHP, priority 0 stat, priority 1 stat, priority 2 stat - weapon class determines which of these values correspond to which stats
    readonly private static int[] tier1BasicBaseStats = { 25, 4, 3, 1 };
    readonly private static int[] tier2BasicBaseStats = { 40, 5, 3, 2 };
    readonly private static int[] tier0BalancedBaseStats = { 12, 2, 2, 1 };
    readonly private static int[] tier1BalancedBaseStats = { 30, 3, 3, 2 };
    readonly private static int[] tier2BalancedBaseStats = { 50, 4, 4, 3 };
    readonly private static int[] tier0TechnicalBaseStats = { 8, 4, 2, 0 };
    readonly private static int[] tier1TechnicalBaseStats = { 18, 5, 3, 1 };
    readonly private static int[] tier2TechnicalBaseStats = { 35, 6, 3, 2 };
    readonly private static int[][] tier0WpnBaseStats = { tier0BasicBaseStats, tier0BalancedBaseStats, tier0TechnicalBaseStats };
    readonly private static int[][] tier1WpnBaseStats = { tier1BasicBaseStats, tier1BalancedBaseStats, tier1TechnicalBaseStats };
    readonly private static int[][] tier2WpnBaseStats = { tier2BasicBaseStats, tier2BalancedBaseStats, tier2TechnicalBaseStats };
    readonly private static int[][][] wpnBaseStatsLookup = { tier0WpnBaseStats, tier1WpnBaseStats, tier2WpnBaseStats };
    private static string[] wpnClassNames;
    private const string wpnClassNamesResourcePath = "wpn_classes";
    private static string[] wpnTypeNames;
    private const string wpnTypeNamesResourcePath = "wpn_types";
    private static string[] wpnPrefixes;
    private const string wpnPrefixesResourcePath = "wpn_prefixes";


    public SovereignWpn(int lv, WpnType _wpnType)
    {
        Debug.Log("Rolling weapon: level " + lv.ToString() + " " + _wpnType);
        wpnLevel = lv;
        wpnType = _wpnType;
        WpnMetaclass metaclass = (WpnMetaclass)UnityEngine.Random.Range(1, 4);
        wpnClass = wpnClassesLookup[(int)metaclass - 1][wpnLevel];
        ApocalypticallyBadAttackArrayGen(metaclass);
        RollWpnStats(metaclass);
        GetName();
    }

    void GetName()
    {
        if (wpnClassNames == null)
        {
            TextAsset a = Resources.Load<TextAsset>(wpnClassNamesResourcePath);
            if (a == null) throw new Exception("Can't get wpn class strings from file");
            wpnClassNames = a.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        }
        if (wpnTypeNames == null)
        {
            TextAsset a = Resources.Load<TextAsset>(wpnTypeNamesResourcePath);
            if (a == null) throw new Exception("Can't get wpn type strings from file");
            wpnTypeNames = a.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        }
        if (wpnPrefixes == null)
        {
            TextAsset a = Resources.Load<TextAsset>(wpnPrefixesResourcePath);
            if (a == null) throw new Exception("Can't get wpn prefix strings from file");
            wpnPrefixes = a.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        }
        wpnName = wpnPrefixes[UnityEngine.Random.Range(0, wpnPrefixes.Length)] + " " + wpnClassNames[(int)wpnClass] + " " + wpnTypeNames[(int)wpnType]; 
    }

    void RollWpnStats (WpnMetaclass metaclass)
    {
        int[] baseStats = wpnBaseStatsLookup[wpnLevel][(int)metaclass - 1];
        HP = baseStats[0];
        switch (wpnType)
        {
            case WpnType.Mace:
                HP = Mathf.RoundToInt(HP * 1.5f);
                Martial = baseStats[1];
                Speed = baseStats[2];
                Magic = baseStats[3];
                break;
            case WpnType.Knives:
                HP = Mathf.RoundToInt(HP * 0.66f);
                Speed = baseStats[1] + 1;
                Martial = baseStats[2] + 1;
                Magic = baseStats[3];
                break;
            case WpnType.Staff:
                Magic = baseStats[1] + 1;
                Speed = baseStats[2];
                Martial = baseStats[3];
                break;
        }
    }

    /// <summary>
    /// NO PEACE.
    /// </summary>
    void ApocalypticallyBadAttackArrayGen (WpnMetaclass metaclass)
    {
        int advancedMovesLeft = 1;
        if (metaclass == WpnMetaclass.Technical) advancedMovesLeft++;
        attacks = new BattlerAction[3];
        Queue<Func<int, BattlerAction>> specialAttacksGetMethods = new Queue<Func<int, BattlerAction>>(3);
        specialAttacksGetMethods.Enqueue(GetSpecial0);
        specialAttacksGetMethods.Enqueue(GetSpecial1);
        specialAttacksGetMethods.Enqueue(GetSpecial2);
        Func<int, BattlerAction>[] specialAttacksGetMethods_shuffled = new Func<int, BattlerAction>[3];
        for (int i = 0; i < 3; i++)
        {
            int r = UnityEngine.Random.Range(0, 3);
            if (specialAttacksGetMethods_shuffled[r] != null) i--;
            else specialAttacksGetMethods_shuffled[r] = specialAttacksGetMethods.Dequeue();
        }
        for (int i = 0; i < specialAttacksGetMethods_shuffled.Length; i++) specialAttacksGetMethods.Enqueue(specialAttacksGetMethods_shuffled[i]);
        for (int i = 0; i < attacks.Length; i++) attacks[i] = BattlerAction.UninitializedVal;
        if (wpnLevel == 0)
        {
            if (metaclass == WpnMetaclass.Technical) attacks[0] = GetBasicAttack(1);
            else attacks[0] = GetBasicAttack(0);
            attacks[1] = specialAttacksGetMethods.Dequeue()(1);
            attacks[2] = BattlerAction.None;
        }
        else if (wpnLevel == 1)
        {
            int r = UnityEngine.Random.Range(0, 3);
            while (advancedMovesLeft > 0)
            {
                if (r == 0)
                {
                    if (attacks[0] == BattlerAction.UninitializedVal)
                    {
                        advancedMovesLeft--;
                        attacks[0] = GetBasicAttack(1);
                    }
                }
                else
                {
                    if (attacks[1] == BattlerAction.UninitializedVal)
                    {
                        advancedMovesLeft--;
                        attacks[1] = specialAttacksGetMethods.Dequeue()(1);
                    }
                    else if (attacks[2] == BattlerAction.UninitializedVal)
                    {
                        advancedMovesLeft--;
                        attacks[2] = specialAttacksGetMethods.Dequeue()(1);
                    }
                }
            }
            if (attacks[1] == BattlerAction.UninitializedVal) attacks[1] = specialAttacksGetMethods.Dequeue()(1);
            if (attacks[0] == BattlerAction.UninitializedVal)
            {
                attacks[0] = GetBasicAttack(0);
                if (attacks[2] == BattlerAction.UninitializedVal) attacks[2] = specialAttacksGetMethods.Dequeue()(1);
            }
            else attacks[2] = BattlerAction.None;
        }
        else if (wpnLevel == 2)
        {
            while (advancedMovesLeft > 0)
            {
                int r = UnityEngine.Random.Range(0, 3);
                if (attacks[r] == BattlerAction.UninitializedVal)
                {
                    if (r == 0) attacks[r] = GetBasicAttack(2);
                    else attacks[r] = specialAttacksGetMethods.Dequeue()(2);
                    advancedMovesLeft--;
                }
            }
            if (attacks[0] == BattlerAction.UninitializedVal) attacks[0] = GetBasicAttack(1);
            if (attacks[1] == BattlerAction.UninitializedVal) attacks[1] = specialAttacksGetMethods.Dequeue()(1);
            if (attacks[2] == BattlerAction.UninitializedVal) attacks[2] = specialAttacksGetMethods.Dequeue()(1);
        }
        if (wpnType == WpnType.Staff)
        {
            ShittyGen_HackySanityCheckForStaves(ref attacks[1]);
            ShittyGen_HackySanityCheckForStaves(ref attacks[2]);
        }
    }

    /// <summary>
    /// Fixing this properly means rewriting the attack array gen as something marginally sane, so this is here until that happens.
    /// </summary>
    void ShittyGen_HackySanityCheckForStaves(ref BattlerAction checkingAtk)
    {
        if (attacks[0] == BattlerAction.IceBullet || attacks[0] == BattlerAction.FrostSpear || attacks[0] == BattlerAction.CometStrike)
        {
            if (checkingAtk == BattlerAction.IceBullet) checkingAtk = BattlerAction.HealingWind;
            else if (checkingAtk == BattlerAction.FrostSpear) checkingAtk = BattlerAction.BreathOfLife;
            else if (checkingAtk == BattlerAction.CometStrike) checkingAtk = BattlerAction.Renewal;
        }
        else if (attacks[0] == BattlerAction.HealingWind || attacks[0] == BattlerAction.BreathOfLife || attacks[0] == BattlerAction.Renewal)
        {
            if (checkingAtk == BattlerAction.HealingWind) checkingAtk = BattlerAction.IceBullet;
            else if (checkingAtk == BattlerAction.BreathOfLife) checkingAtk = BattlerAction.FrostSpear;
            else if (checkingAtk == BattlerAction.Renewal) checkingAtk = BattlerAction.CometStrike;
        }
    }

    BattlerAction GetBasicAttack (int lv)
    {
        switch (wpnType)
        {
            case WpnType.Mace:
                if (lv == 0) return BattlerAction.Bludgeon;
                else if (lv == 1) return BattlerAction.HammerBlow;
                else return BattlerAction.ContinentSmash;
            case WpnType.Knives:
                if (lv == 0) return BattlerAction.QuickStab;
                else if (lv == 1) return BattlerAction.Stiletto;
                else return BattlerAction.ThroatSlit;
            case WpnType.Staff:
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    if (lv == 0) return BattlerAction.HealingWind;
                    else if (lv == 1) return BattlerAction.BreathOfLife;
                    else return BattlerAction.Renewal;
                }
                else
                {
                    if (lv == 0) return BattlerAction.IceBullet;
                    else if (lv == 1) return BattlerAction.FrostSpear;
                    else return BattlerAction.CometStrike;
                }
            default:
                throw new System.Exception("Tried to get basic attack for bad weapon type: " + wpnType.ToString());
        }
    }

    BattlerAction GetSpecial0 (int lv)
    {
        BattlerAction a = BattlerAction.None;
        switch (wpnType)
        {
            case WpnType.Mace:
                if (lv > 1) a = BattlerAction.BloodOfSteel;
                else if (lv > 0) a = BattlerAction.Fortify;
                break;
            case WpnType.Knives:
                if (lv > 1) a = BattlerAction.BladeOfCrimson;
                else if (lv > 0) a = BattlerAction.BloodRites;
                break;
            case WpnType.Staff:
                if (lv > 1) a = BattlerAction.SpeakNoEvil;
                else if (lv > 0) a = BattlerAction.ZoneOfSilence;
                break;
        }
        return a;
    }

    BattlerAction GetSpecial1 (int lv)
    {
        BattlerAction a = BattlerAction.None;
        switch (wpnType)
        {
            case WpnType.Mace:
                if (lv > 1) a = BattlerAction.CataclysmicImpact;
                else if (lv > 0) a = BattlerAction.WideSwing;
                break;
            case WpnType.Knives:
                if (lv > 1) a = BattlerAction.FlashStep;
                else if (lv > 0) a = BattlerAction.FleetFeet;
                break;
            case WpnType.Staff:
                if (lv > 1) a = BattlerAction.CovetNot;
                else if (lv > 0) a = BattlerAction.AntiDrain;
                break;
        }
        return a;
    }

    BattlerAction GetSpecial2 (int lv)
    {
        BattlerAction a = BattlerAction.None;
        switch (wpnType)
        {
            case WpnType.Mace:
                if (lv > 1) a = BattlerAction.DreamlessSleep;
                else if (lv > 0) a = BattlerAction.Concuss;
                break;
            case WpnType.Knives:
                if (lv > 1) a = BattlerAction.WithTheWind;
                else if (lv > 0) a = BattlerAction.SecondSight;
                break;
            case WpnType.Staff:
                if (wpnLevel == 0)
                {
                    if (UnityEngine.Random.Range(0, 2) == 0) a = GetSpecial1(lv);
                    else a = GetSpecial0(lv);
                }
                else if (attacks[0] == BattlerAction.IceBullet || attacks[0] == BattlerAction.FrostSpear || attacks[0] == BattlerAction.CometStrike)
                {
                    if (lv > 1) a = BattlerAction.BreathOfLife;
                    else if (lv > 0) a = BattlerAction.HealingWind;
                }
                else
                {
                    if (lv > 1) a = BattlerAction.FrostSpear;
                    else if (lv > 0) a = BattlerAction.IceBullet;
                }
                break;
        }
        return a;
    }
}
