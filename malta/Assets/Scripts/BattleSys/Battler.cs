using UnityEngine;
using System.Collections.Generic;

public class Battler : MonoBehaviour
{
    public BattleOverseer overseer;
    public BattlerPuppet puppet;
    public Adventurer adventurer;
    public Battler bodyguard;
    public List<Battler> deathblowList;
    public List<BattlerAction> battleStartInterruptActions;
    private int[] _cooldowns_BSIA;
    public List<BattlerAction> onAllyDeathblowInterruptActions;
    private int[] _cooldowns_OADIA;
    public List<BattlerAction> onAllyHitInterruptActions;
    private int[] _cooldowns_OAHIA;
    public List<BattlerAction> onHitInterruptActions;
    private int[] _cooldowns_OHIA;
    public List<BattlerAction> standardBracketActions;
    private int[] _cooldowns_SBA;
    public BattlerAction defaultAction { get { return standardBracketActions[0]; } }
    public bool _underBarrier { get { return barrierTurns > 0; } }
    public bool _underEncore { get { if (isEnemy) return overseer.encoreWaitingForEnemies; else return overseer.encoreWaitingForPlayer; } }
    public bool _underSilence { get { return silentTurns > 0; } }
    public bool _underShieldBlock { get { return shieldBlockTurns > 0; } }
    public bool _underShieldWall { get { return shieldWallTurns > 0; } }
    private bool _breachOwnLines;
    private bool _breachFoeLines;
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
    private int shieldBlockTurns;
    private int shieldWallTurns;
    private int silentTurns;
    private int _bonusMAR;
    private int _bonusMAR_turns;
    private int _bonusMAG;
    private int _bonusMAG_turns;
    private int _bonusSPE;
    private int _bonusSPE_turns;
    private int _sacrificePow;
    private int _sacrificeTurns;
    private int drainBlockTurns;
    private int stunBuffTurns;
    private int stunnedTurns;
    private int dodgeBuffTurns;
    public int MAR { get { return adventurer.Martial + _bonusMAR + _sacrificePow; } }
    public int MAG { get { return adventurer.Magic + _bonusMAG + _sacrificePow; } }
    public int SPE { get { return adventurer.Speed + _bonusSPE + _sacrificePow; } }
    public bool activeBattler { get; private set; }
    private bool pulledOutOfBattle;
    public bool livesOnBackRow;
    public BattlerAction readiedAction;
    int _damageDealt = 0;
    int _drainRounds = 0;
    public bool _wantsToReenterBattle { get; private set; }
    public Battler _target { get; private set; }
    List<Battler> _subtargets;
    Queue<BattleMessageType> _effectMessages;

    void Awake()
    {
        _effectMessages = new Queue<BattleMessageType>();
        deathblowList = new List<Battler>(overseer.playerParty.Length);
        _subtargets = new List<Battler>(overseer.playerParty.Length);
    } 

    void Start ()
    {
        InitializeDisposables();
        activeBattler = true;
    }

