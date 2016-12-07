using UnityEngine;
using UnityEngine.UI;

public class BattlerPuppet : MonoBehaviour
{
    public Battler battler;
    public Text nameText;
    public Text titleText;
    public Text hpText;
    public BattleDamageNumbersGadget damageGadget;
    public bool incomingHit;
    private int cachedHP;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Respond ()
    {
        if (incomingHit)
        {
            damageGadget.Trigger(battler.lastDamage);
            incomingHit = false;
        }
        if (cachedHP != battler.currentHP) RefreshHPText();
        if (battler.dead && gameObject.activeInHierarchy)
        {
            battler.overseer.messageBox.corpseQueue.Enqueue(battler);
            battler.overseer.messageBox.Step(BattleMessageType.SomebodyDead);
            gameObject.SetActive(false);
        }
    }

    public void Setup ()
    {
        if (nameText != null) nameText.text = battler.adventurer.fullName;
        if (titleText != null) titleText.text = battler.adventurer.title;
        RefreshHPText();
    }

    void RefreshHPText ()
    {
        hpText.text = battler.currentHP.ToString() + " / " + battler.adventurer.HP.ToString();
        cachedHP = battler.currentHP;
    }
}
