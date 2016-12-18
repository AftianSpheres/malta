using UnityEngine;
using System.Collections;

public enum AdventurerMugshot
{
    None,
    Sovereign0,
    Sovereign1,
    Sovereign2,
    Sovereign3,
    Sovereign4,
    Sovereign5,
    Sovereign6,
    Sovereign7,
    Human0,
    Human1,
    Human2,
    Human3,
    Human4,
    Human5,
    Human6,
    Human7,
    Fae0,
    Fae1,
    Fae2,
    Fae3,
    Fae4,
    Fae5,
    Fae6,
    Fae7,
    Orc0,
    Orc1,
    Orc2,
    Orc3,
    Orc4,
    Orc5,
    Orc6,
    Orc7,
    Aeon
}

public class Adventurer : ScriptableObject
{
    public string firstName;
    public string lastName;
    public string fullName;
    public string title;
    public string fullTitle;
    public AdventurerAttack[] attacks;
    public AdventurerClass advClass { get; private set; }
    public AdventurerSpecial special;
    public AdventurerSpecies species = AdventurerSpecies.Human;
    public AdventurerMugshot mugshot { get; private set; }
    public int HP { get; private set; }
    public int individualHP { get; private set; }
    public int Martial { get; private set; }
    public int individualMartial { get; private set; }
    public int Magic { get; private set; }
    public int individualMagic { get; private set; }
    public int Speed { get; private set; }
    public int individualSpeed { get; private set; }
    public bool isElite { get; private set; }
    public bool initialized { get; private set; }
    private static string[] attackNames;
    private static string[] classNames;
    private static string[] specialNames;
    private static string[] humanFirstNames;
    private static string[] humanLastNames;
    private static string[] faeFirstNames;
    private static string[] faeLastNames;
    private static string[] orcFirstNames;
    private static string[] orcLastNames;
    private static string[] aeonFirstNames = { "Dwayne" };
    private static string[] aeonLastNames = { "Johnson" };
    private const string attackDescsResourcePath = "attack_descs/";
    private const string specialDescsResourcePath = "special_descs/";
    private const string mugshotsResourcePath = "mugshots/";

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

    void RerollMugshot ()
    {
        if (advClass == AdventurerClass.Sovereign) mugshot = GameDataManager.Instance.sovereignMugshot;
        else
        {
            switch (species)
            {
                case AdventurerSpecies.Human:
                    mugshot = (AdventurerMugshot)Random.Range((int)AdventurerMugshot.Human0, (int)AdventurerMugshot.Human7 + 1);
                    break;
                case AdventurerSpecies.Fae:
                    mugshot = (AdventurerMugshot)Random.Range((int)AdventurerMugshot.Fae0, (int)AdventurerMugshot.Fae7 + 1);
                    break;
                case AdventurerSpecies.Orc:
                    mugshot = (AdventurerMugshot)Random.Range((int)AdventurerMugshot.Orc0, (int)AdventurerMugshot.Orc7 + 1);
                    break;
                case AdventurerSpecies.Aeon:
                    mugshot = AdventurerMugshot.Aeon;
                    break;
            }
        }
    }

    void RerollFullTitle ()
    {
        if (isElite) title = "Elite " + GetSpeciesTerm(species, true) + " " + GetClassName(advClass);
        else title = GetSpeciesTerm(species, true) + " " + GetClassName(advClass);
        fullTitle = fullName + ", " + title;
    }

    void RerollName()
    {
        if (advClass == AdventurerClass.Sovereign)
        {
            firstName = GameDataManager.Instance.sovereignFirstName;
            lastName = GameDataManager.Instance.sovereignLastName;
        }
        else
        {
            string[] firstNames;
            string[] lastNames;
            switch (species)
            {
                case AdventurerSpecies.Human:
                    if (humanFirstNames == null) humanFirstNames = Resources.Load<TextAsset>("first_names_human").text.Split('\n');
                    if (humanLastNames == null) humanLastNames = Resources.Load<TextAsset>("last_names_human").text.Split('\n');
                    firstNames = humanFirstNames;
                    lastNames = humanLastNames;
                    break;
                case AdventurerSpecies.Fae:
                    if (faeFirstNames == null) faeFirstNames = Resources.Load<TextAsset>("first_names_fae").text.Split('\n');
                    if (faeLastNames == null) faeLastNames = Resources.Load<TextAsset>("last_names_fae").text.Split('\n');
                    firstNames = faeFirstNames;
                    lastNames = faeLastNames;
                    break;
                case AdventurerSpecies.Orc:
                    if (orcFirstNames == null) orcFirstNames = Resources.Load<TextAsset>("first_names_orc").text.Split('\n');
                    if (orcLastNames == null) orcLastNames = Resources.Load<TextAsset>("last_names_orc").text.Split('\n');
                    firstNames = orcFirstNames;
                    lastNames = orcLastNames;
                    break;
                default:
                    firstNames = aeonFirstNames;
                    lastNames = aeonLastNames;
                    break;
            }
            firstName = firstNames[Random.Range(0, firstNames.Length)];
            lastName = lastNames[Random.Range(0, lastNames.Length)];
        }
        fullName = firstName + " " + lastName;
    }

    public void Promote ()
    {
        isElite = true;
        RerollFullTitle();
        RecalcStatsAndReloadMoves();
    }

    public void Reclass (AdventurerClass _advClass)
    {
        advClass = _advClass;
        RerollFullTitle();
        RecalcStatsAndReloadMoves();
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
        RerollMugshot();
        initialized = true;
    }

    public void RecalcStatsAndReloadMoves ()
    {
        CalcStats();
        attacks = GetClassAttacks(advClass);
        special = GetClassSpecial(advClass);
    }

    public static Sprite GetMugshot (AdventurerMugshot mugshot)
    {
        Sprite mugSprite = Resources.Load<Sprite>(mugshotsResourcePath + mugshot.ToString());
        return mugSprite;
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

    public static string GetAttackDescription(AdventurerAttack attack)
    {
        string desc = "None";
        TextAsset a = Resources.Load<TextAsset>(attackDescsResourcePath + attack.ToString());
        if (a != null) desc = a.text;
        return desc;
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
                attacks = new AdventurerAttack[] { AdventurerAttack.MaceSwing, AdventurerAttack.None };
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
                attacks = new AdventurerAttack[] { AdventurerAttack.Rend, AdventurerAttack.None };
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

    public static string GetClassName (AdventurerClass advClass)
    {
        string name = "Out of range class name";
        if (classNames == null)
        {
            TextAsset a = Resources.Load<TextAsset>("class_names");
            classNames = a.text.Split('\n');
        }
        if ((int)advClass < classNames.Length) name = classNames[(int)advClass];
        return name;
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

    public void Permadeath ()
    {
        initialized = false;
    }
}
