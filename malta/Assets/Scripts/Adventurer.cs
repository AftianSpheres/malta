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

[System.Serializable]
public class Adventurer
{
    public string firstName;
    public string lastName;
    public string fullName;
    public string title;
    public string fullTitle;
    public string bioText { get; private set; }
    public BattlerAction[] attacks;
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
    public bool awakened { get; private set; }
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
    private static string[] bioAdjectives;
    private static string[] bioAnecdotes;
    private static string[] bioHints;
    private static string[] bioLikes;
    private static string[] bioSpecies;
    private const string bioAdjectivesResourcePath = "bio/0_adjective";
    private const string bioAnecdotesResourcePath = "bio/1_anecdote";
    private const string bioHintsResourcePath = "bio/0_hint";
    private const string bioLikesResourcePath = "bio/2_like";
    private const string bioSpeciesResourcePath = "bio/0_species";
    private const string attackDescsResourcePath = "attack_descs/";
    private const string specialDescsResourcePath = "special_descs/";
    private const string mugshotsResourcePath = "mugshots/";
    private const string enemyGfxResourcePath = "mugshots/enemy/";
    public static int[] awakeningCosts = { 50, 50, 50 };
    private static AdventurerClass[] frontRowClasses = { AdventurerClass.Warrior, AdventurerClass.Footman, AdventurerClass.Sovereign };

    public void Awaken ()
    {
        if (awakened) throw new System.Exception(fullTitle + " is already awakened!");
        int[] a = GetRandomStatPoint();
        individualHP += a[0];
        individualMartial += a[1];
        individualMagic += a[2];
        individualSpeed += a[3];
        CalcStats();
        awakened = true;
    }

    void GenerateBioText ()
    {
        if (bioAdjectives == null) LoadBioTextStrings();
        string line0 = "";
        string line1 = "";
        string line2 = "";
        int index;
        int lastIndex;
        index = Random.Range(0, bioAdjectives.Length);
        line0 += bioAdjectives[index];
        index = (int)species - 1;
        line0 += bioSpecies[index];
        index = Random.Range(0, 3);
        if (individualMagic > 0) index += 3;
        else if (individualSpeed > 0) index += 6;
        line0 += bioHints[index];
        line0 += System.Environment.NewLine;
        index = Random.Range(0, bioAnecdotes.Length);
        lastIndex = index;
        line1 += bioAnecdotes[index] + ' ';
        while (index == lastIndex) index = Random.Range(0, bioAnecdotes.Length);
        line1 += bioAnecdotes[index];
        line1 += System.Environment.NewLine;
        index = Random.Range(0, bioLikes.Length - 2);
        lastIndex = index;
        line2 += bioLikes[0] + bioLikes[index + 2] + ' ';
        while (index == lastIndex) index = Random.Range(0, bioLikes.Length - 2);
        line2 += bioLikes[1] + bioLikes[index + 2];
        bioText = line0 + line1 + line2;
    }

    public void PushWpnToSovereign ()
    {
        if (GameDataManager.Instance.dataStore.sovereignAdventurer != this) throw new System.Exception("Tried to update an adventurer that's not the Sovereign with weapon data!");
        SovereignWpn w = GameDataManager.Instance.dataStore.sovWpn_Set;
        attacks = new BattlerAction[w.attacks.Length + 1];
        w.attacks.CopyTo(attacks, 0);
        attacks[attacks.Length - 1] = GameDataManager.Instance.dataStore.sovereignTactic;
        HP = w.HP;
        Martial = w.Martial;
        Magic = w.Magic;
        Speed = w.Speed;
    }

