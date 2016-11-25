using UnityEngine;
using System.Collections;

public class Adventurer : ScriptableObject
{
    public string firstName;
    public string lastName;
    public string fullName;
    public string title;
    public string fullTitle;
    public AdventurerAttack[] attacks;
    public AdventurerClass advClass = AdventurerClass.Warrior;
    public AdventurerSpecial special;
    public AdventurerSpecies species = AdventurerSpecies.Human;
    public int HP;
    public int individualHP;
    public int Martial;
    public int individualMartial;
    public int Magic;
    public int individualMagic;
    public int Speed;
    public int individualSpeed;
    public bool isElite;
    public bool initialized { get; private set; }
    private static string[] attackNames;
    private static string[] specialNames;
    private const string specialDescsResourcePath = "special_descs/";

    void CalcStats ()
    {
        int[] baseStats = GetClassStats(advClass);
        int[] statMods = GetSpeciesStatMods(species);
        HP = baseStats[0] + statMods[0] + individualHP;
        if (isElite) HP *= 2;
        if (HP < 1) HP = 0;
        Martial = baseStats[1] + statMods[1] + individualMartial;
        if (isElite) Martial += 2;
        if (Martial < 0) Martial = 0;
        Magic = baseStats[2] + statMods[2] + individualMagic;
        if (isElite) Magic += 2;
        if (Magic < 0) Magic = 0;
        Speed = baseStats[3] + statMods[3] + individualSpeed;
        if (isElite) Speed += 2;
        if (Speed < 0) Speed = 0;
    }

    void RerollFullTitle ()
    {
        fullName = firstName + " " + lastName;
        if (isElite) title = "Elite " + GetSpeciesTerm(species, true) + " " + GetClassTerm(advClass);
        else title = GetSpeciesTerm(species, true) + " " + GetClassTerm(advClass);
        fullTitle = fullName + ", " + title;
    }

    void RerollName()
    {
        if (firstName != "Dwayne")
        {
            firstName = "Dwayne";
            lastName = "Johnson";
        }
        else
        {
            firstName = "Amelia";
            lastName = "Earhart";
        }
    }

    public void Reroll (AdventurerClass _advClass, AdventurerSpecies _species, bool _isElite, int[] individualStats)
    {
        advClass = _advClass;
        species = _species;
        isElite = _isElite;
        individualHP = individualStats[0];
        individualMartial = individualStats[1];
        individualMagic = individualStats[2];
        individualSpeed = individualStats[3];
        RerollName();
        RerollFullTitle();
        RecalcStatsAndReloadMoves();
        initialized = true;
    }

    public void RecalcStatsAndReloadMoves ()
    {
        CalcStats();
        attacks = GetClassAttacks(advClass);
        special = GetClassSpecial(advClass);
    }

    public static string GetAttackName (AdventurerAttack attack)
    {
        string name = "Out of range attack name";
        if (attackNames == null)
        {
            TextAsset a = Resources.Load<TextAsset>("attack_names");
            attackNames = a.text.Split('\n');
        }
        if ((int)attack < attackNames.Length) name = attackNames[(int)attack];
        return name;
    }

    public static string GetSpecialDescription (AdventurerSpecial special)
    {
        string desc = "No special ability";
        TextAsset a = Resources.Load<TextAsset>(specialDescsResourcePath + special.ToString());
        if (a != null) desc = a.text;
        return desc;
    }

    public static string GetSpecialName (AdventurerSpecial special)
    {
        string name = "Out of range special name";
        if (specialNames == null)
        {
            TextAsset a = Resources.Load<TextAsset>("special_names");
            specialNames = a.text.Split('\n');
        }
        if ((int)special < specialNames.Length) name = specialNames[(int)special];
        return name;
    }

