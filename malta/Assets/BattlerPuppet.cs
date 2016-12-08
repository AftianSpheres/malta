using UnityEngine;
using UnityEngine.UI;

public class BattlerPuppet : MonoBehaviour
{
    public Battler battler;
    public Text nameText;
    public Text titleText;
    public Text hpText;
    public BattleDamageAnimGadget damageAnimGadget;
    public BattleDamageNumbersGadget damageGadget;

    public bool incomingHit;
    private int cachedHP;
    private bool killedPuppet;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
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
        if (cachedHP != battler.currentHP) RefreshHPText();
        if (battler.dead && !killedPuppet)
        {
            killedPuppet = true;
            battler.overseer.messageBox.corpseQueue.Enqueue(battler);
            battler.overseer.messageBox.Step(BattleMessageType.SomebodyDead);
            
        }
    }

    public void Setup ()
    {
        if (nameText != null) nameText.text = battler.adventurer.fullName;
        if (titleText != null) titleText.text = battler.adventurer.title;
        RefreshHPText();
        killedPuppet = false;
    }

    void RefreshHPText ()
    {
        hpText.text = battler.currentHP.ToString() + " / " + battler.adventurer.HP.ToString();
        cachedHP = battler.currentHP;
    }
}
