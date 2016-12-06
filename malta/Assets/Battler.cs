using UnityEngine;
using System.Collections.Generic;

public enum BattlerAction
{
    None,
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
    Rend
}

public enum BattlerActionInterruptType
{
    None,
    BattleStart,
    OnAllyDeathblow,
    OnAllyHit,
    OnHit
}

public class Battler : MonoBehaviour
{
    public Adventurer adventurer;
    public List<BattlerAction> battleStartInterruptActions;
    public List<BattlerAction> onAllyDeathblowInterruptActions;
    public List<BattlerAction> onAllyHitInterruptActions;
    public List<BattlerAction> onHitInterruptActions;
    public BattlerAction defaultAction;
    public AdventurerAttack attack0;
    public AdventurerAttack attack1;
    public bool dead;
    public float moveSpeed;
    public int currentHP;
    private bool getBehindMeProc;
    private int barrierTurns;
    private int burstOfSpeedCooldown;
    private int hasteCooldown;
    private int hasteTurns;
    private int shieldBlockTurns;
    private int shieldWallTurns;
    private int silentTurns;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void DealDamage (int damage)
    {
        if (damage > 0)
        {
            if (barrierTurns > 0) damage /= 2;
            if (shieldWallTurns > 0) damage /= 2;
            if (shieldBlockTurns > 0) damage -= 5;
            if (damage < 0) damage = 0;
        }
        currentHP -= damage;
        if (currentHP > adventurer.HP) currentHP = adventurer.HP;
        else if (currentHP < 0)
        {
            currentHP = 0;
            dead = true;
        }
    }

    public BattlerAction GetAction (int turn, int liveEnemiesCount, Battler[] allies, Battler[] opponents)
    {
        BattlerAction action = BattlerAction.None;
        bool spellcasterInEnemyParty = false;
        for (int i = 0; i < opponents.Length; i++)
        {
            if (opponents[i].adventurer.advClass == AdventurerClass.Mystic || opponents[i].adventurer.advClass == AdventurerClass.Sage || opponents[i].adventurer.advClass == AdventurerClass.Wizard)
            {
                spellcasterInEnemyParty = true;
                break;
            }
        }
        if (adventurer.special == AdventurerSpecial.SilencingShot && turn == 0 && spellcasterInEnemyParty) action = BattlerAction.SilencingShot;
        else if (HasAttack(AdventurerAttack.RainOfArrows) && liveEnemiesCount > 2) action = BattlerAction.RainOfArrows;
        else if (HasAttack(AdventurerAttack.ShieldBlock) && shieldBlockTurns < 1) action = BattlerAction.ShieldBlock;
        else if (HasAttack(AdventurerAttack.Haste) && hasteCooldown < 1) action = BattlerAction.Haste;
        else if (HasAttack(AdventurerAttack.BurstOfSpeed) && burstOfSpeedCooldown < 1 && turn > 0) action = BattlerAction.BurstOfSpeed;
        else if (HasAttack(AdventurerAttack.Inferno) && liveEnemiesCount > 1) action = BattlerAction.Inferno;
        else action = defaultAction;
        return action;
    }

    public BattlerAction GetInterruptAction (BattlerActionInterruptType interrupt, int turn, int liveEnemiesCount, Battler[] allies, Battler[] opponents)
    {
        BattlerAction action = BattlerAction.None;
        switch (interrupt)
        {
            case BattlerActionInterruptType.BattleStart:
                if (battleStartInterruptActions.Count > 0) action = battleStartInterruptActions[0]; // nothing in this class right now needs us to be smart about how we handle this case, but the list lets AI do something if needed in future
                break;
            case BattlerActionInterruptType.OnAllyDeathblow:
                if (onAllyDeathblowInterruptActions.Count > 0)
                {
                    if (!getBehindMeProc || onAllyDeathblowInterruptActions[0] != BattlerAction.GetBehindMe) action = onAllyDeathblowInterruptActions[0];
                    else if (onAllyDeathblowInterruptActions.Count > 1) action = onAllyDeathblowInterruptActions[1]; // this should never happen, but if it does we should handle it properly
                }
                break;
            case BattlerActionInterruptType.OnAllyHit:
                if (onAllyHitInterruptActions.Count > 1) action = onAllyHitInterruptActions[0];
                break;
            case BattlerActionInterruptType.OnHit:
                if (onHitInterruptActions.Count > 1) action = onHitInterruptActions[0];
                break;
        }
        return action;
    }