    public static AdventurerAttack[] GetClassAttacks (AdventurerClass advClass)
    {
        AdventurerAttack[] attacks = { };
        switch (advClass)
        {
            case AdventurerClass.Warrior:
                attacks = new AdventurerAttack[] { AdventurerAttack.MaceSwing };
                break;
            case AdventurerClass.Bowman:
                attacks = new AdventurerAttack[] { AdventurerAttack.Bowshot, AdventurerAttack.RainOfArrows };
                break;
            case AdventurerClass.Footman:
                attacks = new AdventurerAttack[] { AdventurerAttack.ShieldBlock, AdventurerAttack.SpearThrust };
                break;
            case AdventurerClass.Mystic:
                attacks = new AdventurerAttack[] { AdventurerAttack.Siphon, AdventurerAttack.Haste };
                break;
            case AdventurerClass.Sage:
                attacks = new AdventurerAttack[] { AdventurerAttack.VampiricWinds, AdventurerAttack.BurstOfSpeed };
                break;
            case AdventurerClass.Wizard:
                attacks = new AdventurerAttack[] { AdventurerAttack.Inferno, AdventurerAttack.Lightning };
                break;
            case AdventurerClass.Sovereign:
                attacks = new AdventurerAttack[] { AdventurerAttack.HammerBlow, GameDataManager.Instance.sovereignTactic };
                break;
            case AdventurerClass.Avatar:
                attacks = new AdventurerAttack[] { AdventurerAttack.Rend };
                break;
        }
        return attacks;
    }

    public static AdventurerSpecial GetClassSpecial (AdventurerClass advClass)
    {
        AdventurerSpecial special = AdventurerSpecial.None;
        switch (advClass)
        {
            case AdventurerClass.Bowman:
                special = AdventurerSpecial.SilencingShot;
                break;
            case AdventurerClass.Footman:
                special = AdventurerSpecial.ShieldWall;
                break;
            case AdventurerClass.Sage:
                special = AdventurerSpecial.Barrier;
                break;
            case AdventurerClass.Wizard:
                special = AdventurerSpecial.Feedback;
                break;
            case AdventurerClass.Sovereign:
                special = GameDataManager.Instance.sovereignSkill;
                break;
            case AdventurerClass.Avatar:
                special = AdventurerSpecial.LoseBattle;
                break;
        }
        return special;
    }

    public static int[] GetClassStats (AdventurerClass advClass)
    {
        int[] stats = { 10, 1, 1, 1 };
        switch (advClass)
        {
            case AdventurerClass.Warrior:
                stats = new int[] { 10, 2, 2, 2 };
                break;
            case AdventurerClass.Bowman:
                stats = new int[] { 6, 3, 3, 3 };
                break;
            case AdventurerClass.Footman:
                stats = new int[] { 20, 3, 1, 1 };
                break;
            case AdventurerClass.Mystic:
                stats = new int[] { 8, 1, 3, 2 };
                break;
            case AdventurerClass.Sage:
                stats = new int[] { 12, 2, 3, 1 };
                break;
            case AdventurerClass.Wizard:
                stats = new int[] { 4, 0, 4, 2 };
                break;
            case AdventurerClass.Sovereign:
                stats = new int[] { 40, 4, 4, 3 };
                break;
            case AdventurerClass.Avatar:
                stats = new int[] { 120, 7, 7, 7 };
                break;
        }
        return stats;
    }

    public static string GetClassTerm (AdventurerClass advClass)
    {
        string term = "DEFAULT";
        switch (advClass)
        {
            case AdventurerClass.Warrior:
                term = "Warrior";
                break;
            case AdventurerClass.Bowman:
                term = "Bowman";
                break;
            case AdventurerClass.Footman:
                term = "Footman";
                break;
            case AdventurerClass.Mystic:
                term = "Mystic";
                break;
            case AdventurerClass.Sage:
                term = "Sage";
                break;
            case AdventurerClass.Wizard:
                term = "Wizard";
                break;
            case AdventurerClass.Sovereign:
                term = "Sovereign";
                break;
            case AdventurerClass.Avatar:
                term = "Avatar";
                break;
        }
        return term;
    }

    public static int[] GetSpeciesStatMods (AdventurerSpecies species)
    {
        int[] mods = { 0, 0, 0, 0 };
        switch (species)
        {
            case AdventurerSpecies.Fae:
                mods = new int[] { 0, -1, 2, -1 };
                break;
            case AdventurerSpecies.Orc:
                mods = new int[] { 0, 1, -2, 1};
                break;
            case AdventurerSpecies.Aeon:
                mods = new int[] { 10, 5, 5, 5 };
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
