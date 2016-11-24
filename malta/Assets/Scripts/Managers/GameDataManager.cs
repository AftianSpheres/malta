using UnityEngine;
using System.Collections;

public class GameDataManager : Manager<GameDataManager>
{
    public Adventurer sovereignAdventurer;
    public AdventurerAttack sovereignTactic;
    public AdventurerSpecial sovereignSkill;
    public int resBricks;
    public int resClay;
    public int resLumber;
    public int resMetal;
    public int resOre;
    public int resPlanks;

    void Start()
    {
        sovereignTactic = AdventurerAttack.GetBehindMe;
        sovereignSkill = AdventurerSpecial.Protect;
        sovereignAdventurer = ScriptableObject.CreateInstance<Adventurer>();
        sovereignAdventurer.Reroll(AdventurerClass.Sovereign, AdventurerSpecies.Human, false, new int[] { 0, 0, 0, 0 });
    }
}