    void LoadBioTextStrings ()
    {
        bioAdjectives = Resources.Load<TextAsset>(bioAdjectivesResourcePath).text.Split('\n');
        bioAnecdotes = Resources.Load<TextAsset>(bioAnecdotesResourcePath).text.Split('\n');
        bioHints = Resources.Load<TextAsset>(bioHintsResourcePath).text.Split('\n');
        bioLikes = Resources.Load<TextAsset>(bioLikesResourcePath).text.Split('\n');
        bioSpecies = Resources.Load<TextAsset>(bioSpeciesResourcePath).text.Split('\n');
    }

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
        if (advClass == AdventurerClass.Sovereign) mugshot = GameDataManager.Instance.dataStore.sovereignMugshot;
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
            firstName = GameDataManager.Instance.dataStore.sovereignFirstName;
            lastName = GameDataManager.Instance.dataStore.sovereignLastName;
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
        GenerateBioText();
        initialized = true;
        awakened = false;
    }

    public void RecalcStatsAndReloadMoves ()
    {
        CalcStats();
        attacks = GetClassAttacks(advClass);
        special = GetClassSpecial(advClass);
    }

    public static int[] GetRandomStatPoint ()
    {
        int[] r;
        int randomInt = Random.Range(0, 3);
        if (randomInt == 0) r = new int[] { 0, 1, 0, 0 };
        else if (randomInt == 1) r = new int[] { 0, 0, 1, 0 };
        else r = new int[] { 0, 0, 0, 1 };
        return r;
    }

    public static string GetAttackName (BattlerAction attack)
    {
        return BattlerActionData.get(attack).name;
    }

    public static string GetAttackDescription(BattlerAction attack)
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

    public static BattlerAction[] GetClassAttacks (AdventurerClass advClass)
    {
        BattlerAction[] attacks = { };
        switch (advClass)
        {
            case AdventurerClass.Warrior:
                attacks = new BattlerAction[] { BattlerAction.MaceSwing, BattlerAction.None };
                break;
            case AdventurerClass.Bowman:
                attacks = new BattlerAction[] { BattlerAction.Bowshot, BattlerAction.RainOfArrows };
                break;
            case AdventurerClass.Footman:
                attacks = new BattlerAction[] { BattlerAction.ShieldBlock, BattlerAction.SpearThrust };
                break;
            case AdventurerClass.Mystic:
                attacks = new BattlerAction[] { BattlerAction.Siphon, BattlerAction.Haste };
                break;
            case AdventurerClass.Sage:
                attacks = new BattlerAction[] { BattlerAction.VampiricWinds, BattlerAction.BurstOfSpeed };
                break;
            case AdventurerClass.Wizard:
                attacks = new BattlerAction[] { BattlerAction.Inferno, BattlerAction.Lightning };
                break;
            case AdventurerClass.Sovereign:
                attacks = new BattlerAction[] { BattlerAction.HammerBlow, GameDataManager.Instance.dataStore.sovereignTactic };
                break;
            case AdventurerClass.Avatar:
                attacks = new BattlerAction[] { BattlerAction.Rend, BattlerAction.None };
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
                special = GameDataManager.Instance.dataStore.sovereignSkill;
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

    public static bool ClassIsFrontRowClass (AdventurerClass advClass)
    {
        bool r = false;
        for (int i = 0; i < frontRowClasses.Length; i++)
        {
            if (frontRowClasses[i] == advClass)
            {
                r = true;
                break;
            }
        }
        return r;
    }

    public void Permadeath ()
    {
        initialized = false;
    }

    public Sprite GetEnemyGraphic ()
    {
        Sprite s = default(Sprite);
        string p = enemyGfxResourcePath + species.ToString() + advClass.ToString();
        s = Resources.Load<Sprite>(p);
        if (s == null) throw new System.Exception("No enemy graphic for " + p);
        return s;
    }

    public Sprite GetMugshotGraphic()
    {
        Sprite mugSprite;
        if (mugshot == AdventurerMugshot.None) mugSprite = null;
        else
        {
            mugSprite = Resources.Load<Sprite>(mugshotsResourcePath + mugshot.ToString());
            if (mugSprite == null) throw new System.Exception("No mugshot for " + mugshotsResourcePath + mugshot.ToString());
        }
        return mugSprite;
    }
}
