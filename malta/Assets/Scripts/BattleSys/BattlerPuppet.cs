using UnityEngine;
using UnityEngine.UI;

public class BattlerPuppet : MonoBehaviour
{
    public Battler battler;
    public Text conditionText;
    public Text nameText;
    public Text titleText;
    public Text hpText;
    public TextAsset conditionStringsResource;
    public BuffAnimGadget buffAnimGadget;
    public BattleDamageAnimGadget damageAnimGadget;
    public BattleDamageNumbersGadget damageGadget;
    public bool incomingHit;
    public bool incomingBuff;
    private int cachedHP;
    private bool killedPuppet;
    private string[] conditionStrings;

    void Start ()
    {
        conditionStrings = conditionStringsResource.text.Split('\n');
    }

    void Update ()
    {
        if (killedPuppet && !damageAnimGadget.triggeredGadget) gameObject.SetActive(false); // let hit anims play before vanishing 
	}

    public void Respond ()
    {
        if (incomingHit)
        {
            damageAnimGadget.Trigger(battler.lastDamage);
            damageGadget.Trigger(battler.lastDamage);
            incomingHit = false;
        }
        if (incomingBuff)
        {
            buffAnimGadget.Trigger();
            incomingBuff = false;
        }
        if (cachedHP != battler.currentHP) RefreshHPText();
        if (battler.dead && !killedPuppet)
        {
            killedPuppet = true;
            battler.overseer.messageBox.corpseQueue.Enqueue(battler);
            battler.overseer.messageBox.Step(BattleMessageType.SomebodyDead);         
        }
        RefreshConditionText();
    }

    public void Setup ()
    {
        if (nameText != null) nameText.text = battler.adventurer.fullName;
        if (titleText != null) titleText.text = battler.adventurer.title;
        RefreshHPText();
        killedPuppet = false;
    }

    void RefreshConditionText ()
    {
        if (battler._underEncore) { if (conditionText.text != conditionStrings[5]) conditionText.text = conditionStrings[5]; }
        else if (battler._underBarrier) { if (conditionText.text != conditionStrings[4]) conditionText.text = conditionStrings[4]; }
        else if (battler._underShieldBlock) { if (conditionText.text != conditionStrings[3]) conditionText.text = conditionStrings[3]; }
        else if (battler._underShieldWall) { if (conditionText.text != conditionStrings[2]) conditionText.text = conditionStrings[2]; }
        else if (battler._underSilence) { if (conditionText.text != conditionStrings[1]) conditionText.text = conditionStrings[1]; }
        else if (conditionText.text != conditionStrings[0]) conditionText.text = conditionStrings[0];
    }

    void RefreshHPText ()
    {
        hpText.text = battler.currentHP.ToString() + " / " + battler.adventurer.HP.ToString();
        cachedHP = battler.currentHP;
    }
}