    public void GenerateBattleData (Adventurer _adventurer)
    {
        defaultAction = BattlerAction.None;
        battleStartInterruptActions = new List<BattlerAction>();
        onAllyHitInterruptActions = new List<BattlerAction>();
        onAllyDeathblowInterruptActions = new List<BattlerAction>();
        onHitInterruptActions = new List<BattlerAction>();
        adventurer = _adventurer;
        currentHP = adventurer.HP;
        RefreshCooldownsAndShit();
        for (int i = 0; i < adventurer.attacks.Length; i++)
        {
            switch (adventurer.attacks[i])
            {
                case AdventurerAttack.MaceSwing:
                    defaultAction = BattlerAction.MaceSwing;
                    break;
                case AdventurerAttack.Bowshot:
                    defaultAction = BattlerAction.Bowshot;
                    break;
                case AdventurerAttack.SpearThrust:
                    defaultAction = BattlerAction.SpearThrust;
                    break;
                case AdventurerAttack.Siphon:
                    defaultAction = BattlerAction.Siphon;
                    break;
                case AdventurerAttack.VampiricWinds:
                    defaultAction = BattlerAction.VampiricWinds;
                    break;
                case AdventurerAttack.Lightning:
                    defaultAction = BattlerAction.Lightning;
                    break;
                case AdventurerAttack.HammerBlow:
                    defaultAction = BattlerAction.HammerBlow;
                    break;
                case AdventurerAttack.Rend:
                    defaultAction = BattlerAction.Rend;
                    break;
            }
            if (defaultAction != BattlerAction.None) break;
        }
        if (adventurer.special == AdventurerSpecial.ShieldWall) battleStartInterruptActions.Add(BattlerAction.ShieldWall);
        else if (adventurer.special == AdventurerSpecial.Barrier) battleStartInterruptActions.Add(BattlerAction.Barrier);
        else if (adventurer.special == AdventurerSpecial.Feedback) onHitInterruptActions.Add(BattlerAction.Feedback);
        else if (adventurer.special == AdventurerSpecial.Protect) onAllyHitInterruptActions.Add(BattlerAction.Protect);
        if (HasAttack(AdventurerAttack.GetBehindMe)) onAllyDeathblowInterruptActions.Add(BattlerAction.GetBehindMe);
        if (HasAttack(AdventurerAttack.Flanking)) onAllyDeathblowInterruptActions.Add(BattlerAction.Flanking);
    }

    public void TickCooldownsAndShit ()
    {
        if (barrierTurns > 0) barrierTurns--;
        if (burstOfSpeedCooldown > 0) burstOfSpeedCooldown--;
        if (hasteCooldown > 0) hasteCooldown--;
        if (hasteTurns > 0) hasteTurns--;
        if (shieldBlockTurns > 0) shieldBlockTurns--;
        if (shieldWallTurns > 0) shieldWallTurns--;
        if (silentTurns > 0) silentTurns--;
    }

    public void RollMoveSpeed ()
    {
        float speed = adventurer.Speed;
        if (hasteTurns > 0) speed += 3;
        if (speed < 1) speed = 1;
        moveSpeed = Random.Range(1.0f, speed);
    }

    public void SetBattlerActive ()
    {
        gameObject.SetActive(true);
    }

    public void SetBattlerInactive()
    {
        gameObject.SetActive(false);
    }

    private bool HasAttack (AdventurerAttack attack)
    {
        return attack0 == attack || attack1 == attack;
    }

    private void RefreshCooldownsAndShit ()
    {
        getBehindMeProc = false;
        barrierTurns = 0;
        burstOfSpeedCooldown = 0;
        hasteCooldown = 0;
        hasteTurns = 0;
        shieldBlockTurns = 0;
        shieldWallTurns = 0;
        silentTurns = 0;
    }
}
