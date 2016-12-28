﻿using UnityEngine;
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
    Rend,
    _CantMove_Silenced = 10000
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
    private static int[] actionBaseDamages = { 0, 6, 5, 0, 2, 3, 6, 0, 0, 4, 0, 8, 0, 10, 10, 5, 4, 10, 0, 6, 99 };
    private static bool[] actionMagicStatus = { false, false, true, false, false, false, false, false, false, false, false, true, false, true, true, true, false, false, false, false, true };
    public BattleOverseer overseer;
    public BattlerPuppet puppet;
    public Adventurer adventurer;
    public Battler bodyguard;
    public List<Battler> deathblowList;
    public List<BattlerAction> battleStartInterruptActions;
    public List<BattlerAction> onAllyDeathblowInterruptActions;
    public List<BattlerAction> onAllyHitInterruptActions;
    public List<BattlerAction> onHitInterruptActions;
    public BattlerAction defaultAction;
    public AdventurerAttack attack0;
    public AdventurerAttack attack1;
    public bool _underBarrier { get { return barrierTurns > 0; } }
    public bool _underEncore { get { if (isEnemy) return overseer.encoreWaitingForEnemies; else return overseer.encoreWaitingForPlayer; } }
    public bool _underSilence { get { return silentTurns > 0; } }
    public bool _underShieldBlock { get { return shieldBlockTurns > 0; } }
    public bool _underShieldWall { get { return shieldWallTurns > 0; } }
    public bool dead;
    public bool incomingHit;
    public bool isEnemy;
    public float moveSpeed;
    public int currentHP;
    public int lastDamage;
    public bool existsInBattle { get { return (!dead && activeBattler); } }
    public bool isValidTarget { get { return (existsInBattle && !pulledOutOfBattle); } }
    private bool getBehindMeProc;
    private int barrierTurns;
    private int burstOfSpeedCooldown;
    private int feedbackCooldown;
    private int hasteCooldown;
    private int hasteTurns;
    private int shieldBlockTurns;
    private int shieldWallTurns;
    private int silentTurns;
    public bool activeBattler { get; private set; }
    private bool pulledOutOfBattle;
    bool _readiedActionNeedsTarget;
    int _damageDealt = 0;
    int _drainRounds = 0;
    int _damage;
    bool _hitAll;
    bool _isMagic;
    public bool _wantsToReenterBattle { get; private set; }
    public Battler _target { get; private set; }
    Battler _secondaryTarget;
    BattlerAction _action;
    Queue<BattleMessageType> _effectMessages;

    void Start ()
    {
        InitializeDisposables();
        activeBattler = true;
        _effectMessages = new Queue<BattleMessageType>();
	}

    public void Upkeep ()
    {
        TickCooldownsAndShit();
        if (pulledOutOfBattle)
        {
            currentHP += adventurer.HP / 4;
            if (currentHP > adventurer.HP) currentHP = adventurer.HP;
            if (currentHP == adventurer.HP) _wantsToReenterBattle = true;
        }
    }

    public void PullOutOfBattle ()
    {
        pulledOutOfBattle = true;
        overseer.outOfBattleBattler = this;
        puppet.PullOutAnim();
        overseer.messageBox.Step(BattleMessageType.PulledOutOfBattle, this);
    }

    public void ReenterBattle ()
    {
        pulledOutOfBattle = false;
        overseer.outOfBattleBattler = null;
        puppet.ReenterBattleAnim();
        puppet.Respond();
        overseer.messageBox.Step(BattleMessageType.ReenteredBattle, this);
    }

    private Battler AcquireTarget (List<Battler> validEnemyTargets, List<Battler> friends, BattlerAction action, int baseDamage, bool isMagic)
    {
        int damage;
        int offense;
        int defense;
        if (isMagic) offense = adventurer.Magic;
        else offense = adventurer.Martial;
        Battler target = default(Battler); // if it's left null we're doing something that doesn't have a "target" per se
        switch (action)
        {
            case BattlerAction.MaceSwing:
            case BattlerAction.SilencingShot:
            case BattlerAction.Bowshot:
            case BattlerAction.SpearThrust:
            case BattlerAction.Siphon:
            case BattlerAction.VampiricWinds:
            case BattlerAction.Lightning:
            case BattlerAction.HammerBlow:
            case BattlerAction.Rend:
                int mostDamageDealt = 0;
                _readiedActionNeedsTarget = true;
                for (int i = 0; i < validEnemyTargets.Count; i++)
                {
                    if (action == BattlerAction.SilencingShot && validEnemyTargets[i].adventurer.advClass != AdventurerClass.Mystic &&
                        validEnemyTargets[i].adventurer.advClass != AdventurerClass.Sage && validEnemyTargets[i].adventurer.advClass != AdventurerClass.Wizard) continue; // has to be a magic-user
                    if (isMagic) defense = validEnemyTargets[i].adventurer.Magic;
                    else defense = validEnemyTargets[i].adventurer.Martial;
                    damage = CalcDamage(baseDamage, offense, defense);
                    if (damage >= validEnemyTargets[i].currentHP) // take kills if you can
                    {
                        target = validEnemyTargets[i];
                        deathblowList.Add(target);
                        break; // already found the target, don't bother with anything else
                    }
                    else if (validEnemyTargets[i].shieldWallTurns > 0)
                    {
                        target = validEnemyTargets[i]; // shield wall pulls aggro
                        break;
                    }
                    else if (damage > mostDamageDealt || (damage == mostDamageDealt && Random.Range(0, 2) == 0)) target = validEnemyTargets[i];
                }
                break;
            case BattlerAction.Feedback:
                _readiedActionNeedsTarget = true;
                target = validEnemyTargets[Random.Range(0, validEnemyTargets.Count)];
                break;
            case BattlerAction.GetBehindMe: // If using this action, target needs to be the ally we're covering!!!
                _readiedActionNeedsTarget = true;
                target = overseer.currentTurnTarget;
                break;
            case BattlerAction.Flanking:
                _readiedActionNeedsTarget = true;
                target = overseer.standardPriorityActingBattler;
                break;
            case BattlerAction.RainOfArrows:
            case BattlerAction.Inferno:
                _hitAll = true;
                break;
            case BattlerAction.Protect:
                _readiedActionNeedsTarget = true;
                target = overseer.standardPriorityActingBattler;
                break;
        }
        if (_hitAll)
        {
            for (int i = 0; i < validEnemyTargets.Count; i++)
            {
                if (isMagic) defense = validEnemyTargets[i].adventurer.Magic;
                else defense = validEnemyTargets[i].adventurer.Martial;
                damage = CalcDamage(baseDamage, offense, defense);
                if (damage >= validEnemyTargets[i].currentHP) deathblowList.Add(validEnemyTargets[i]);
            }
        }
        if (overseer.standardPriorityActingBattler == this) overseer.currentTurnTarget = target;
        if (target != null && !target.isValidTarget) target = null;
        return target;
    }

    public void ApplyBarrier (int turns = 1)
    {
        if (barrierTurns < turns) barrierTurns = turns;
        puppet.incomingBuff = true;
    }

    public void ApplyHaste (int turns = 1)
    {
        if (hasteTurns < turns) hasteTurns = turns;
        puppet.incomingBuff = true;
    }

    public void ApplyShieldBlock (int turns = 1)
    {
        if (shieldBlockTurns < turns) shieldBlockTurns = turns;
        puppet.incomingBuff = true;
    }

    public void ApplyShieldWall (int turns = 1)
    {
        if (shieldWallTurns < turns) shieldWallTurns = turns;
        puppet.incomingBuff = true;
    }

    public void AttackWith (List<Battler> validEnemyTargets, List<Battler> friends, BattlerAction action)
    {
        ReadyAttack(validEnemyTargets, friends, action);
        ExecuteAttack(validEnemyTargets, friends);
    }

    public void ReadyAttack (List<Battler> validEnemyTargets, List<Battler> friends, BattlerAction action)
    {
        _damage = GetActionBaseDamage(action);
        _isMagic = GetActionMagicStatus(action);
        _target = AcquireTarget(validEnemyTargets, friends, action, _damage, _isMagic);
        _secondaryTarget = default(Battler);
        switch (action)
        {
            case BattlerAction.Siphon:
                _drainRounds = 1;
                break;
            case BattlerAction.VampiricWinds:
                if (validEnemyTargets.Count > 1)
                {
                    while (true)
                    {
                        _secondaryTarget = validEnemyTargets[Random.Range(0, validEnemyTargets.Count)];
                        if (_secondaryTarget != _target) break;
                    }
                }
                _drainRounds = 2;
                break;
        }
        _action = action;
        if (_target != null)
        {
            _target.incomingHit = _target.puppet.incomingHit = true;
            if (_secondaryTarget != null) _secondaryTarget.incomingHit = _secondaryTarget.puppet.incomingHit = true;
        }
        else if (_hitAll)
        {
            for (int i = 0; i < validEnemyTargets.Count; i++) validEnemyTargets[i].incomingHit = validEnemyTargets[i].puppet.incomingHit = true;
        }
    }

    public void ExecuteAttack (List<Battler> validEnemyTargets, List<Battler> friends)
    {
        if (silentTurns > 0)
        {
            silentTurns--;
            if (_isMagic) _action = BattlerAction._CantMove_Silenced;
        }
        if (_readiedActionNeedsTarget && (_target == null || !_target.isValidTarget))
        {
            AcquireTarget(validEnemyTargets, friends, _action, GetActionBaseDamage(_action), _isMagic);
            if (_target == null) _action = BattlerAction.None; // can't reacquire target - can't do anything
        }
        if (_action == BattlerAction._CantMove_Silenced)
        {
            overseer.messageBox.Step(BattleMessageType.FailedCast, this);
        }
        else if (_action != BattlerAction.None)
        {
            int offense;
            if (_isMagic) offense = adventurer.Magic;
            else offense = adventurer.Martial;
            int defense;
            switch (_action)
            {
                case BattlerAction.SilencingShot:
                    _target.Silence();
                    break;
                case BattlerAction.ShieldWall:
                    ApplyShieldWall(1);
                    _effectMessages.Enqueue(BattleMessageType.ShieldWall);
                    break;
                case BattlerAction.ShieldBlock:
                    ApplyShieldBlock(1);
                    _effectMessages.Enqueue(BattleMessageType.ShieldBlock);
                    break;
                case BattlerAction.Haste:
                    hasteCooldown = 1;
                    for (int i = 0; i < friends.Count; i++) friends[i].ApplyHaste();
                    _effectMessages.Enqueue(BattleMessageType.Haste);
                    break;
                case BattlerAction.Barrier:
                    for (int i = 0; i < friends.Count; i++) friends[i].ApplyBarrier();
                    _effectMessages.Enqueue(BattleMessageType.Barrier);
                    break;
                case BattlerAction.BurstOfSpeed:
                    burstOfSpeedCooldown = 3;
                    overseer.GiveEncore(isEnemy);
                    _effectMessages.Enqueue(BattleMessageType.Encore);
                    break;
                case BattlerAction.Feedback:
                    feedbackCooldown = 3;
                    _effectMessages.Enqueue(BattleMessageType.Feedback);
                    break;
                case BattlerAction.GetBehindMe: // If using this action, target needs to be the ally we're covering!!!
                    _target.Cover(this);
                    getBehindMeProc = true;
                    break;
            }
            if (_damage > 0)
            {
                if (adventurer.special == AdventurerSpecial.HammerSmash && Random.Range(0, 11) < 3)
                {
                    _damage *= 2;
                    _effectMessages.Enqueue(BattleMessageType.Critical);
                }
                if (_hitAll) for (int i = 0; i < validEnemyTargets.Count; i++)
                    {
                        if (_isMagic) defense = validEnemyTargets[i].adventurer.Magic;
                        else defense = validEnemyTargets[i].adventurer.Martial;
                        _damageDealt += validEnemyTargets[i].DealDamage(CalcDamage(_damage, offense, defense));
                    }
                else if (_target != null)
                {
                    if (_isMagic) defense = _target.adventurer.Magic;
                    else defense = _target.adventurer.Martial;
                    _damageDealt += _target.DealDamage(CalcDamage(_damage, offense, defense));
                    if (_secondaryTarget != null)
                    {
                        if (_isMagic) defense = _secondaryTarget.adventurer.Magic;
                        else defense = _secondaryTarget.adventurer.Martial;
                        _damageDealt += _secondaryTarget.DealDamage(CalcDamage(_damage, offense, defense));
                    }
                }
                else throw new System.Exception("Battler " + gameObject.name + " tried to use a damage-dealing action, but with no target.");
            }
            if (_secondaryTarget != null && !_secondaryTarget.isValidTarget)
            {
                _secondaryTarget = null;
                _drainRounds = 0;
            }
            if (_drainRounds > 1) _effectMessages.Enqueue(BattleMessageType.MultiHeal);
            else if (_drainRounds > 1) _effectMessages.Enqueue(BattleMessageType.Heal);
            while (_drainRounds > 0 && _damageDealt > 0)
            {
                int damageTaken = 0;
                int healValue = -_damageDealt / 2;
                if (healValue > -1) healValue = -1; // have to heal __something__
                Battler weakestAlly = this;
                for (int i = 0; i < friends.Count; i++)
                {
                    if (friends[i].adventurer.HP - friends[i].currentHP > damageTaken)
                    {
                        damageTaken = friends[i].adventurer.HP - friends[i].currentHP;
                        weakestAlly = friends[i];
                    }
                }
                weakestAlly.DealDamage(healValue);
                weakestAlly.puppet.incomingBuff = true;
                _drainRounds--; // this loop caused the battle to hang for an embarrassingly long time because I forgot the decrement, lol
            }
            overseer.messageBox.Step(BattleMessageType.StandardTurnMessage, this, _action);
            while (_effectMessages.Count > 0) overseer.messageBox.Step(_effectMessages.Dequeue());
        }

    }

    public static int CalcDamage (int damage, int offense, int defense)
    {
        float rawDamage = damage;
        int mod = offense - defense;
        rawDamage += (rawDamage * .25f * mod);
        damage = Mathf.RoundToInt(rawDamage);
        if (damage < 0) damage = 0;
        return damage;
    }

    public void Cover (Battler _bodyguard)
    {
        bodyguard = _bodyguard;
    }

    public int DealDamage (int damage)
    {
        if (damage > 0)
        {
            if (bodyguard != null)
            {
                bodyguard.DealDamage(damage / 4);
                damage = 0;
                puppet.incomingBuff = true;
                overseer.messageBox.Step(BattleMessageType.SavedAlly);
                bodyguard = default(Battler); // IF YOU NULL BODYGUARD AND THEN TRY TO DAMAGE BODYGUARD NO SHIT IT'S GONNA CRASH
            }
            if (barrierTurns > 0)
            {
                damage /= 2;
            }
            if (shieldWallTurns > 0)
            {
                damage /= 2;
            }
            if (shieldBlockTurns > 0)
            {
                damage -= 5;
                shieldBlockTurns--;
            }
            if (damage < 0) damage = 0;
        }
        currentHP -= damage;
        if (currentHP > adventurer.HP) currentHP = adventurer.HP;
        else if (currentHP <= 0)
        {
            Die();
        }
        lastDamage = damage;
        return damage;
    }

    public void Die ()
    {
        currentHP = 0;
        if (!isEnemy)
        {
            overseer.playerDeaths++;
            overseer.lastDeadPlayerAdvName = adventurer.fullName;
            if (adventurer.advClass != AdventurerClass.Sovereign) adventurer.Permadeath();
        }
        dead = true;
    }

    public BattlerAction GetAction (int turn, List<Battler> allies, List<Battler> opponents)
    {
        BattlerAction action = BattlerAction.None;
        if (!pulledOutOfBattle)
        {
            bool spellcasterInEnemyParty = false;
            for (int i = 0; i < opponents.Count; i++)
            {
                if (opponents[i].adventurer.advClass == AdventurerClass.Mystic || opponents[i].adventurer.advClass == AdventurerClass.Sage || opponents[i].adventurer.advClass == AdventurerClass.Wizard)
                {
                    spellcasterInEnemyParty = true;
                    break;
                }
            }
            if (adventurer.special == AdventurerSpecial.SilencingShot && turn == 0 && spellcasterInEnemyParty) action = BattlerAction.SilencingShot;
            else if (HasAttack(AdventurerAttack.RainOfArrows) && opponents.Count > 2) action = BattlerAction.RainOfArrows;
            else if (HasAttack(AdventurerAttack.ShieldBlock) && shieldBlockTurns < 1) action = BattlerAction.ShieldBlock;
            else if (HasAttack(AdventurerAttack.Haste) && hasteCooldown < 1) action = BattlerAction.Haste;
            else if (HasAttack(AdventurerAttack.BurstOfSpeed) && burstOfSpeedCooldown < 1 && turn > 0) action = BattlerAction.BurstOfSpeed;
            else if (HasAttack(AdventurerAttack.Inferno) && opponents.Count > 1) action = BattlerAction.Inferno;
            else action = defaultAction;
        }
        _action = action;
        return action;
    }


    public BattlerAction GetInterruptAction (BattlerActionInterruptType interrupt)
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
                if (onAllyHitInterruptActions.Count > 0) action = onAllyHitInterruptActions[0]; // COUNT > 1, NO SHIT, OF COURSE IT WASN'T RETURNING AN ACTION
                break;
            case BattlerActionInterruptType.OnHit:
                if (onHitInterruptActions.Count > 0) action = onHitInterruptActions[0];
                break;
        }
        switch (action)
        {
            case BattlerAction.Barrier:
                if (barrierTurns > 0) action = BattlerAction.None; // don't barrier if we've already barriered our barrier
                break;
        }
        
        return action;
    }

    public static int GetActionBaseDamage (BattlerAction action)
    {
        int v;
        if (action == BattlerAction._CantMove_Silenced) v = 0;
        else v = actionBaseDamages[(int)action];
        return v;
    }

    public static bool GetActionMagicStatus (BattlerAction action)
    {
        bool v;
        if (action == BattlerAction._CantMove_Silenced) v = false;
        else v = actionMagicStatus[(int)action];
        return v;
    }

    public void GenerateBattleData (Adventurer _adventurer)
    {
        SetBattlerActive();
        dead = false;
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
            RegisterAttack(adventurer.attacks[i]);
            if (defaultAction == BattlerAction.None) switch (adventurer.attacks[i])
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
        }
        if (adventurer.special == AdventurerSpecial.ShieldWall) battleStartInterruptActions.Add(BattlerAction.ShieldWall);
        else if (adventurer.special == AdventurerSpecial.Barrier) battleStartInterruptActions.Add(BattlerAction.Barrier);
        else if (adventurer.special == AdventurerSpecial.Feedback) onHitInterruptActions.Add(BattlerAction.Feedback);
        else if (adventurer.special == AdventurerSpecial.Protect) onAllyHitInterruptActions.Add(BattlerAction.Protect);
        if (HasAttack(AdventurerAttack.GetBehindMe)) onAllyDeathblowInterruptActions.Add(BattlerAction.GetBehindMe);
        if (HasAttack(AdventurerAttack.Flanking)) onAllyDeathblowInterruptActions.Add(BattlerAction.Flanking);
        puppet.Setup();
    }

    private void RegisterAttack (AdventurerAttack attack)
    {
        if (attack0 == AdventurerAttack.None) attack0 = attack;
        else attack1 = attack;
    }

    public void TickCooldownsAndShit ()
    {
        if (barrierTurns > 0) barrierTurns--;
        if (burstOfSpeedCooldown > 0) burstOfSpeedCooldown--;
        if (feedbackCooldown > 0) feedbackCooldown--;
        if (hasteCooldown > 0) hasteCooldown--;
        if (hasteTurns > 0) hasteTurns--;
        if (shieldWallTurns > 0) shieldWallTurns--;
        InitializeDisposables();
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
        activeBattler = true;
        puppet.AliveDisplay();
    }

    public void SetBattlerInactive ()
    {
        activeBattler = false;
        puppet.KillDisplay();
    }

    public void Silence (int turns = 1)
    {
        if (silentTurns < turns) silentTurns = turns;
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
        feedbackCooldown = 0;
        hasteCooldown = 0;
        hasteTurns = 0;
        shieldBlockTurns = 0;
        shieldWallTurns = 0;
        silentTurns = 0;
        InitializeDisposables();
    }

    private void InitializeDisposables ()
    {
        bodyguard = default(Battler);
        incomingHit = false;
        _damageDealt = 0;
        _wantsToReenterBattle = false;
        _readiedActionNeedsTarget = false;
        _drainRounds = 0;
        _damage = 0;
        _hitAll = false;
        _isMagic = false;
        _target = default(Battler);
        _secondaryTarget = default(Battler);
        _action = BattlerAction.None;
        if (deathblowList.Count > 0) deathblowList = new List<Battler>(); // This is gross, but it's controlled grossness, because either it happens rarely or you're throwing these away every turn and the adventure is just gonna be 12 turns anyway.
    }
}