    private void SetCooldown (BattlerAction action)
    {
        BattlerActionData d = BattlerActionData.get(action);
        switch (d.interruptType)
        {
            case BattlerActionInterruptType.BattleStart:
                _cooldowns_BSIA[battleStartInterruptActions.IndexOf(action)] = d.cooldownTurns;
                break;
            case BattlerActionInterruptType.None:
                _cooldowns_SBA[standardBracketActions.IndexOf(action)] = d.cooldownTurns;
                break;
            case BattlerActionInterruptType.OnAllyDeathblow:
                _cooldowns_OADIA[onAllyDeathblowInterruptActions.IndexOf(action)] = d.cooldownTurns;
                break;
            case BattlerActionInterruptType.OnAllyHit:
                _cooldowns_OAHIA[onAllyHitInterruptActions.IndexOf(action)] = d.cooldownTurns;
                break;
            case BattlerActionInterruptType.OnHit:
                _cooldowns_OHIA[onHitInterruptActions.IndexOf(action)] = d.cooldownTurns;
                break;
        }
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

    private Battler AcquireTarget (List<Battler> validEnemyTargets, List<Battler> friends, BattlerAction action)
    {
        BattlerActionData dat = BattlerActionData.get(action);
        int damage;
        int offense;
        int defense;
        bool meleeAtk;
        if (dat.HasEffectFlag(BattlerActionEffectFlags.IsMagic)) offense = MAG;
        else offense = MAR;
        if (dat.HasEffectFlag(BattlerActionEffectFlags.Melee)) meleeAtk = true;
        else meleeAtk = false;
        if (meleeAtk && !_breachOwnLines) offense--;
        if (offense < 0) offense = 0;
        Battler target = null; // if it's left null we're doing something that doesn't have a "target" per se
        if (validEnemyTargets.Count > 0)
        {
            switch (dat.target)
            {
                case BattlerActionTarget.TargetWeakestEnemy:
                    int mostDamageDealt = 0;
                    for (int i = 0; i < validEnemyTargets.Count; i++)
                    {
                        if (validEnemyTargets[i]._sacrificePow > 0)
                        {
                            bool nonDodgyTargetExists = false;
                            for (int ix = 0; ix < validEnemyTargets.Count; ix++)
                            {
                                if (validEnemyTargets[ix]._sacrificePow < 1)
                                {
                                    nonDodgyTargetExists = true;
                                    break;
                                }
                            }
                            if (nonDodgyTargetExists) continue; // don't hit a target that's ducking aggro if we can avoid it
                        }
                        if (dat.HasEffectFlag(BattlerActionEffectFlags.IsMagic)) defense = validEnemyTargets[i].MAG;
                        else defense = validEnemyTargets[i].MAR;
                        if (validEnemyTargets[i].livesOnBackRow && meleeAtk && !_breachFoeLines) defense += 1;
                        damage = CalcDamage(dat.baseDamage, offense, defense);
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
                        else if (damage > mostDamageDealt || (damage == mostDamageDealt && Random.Range(0, 2) == 0))
                        {
                            if (!meleeAtk || target == null || !validEnemyTargets[i].livesOnBackRow || target.livesOnBackRow || _breachFoeLines)
                            {
                                target = validEnemyTargets[i];
                                mostDamageDealt = damage;
                            }
                        }
                    }
                    break;
                case BattlerActionTarget.RandomTarget:
                    bool foundTarget = false;
                    while (!foundTarget)
                    {
                        target = validEnemyTargets[Random.Range(0, validEnemyTargets.Count)];
                        if ((target.livesOnBackRow && meleeAtk && !_breachFoeLines) || target._sacrificePow > 0)
                        {
                            if (Random.Range(0, 4) == 0) foundTarget = true;
                        }
                        else foundTarget = true;
                    }
                    break;
                case BattlerActionTarget.RandomTarget_ButOnlyCasters:
                    List<Battler> nuVet = new List<Battler>(validEnemyTargets.Count); // since we're just copying out of validEnemyTargets this is the largest capacity this list will ever need
                    for (int i = 0; i < validEnemyTargets.Count; i++)
                    {
                        if (validEnemyTargets[i].adventurer.advClass == AdventurerClass.Mystic || validEnemyTargets[i].adventurer.advClass == AdventurerClass.Wizard || validEnemyTargets[i].adventurer.advClass == AdventurerClass.Sage)
                        {
                            nuVet.Add(validEnemyTargets[i]);
                        }
                    }
                    if (nuVet.Count > 0)
                    {
                        foundTarget = false;
                        while (!foundTarget)
                        {
                            target = nuVet[Random.Range(0, nuVet.Count)];
                            if ((target.livesOnBackRow && meleeAtk && !_breachFoeLines) || target._sacrificePow > 0)
                            {
                                if (Random.Range(0, 4) == 0) foundTarget = true;
                            }
                            else foundTarget = true;
                        }
                    }
                    else target = null;
                    break;
                case BattlerActionTarget.TargetOwnSide:
                    target = null;
                    break;
                case BattlerActionTarget.HitAll:
                    target = null;
                    for (int i = 0; i < validEnemyTargets.Count; i++)
                    {
                        if (dat.HasEffectFlag(BattlerActionEffectFlags.IsMagic)) defense = validEnemyTargets[i].MAG;
                        else defense = validEnemyTargets[i].MAR;
                        if (validEnemyTargets[i].livesOnBackRow && meleeAtk && !_breachFoeLines) defense += 1;
                        damage = CalcDamage(dat.baseDamage, offense, defense);
                        if (damage >= validEnemyTargets[i].currentHP) deathblowList.Add(validEnemyTargets[i]);
                    }
                    break;
                case BattlerActionTarget.TargetSelf:
                    target = this;
                    break;
                case BattlerActionTarget.TargetEndangeredAlly:
                    if (dat.interruptType != BattlerActionInterruptType.None)
                    {
                        if (overseer.currentTurnTarget != this) target = overseer.currentTurnTarget;
                    }
                    else
                    {
                        int remainingHP = int.MaxValue;
                        for (int i = 0; i < friends.Count; i++)
                        {
                            if (friends[i].currentHP < remainingHP)
                            {
                                remainingHP = friends[i].currentHP;
                                target = friends[i];
                            }
                        }
                    }
                    break;
                case BattlerActionTarget.TargetActingBattler:
                    target = overseer.standardPriorityActingBattler;
                    break;
            }
            if (overseer.standardPriorityActingBattler == this) overseer.currentTurnTarget = target;
            if (target != null && !target.isValidTarget) target = null;
        }
        return target;
    }

    public void ApplyBarrier (int turns)
    {
        if (barrierTurns < turns)
        {
            barrierTurns = turns;
            puppet.incomingBuff = true;
        }
    }

    public void ApplyShieldBlock (int turns)
    {
        if (shieldBlockTurns < turns)
        {
            shieldBlockTurns = turns;
            puppet.incomingBuff = true;
        }
    }

    public void ApplyShieldWall (int turns)
    {
        if (shieldWallTurns < turns)
        {
            shieldWallTurns = turns;
            puppet.incomingBuff = true;
        }
    }

    public void AttackWith (List<Battler> validEnemyTargets, List<Battler> friends, BattlerAction action)
    {
        ReadyAttack(validEnemyTargets, friends, action);
        ExecuteAttack(validEnemyTargets, friends);
    }

    public void ReadyAttack (List<Battler> validEnemyTargets, List<Battler> friends, BattlerAction action)
    {
        BattlerActionData dat = BattlerActionData.get(action);
        _target = AcquireTarget(validEnemyTargets, friends, action);
        _subtargets.Clear();
        if (dat.numberOfSubtargets > 0 && validEnemyTargets.Count > 1)
        {
            int mvec = validEnemyTargets.Count;
            while (mvec > 1)
            {
                for (int i = 0; i < dat.numberOfSubtargets; i++)
                {
                    while (true)
                    {
                        if (_subtargets.Count <= i) _subtargets.Add(validEnemyTargets[Random.Range(0, validEnemyTargets.Count)]);
                        else _subtargets[i] = validEnemyTargets[Random.Range(0, validEnemyTargets.Count)];
                        if (_subtargets[i] != _target) break;
                    }
                    mvec--;
                }
            }

        }
        if (dat.HasEffectFlag(BattlerActionEffectFlags.Drain)) _drainRounds = 1 + _subtargets.Count;
        readiedAction = action;
        if (_target != null)
        {
            _target.incomingHit = _target.puppet.incomingHit = true;
            for (int i = 0; i < _subtargets.Count; i++) _subtargets[i].incomingHit = _subtargets[i].puppet.incomingHit = true;
        }
        else if (dat.target == BattlerActionTarget.HitAll)
        {
            for (int i = 0; i < validEnemyTargets.Count; i++) validEnemyTargets[i].incomingHit = validEnemyTargets[i].puppet.incomingHit = true;
        }
    }

    private void EnqueueEffectMessagesBasedOnFlags (BattlerActionEffectFlags flags, int str)
    {
        if ((flags & BattlerActionEffectFlags.Barrier) == BattlerActionEffectFlags.Barrier) _effectMessages.Enqueue(BattleMessageType.Barrier);
        if ((flags & BattlerActionEffectFlags.ShieldBlock) == BattlerActionEffectFlags.ShieldBlock) _effectMessages.Enqueue(BattleMessageType.ShieldBlock);
        if ((flags & BattlerActionEffectFlags.ShieldWall) == BattlerActionEffectFlags.ShieldWall) _effectMessages.Enqueue(BattleMessageType.ShieldWall);
        if ((flags & BattlerActionEffectFlags.Silence) == BattlerActionEffectFlags.Silence) _effectMessages.Enqueue(BattleMessageType.Silence);
        if ((flags & BattlerActionEffectFlags.MarBoost) == BattlerActionEffectFlags.MarBoost) _effectMessages.Enqueue(BattleMessageType.PumpedUp);
        if ((flags & BattlerActionEffectFlags.MagBoost) == BattlerActionEffectFlags.MagBoost) _effectMessages.Enqueue(BattleMessageType.MagBoost);
        if ((flags & BattlerActionEffectFlags.SpeBoost) == BattlerActionEffectFlags.SpeBoost) _effectMessages.Enqueue(BattleMessageType.Haste);
        if ((flags & BattlerActionEffectFlags.Sacrifice) == BattlerActionEffectFlags.Sacrifice) _effectMessages.Enqueue(BattleMessageType.Sacrifice);
        if ((flags & BattlerActionEffectFlags.DrainBlock) == BattlerActionEffectFlags.DrainBlock) _effectMessages.Enqueue(BattleMessageType.DrainKill);
        if ((flags & BattlerActionEffectFlags.StunBuff) == BattlerActionEffectFlags.StunBuff) _effectMessages.Enqueue(BattleMessageType.StunBuff);
        if ((flags & BattlerActionEffectFlags.DodgeBuff) == BattlerActionEffectFlags.DodgeBuff) _effectMessages.Enqueue(BattleMessageType.AttunedBuff);
        if ((flags & BattlerActionEffectFlags.FStep) == BattlerActionEffectFlags.FStep)
        {
            if (str > 1) _effectMessages.Enqueue(BattleMessageType.SwiftlyMovesBetweenLines);
            else _effectMessages.Enqueue(BattleMessageType.MovesBetweenLines);
        }
    }

    public void ApplyEffectsBasedOnFlags (BattlerActionEffectFlags flags, int str, int len)
    {
        if ((flags & BattlerActionEffectFlags.Barrier) == BattlerActionEffectFlags.Barrier) ApplyBarrier(str);
        if ((flags & BattlerActionEffectFlags.ShieldBlock) == BattlerActionEffectFlags.ShieldBlock) ApplyShieldBlock(str);
        if ((flags & BattlerActionEffectFlags.ShieldWall) == BattlerActionEffectFlags.ShieldWall) ApplyShieldWall(str);
        if ((flags & BattlerActionEffectFlags.Silence) == BattlerActionEffectFlags.Silence) Silence(str);
        if ((flags & BattlerActionEffectFlags.MarBoost) == BattlerActionEffectFlags.MarBoost) GiveBonusMar(str, len);
        if ((flags & BattlerActionEffectFlags.MagBoost) == BattlerActionEffectFlags.MagBoost) GiveBonusMag(str, len);
        if ((flags & BattlerActionEffectFlags.SpeBoost) == BattlerActionEffectFlags.SpeBoost) GiveBonusSpe(str, len);
        if ((flags & BattlerActionEffectFlags.Sacrifice) == BattlerActionEffectFlags.Sacrifice) MakeSacrifice(str, len);
        if ((flags & BattlerActionEffectFlags.DrainBlock) == BattlerActionEffectFlags.DrainBlock) DrainBlock(str);
        if ((flags & BattlerActionEffectFlags.FStep) == BattlerActionEffectFlags.FStep) FlashStep(str);
        if ((flags & BattlerActionEffectFlags.StunBuff) == BattlerActionEffectFlags.StunBuff) StunBuff(str);
        if ((flags & BattlerActionEffectFlags.DodgeBuff) == BattlerActionEffectFlags.DodgeBuff) DodgeBuff(str);
    }

    void DodgeBuff (int turns)
    {
        if (dodgeBuffTurns < turns)
        {
            dodgeBuffTurns = turns;
            puppet.incomingBuff = true;
        }
    }

    void FlashStep (int lv)
    {
        if (lv > 1) _breachFoeLines = true;
        _breachOwnLines = true;
        puppet.incomingBuff = true;
    }

    void DrainBlock (int turns)
    {
        if (drainBlockTurns < turns)
        {
            drainBlockTurns = turns;
            puppet.incomingBuff = true;
        }
    }

    void StunBuff (int turns)
    {
        if (stunBuffTurns < turns)
        {
            stunBuffTurns = turns;
            puppet.incomingBuff = true;
        }
    }

    void MakeSacrifice (int stat, int turns)
    {
        int dmg = currentHP / 3;
        if (currentHP - dmg == 0) dmg--;
        if (dmg == 0) dmg = 1;
        DealDamage(dmg);
        _sacrificePow = stat;
        _sacrificeTurns = turns;
        puppet.incomingBuff = true;
    }

    void GiveBonusMar (int mar, int turns)
    {
        if (_bonusMAR < 0 || _bonusMAR_turns < 1)
        {
            _bonusMAR = mar;
            _bonusMAR_turns = turns;
            puppet.incomingBuff = true;
        }
    }

    void GiveBonusMag (int mag, int turns)
    {
        if (_bonusMAG < 0 || _bonusMAG_turns < 1)
        {
            _bonusMAG = mag;
            _bonusMAG_turns = turns;
            puppet.incomingBuff = true;
        }
    }

    void GiveBonusSpe (int spe, int turns)
    {
        if (_bonusSPE < 0 || _bonusSPE_turns < 1)
        {
            _bonusSPE = spe;
            _bonusSPE_turns = turns;
            puppet.incomingBuff = true;
        }
    }

    public void Stun ()
    {
        if (stunnedTurns < 1) stunnedTurns = 1; // stun is really powerful so you can't stack it
    }

    private void UpdateSelfStateBasedOnFlags (BattlerActionEffectFlags flags)
    {
        if ((flags & BattlerActionEffectFlags.Encore) == BattlerActionEffectFlags.Encore)
        {
            overseer.GiveEncore(isEnemy);
        }
        if ((flags & BattlerActionEffectFlags.Bodyguard) == BattlerActionEffectFlags.Bodyguard)
        {
            _target.Cover(this);
            getBehindMeProc = true;
        }
    }

    public void ExecuteAttack (List<Battler> validEnemyTargets, List<Battler> friends)
    {
        BattlerActionData dat = BattlerActionData.get(readiedAction);
        if (stunnedTurns > 0)
        {
            stunnedTurns--;
            readiedAction = BattlerAction._CantMove_Stunned;       
        }
        else if (silentTurns > 0)
        {
            silentTurns--;
            if (dat.HasEffectFlag(BattlerActionEffectFlags.IsMagic)) readiedAction = BattlerAction._CantMove_Silenced;
        }
        if (dat.HasEffectFlag(BattlerActionEffectFlags.NeedsTarget) && (_target == null || !_target.isValidTarget))
        {
            AcquireTarget(validEnemyTargets, friends, readiedAction);
            if (_target == null) readiedAction = BattlerAction.None; // can't reacquire target - can't do anything
        }
        if (readiedAction == BattlerAction._CantMove_Stunned)
        {
            overseer.messageBox.Step(BattleMessageType.StunnedNoMove, this);
            overseer.lastActionAnim = BattlerActionAnim.None;
        }
        else if (readiedAction == BattlerAction._CantMove_Silenced)
        {
            overseer.messageBox.Step(BattleMessageType.FailedCast, this);
            overseer.lastActionAnim = BattlerActionAnim.None;
        }
        else if (readiedAction != BattlerAction.None)
        {
            int _damage = dat.baseDamage;
            int offense;
            int defense;
            if (dat.HasEffectFlag(BattlerActionEffectFlags.IsMagic)) offense = MAG;
            else offense = MAR;
            bool melee = dat.HasEffectFlag(BattlerActionEffectFlags.Melee);
            if (melee && !_breachOwnLines) offense--;
            if (offense < 0) offense = 0;
            if (_target == null)
            {
                if (dat.target == BattlerActionTarget.TargetOwnSide) for (int i = 0; i < friends.Count; i++) friends[i].ApplyEffectsBasedOnFlags(dat.flags, dat.effectPower, dat.cooldownTurns);
                else if (dat.target == BattlerActionTarget.HitAll) for (int i = 0; i < validEnemyTargets.Count; i++) validEnemyTargets[i].ApplyEffectsBasedOnFlags(dat.flags, dat.effectPower, dat.cooldownTurns);
            }
            else
            {
                _target.ApplyEffectsBasedOnFlags(dat.flags, dat.effectPower, dat.cooldownTurns);
                for (int i = 0; i < _subtargets.Count; i++) _subtargets[i].ApplyEffectsBasedOnFlags(dat.flags, dat.effectPower, dat.cooldownTurns);
            }
            EnqueueEffectMessagesBasedOnFlags(dat.flags, dat.effectPower);
            UpdateSelfStateBasedOnFlags(dat.flags);
            if (readiedAction == BattlerAction.Feedback)
            {
                _effectMessages.Enqueue(BattleMessageType.Feedback);
            }
            if (_damage < 0)
            {
                if (dat.target == BattlerActionTarget.TargetOwnSide)
                {
                    for (int i = 0; i < friends.Count; i++)
                    {
                        friends[i].DealDamage(_damage);
                    }
                }
                else if (_target != null) _target.DealDamage(_damage);
                else throw new System.Exception("Battler " + gameObject.name + " tried to use a heal action, but with no target.");
            }
            else if (_damage > 0)
            {
                if (adventurer.special == AdventurerSpecial.HammerSmash && Random.Range(0, 11) < 3)
                {
                    _damage *= 2;
                    _effectMessages.Enqueue(BattleMessageType.Critical);
                }
                if (dat.target == BattlerActionTarget.HitAll) for (int i = 0; i < validEnemyTargets.Count; i++)
                    {
                        if (dat.HasEffectFlag(BattlerActionEffectFlags.IsMagic)) defense = validEnemyTargets[i].MAG;
                        else defense = validEnemyTargets[i].MAR;
                        if (validEnemyTargets[i].livesOnBackRow && dat.HasEffectFlag(BattlerActionEffectFlags.Melee) && !_breachFoeLines) defense++;
                        _damageDealt += validEnemyTargets[i].DealDamage(CalcDamage(_damage, offense, defense));
                        if (melee && stunBuffTurns > 0)
                        {
                            stunBuffTurns--;
                            validEnemyTargets[i].Stun();
                            _effectMessages.Enqueue(BattleMessageType.Stun);
                            if (stunBuffTurns < 1) _effectMessages.Enqueue(BattleMessageType.StunBuffFaded);
                        }
                    }
                else if (_target != null)
                {
                    if (dat.HasEffectFlag(BattlerActionEffectFlags.IsMagic)) defense = _target.MAG;
                    else defense = _target.MAR;
                    if (_target.livesOnBackRow && dat.HasEffectFlag(BattlerActionEffectFlags.Melee) && !_breachFoeLines) defense++;
                    _damageDealt += _target.DealDamage(CalcDamage(_damage, offense, defense));
                    for (int i = 0; i < _subtargets.Count; i++)
                    {
                        if (dat.HasEffectFlag(BattlerActionEffectFlags.IsMagic)) defense = _subtargets[i].MAG;
                        else defense = _subtargets[i].MAR;
                        if (_subtargets[i].livesOnBackRow && dat.HasEffectFlag(BattlerActionEffectFlags.Melee) && !_breachFoeLines) defense++;
                        _damageDealt += _subtargets[i].DealDamage(CalcDamage(_damage, offense, defense));
                    }
                    if (melee && stunBuffTurns > 0)
                    {
                        stunBuffTurns--;
                        _target.Stun();
                        _effectMessages.Enqueue(BattleMessageType.Stun);
                        if (stunBuffTurns < 1) _effectMessages.Enqueue(BattleMessageType.StunBuffFaded);
                    }
                }
                else throw new System.Exception("Battler " + gameObject.name + " tried to use a damage-dealing action, but with no target.");
            }
            for (int i = 0; i < _subtargets.Count; i++)
            {
                if (!_subtargets[i].isValidTarget)
                {
                    _subtargets.RemoveAt(i);
                    _drainRounds--;
                }
            }
            if (_drainRounds > 0)
            {
                if (drainBlockTurns > 0)
                {
                    drainBlockTurns--;
                    _effectMessages.Enqueue(BattleMessageType.FailedToDrain);
                }
                else
                {
                    if (_drainRounds > 1) _effectMessages.Enqueue(BattleMessageType.MultiHeal);
                    else if (_drainRounds > 0) _effectMessages.Enqueue(BattleMessageType.Heal);
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
                }
            }
            overseer.messageBox.Step(BattleMessageType.StandardTurnMessage, this, readiedAction);
            while (_effectMessages.Count > 0) overseer.messageBox.Step(_effectMessages.Dequeue());
            overseer.lastActionAnim = dat.anim;
        }
        else overseer.lastActionAnim = BattlerActionAnim.None;

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
            if (dodgeBuffTurns > 0)
            {
                dodgeBuffTurns--;
                damage = 0;
                overseer.messageBox.Step(BattleMessageType.Dodged);
                if (dodgeBuffTurns < 1) overseer.messageBox.Step(BattleMessageType.AttunedBuffFaded);
            }
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
        currentHP = currentHP - damage;
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

    private bool StdActionAllowed (BattlerActionData d, int index)
    {
        if (_cooldowns_SBA[index] > 0) return false;
        if (d.HasEffectFlag(BattlerActionEffectFlags.ForbidAfterTurn1) && overseer.turn > 0) return false;
        if (d.HasEffectFlag(BattlerActionEffectFlags.ForbidOnTurn1) && overseer.turn == 0) return false;
        if (d.HasEffectFlag(BattlerActionEffectFlags.Sacrifice) && currentHP == 1) return false;
        return true;
    }

    private bool StdActionDecide (BattlerActionData ld, BattlerActionData d, bool spellcasterInEnemyParty, List<Battler> allies, List<Battler> opponents)
    {
        if (ld.HasEffectFlag(BattlerActionEffectFlags.Encore)) return false;
        if (d.HasEffectFlag(BattlerActionEffectFlags.Encore)) return true; // we never _don't_ want to take an encore because free turns, lol
        if (spellcasterInEnemyParty)
        {
            if (ld.HasEffectFlag(BattlerActionEffectFlags.Silence)) return false;
            if (d.HasEffectFlag(BattlerActionEffectFlags.Silence)) return true;
        }
        if (_sacrificePow < 1)
        {
            if (currentHP == adventurer.HP || Random.Range(0, 2) == 0)
            {
                if (ld.HasEffectFlag(BattlerActionEffectFlags.Sacrifice)) return false;
                if (d.HasEffectFlag(BattlerActionEffectFlags.Sacrifice)) return true;
            }
        }
        if (_bonusMAR < 1)
        {
            if (ld.HasEffectFlag(BattlerActionEffectFlags.MarBoost)) return false;
            if (d.HasEffectFlag(BattlerActionEffectFlags.MarBoost)) return true;
        }
        if (_bonusMAG < 1)
        {
            if (ld.HasEffectFlag(BattlerActionEffectFlags.MagBoost)) return false;
            if (d.HasEffectFlag(BattlerActionEffectFlags.MagBoost)) return true;
        }
        if (_bonusSPE < 1)
        {
            if (ld.HasEffectFlag(BattlerActionEffectFlags.SpeBoost)) return false;
            if (d.HasEffectFlag(BattlerActionEffectFlags.SpeBoost)) return true;
        }
        if (ld.HasEffectFlag(BattlerActionEffectFlags.ShieldBlock)) return false;
        if (d.HasEffectFlag(BattlerActionEffectFlags.ShieldBlock)) return true;
        if (d.baseDamage > 0)
        {
            if (d.baseDamage * opponents.Count >= BattlerActionData.get(defaultAction).baseDamage)
            {
                if (ld.target == BattlerActionTarget.HitAll) return false;
                if (d.target == BattlerActionTarget.HitAll) return true;
            }
        }
        return false;
    }

    public BattlerAction GetStandardAction (int turn, List<Battler> allies, List<Battler> opponents)
    {
        BattlerAction action = defaultAction;
        BattlerActionData d;
        BattlerActionData ld;
        ld = BattlerActionData.get(BattlerAction.None);
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
            for (int i = 0; i < standardBracketActions.Count; i++)
            {
                d = BattlerActionData.get(standardBracketActions[i]);
                if (StdActionAllowed(d, i))
                {
                    if (StdActionDecide(ld, d, spellcasterInEnemyParty, allies, opponents))
                    {
                        ld = d;
                        action = standardBracketActions[i];
                    }
                }
            }
        }
        readiedAction = action;
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

    public void GenerateBattleData (Adventurer _adventurer)
    {
        SetBattlerActive();
        dead = false;
        if (standardBracketActions == null) standardBracketActions = new List<BattlerAction>(); else standardBracketActions.Clear();
        if (battleStartInterruptActions == null) battleStartInterruptActions = new List<BattlerAction>(); else battleStartInterruptActions.Clear();
        if (onAllyHitInterruptActions == null) onAllyHitInterruptActions = new List<BattlerAction>(); else onAllyHitInterruptActions.Clear();
        if (onAllyDeathblowInterruptActions == null) onAllyDeathblowInterruptActions = new List<BattlerAction>(); else onAllyDeathblowInterruptActions.Clear();
        if (onHitInterruptActions == null) onHitInterruptActions = new List<BattlerAction>(); else onHitInterruptActions.Clear();
        adventurer = _adventurer;
        currentHP = adventurer.HP;
        RefreshTempState();
        for (int i = 0; i < adventurer.attacks.Length; i++)
        {
            BattlerActionData bad = BattlerActionData.get(adventurer.attacks[i]);
            switch (bad.interruptType)
            {
                case BattlerActionInterruptType.None:
                    standardBracketActions.Add(adventurer.attacks[i]);
                    break;
                case BattlerActionInterruptType.BattleStart:
                    battleStartInterruptActions.Add(adventurer.attacks[i]);
                    break;
                case BattlerActionInterruptType.OnAllyDeathblow:
                    onAllyDeathblowInterruptActions.Add(adventurer.attacks[i]);
                    break;
                case BattlerActionInterruptType.OnAllyHit:
                    onAllyHitInterruptActions.Add(adventurer.attacks[i]);
                    break;
                case BattlerActionInterruptType.OnHit:
                    onHitInterruptActions.Add(adventurer.attacks[i]);
                    break;
            }
        }
        if (adventurer.special == AdventurerSpecial.ShieldWall) battleStartInterruptActions.Add(BattlerAction.ShieldWall); // to do: replace this block with a smarter solution
        else if (adventurer.special == AdventurerSpecial.Barrier) battleStartInterruptActions.Add(BattlerAction.Barrier);
        else if (adventurer.special == AdventurerSpecial.Feedback) onHitInterruptActions.Add(BattlerAction.Feedback);
        else if (adventurer.special == AdventurerSpecial.Protect) onAllyHitInterruptActions.Add(BattlerAction.Protect);
        _cooldowns_SBA = new int[standardBracketActions.Count];
        _cooldowns_BSIA = new int[battleStartInterruptActions.Count];
        _cooldowns_OAHIA = new int[onAllyHitInterruptActions.Count];
        _cooldowns_OADIA = new int[onAllyDeathblowInterruptActions.Count];
        _cooldowns_OHIA = new int[onHitInterruptActions.Count];
        livesOnBackRow = !Adventurer.ClassIsFrontRowClass(adventurer.advClass);
        puppet.Setup();
    }

    public void TickCooldownsAndShit ()
    {
        if (barrierTurns > 0) barrierTurns--;
        for (int i = 0; i < _cooldowns_BSIA.Length; i++) if (_cooldowns_BSIA[i] > 0) _cooldowns_BSIA[i]--;
        for (int i = 0; i < _cooldowns_OADIA.Length; i++) if (_cooldowns_OADIA[i] > 0) _cooldowns_OADIA[i]--;
        for (int i = 0; i < _cooldowns_OAHIA.Length; i++) if (_cooldowns_OAHIA[i] > 0) _cooldowns_OAHIA[i]--;
        for (int i = 0; i < _cooldowns_OHIA.Length; i++) if (_cooldowns_OHIA[i] > 0) _cooldowns_OHIA[i]--;
        for (int i = 0; i < _cooldowns_SBA.Length; i++) if (_cooldowns_SBA[i] > 0) _cooldowns_SBA[i]--;
        bool lostStatBoosts = false;
        if (_bonusMAR_turns > 0) _bonusMAR_turns--;
        if (_bonusMAR_turns == 0 && _bonusMAR != 0)
        {
            _bonusMAR = 0;
            lostStatBoosts = true;
        }
        if (_bonusMAG_turns > 0) _bonusMAG_turns--;
        if (_bonusMAG_turns == 0 && _bonusMAG != 0)
        {
            _bonusMAG = 0;
            lostStatBoosts = true;
        }
        if (_bonusSPE_turns > 0) _bonusSPE_turns--;
        if (_bonusSPE_turns == 0 && _bonusSPE != 0)
        {
            _bonusSPE = 0;
            lostStatBoosts = true;
        }
        if (lostStatBoosts) _effectMessages.Enqueue(BattleMessageType.StatBoostsLost);
        if (_sacrificeTurns > 0) _sacrificeTurns--;
        if (_sacrificeTurns == 0 && _sacrificePow != 0)
        {
            _sacrificePow = 0;
            _effectMessages.Enqueue(BattleMessageType.SacrificeLost);
        }
        if (shieldWallTurns > 0) shieldWallTurns--;
        InitializeDisposables();
    }

    public void RollMoveSpeed ()
    {
        float speed = SPE;
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

    private bool HasAttack (BattlerAction attack)
    {
        return standardBracketActions.Contains(attack) || battleStartInterruptActions.Contains(attack) || onHitInterruptActions.Contains(attack) || onAllyDeathblowInterruptActions.Contains(attack) || onAllyHitInterruptActions.Contains(attack);
    }

    private void RefreshTempState ()
    {
        _breachOwnLines = false;
        _breachFoeLines = false;
        getBehindMeProc = false;
        barrierTurns = 0;
        shieldBlockTurns = 0;
        shieldWallTurns = 0;
        silentTurns = 0;
        drainBlockTurns = 0;
        _bonusMAR = 0;
        _bonusMAR_turns = 0;
        _bonusMAG = 0;
        _bonusMAG_turns = 0;
        _bonusSPE = 0;
        _bonusSPE_turns = 0;
        _sacrificePow = 0;
        _sacrificeTurns = 0;
        stunnedTurns = 0;
        dodgeBuffTurns = 0;
        InitializeDisposables();
    }

    private void InitializeDisposables ()
    {
        bodyguard = null;
        incomingHit = false;
        _damageDealt = 0;
        _wantsToReenterBattle = false;
        _drainRounds = 0;
        _target = null;
        _subtargets.Clear();
        readiedAction = BattlerAction.None;
        if (deathblowList.Count > 0) deathblowList.Clear();
    }
}