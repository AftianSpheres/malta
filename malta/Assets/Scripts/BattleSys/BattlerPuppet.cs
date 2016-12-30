using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MovementEffects;

public class BattlerPuppet : MonoBehaviour
{
    public Battler battler;
    public Button pullOutButton;
    public Image panel;
    public Image mugshot;
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
    private Vector3 originalPos;
    private Vector3 originalScale;
    const float deathAnimLength = 1.0f;
    const float pullOutAnimLength = 1.0f;
    const float pullOutAnimDist = 360.0f;

    void Start ()
    {
        conditionStrings = conditionStringsResource.text.Split('\n');
        originalPos = transform.position;
        originalScale = transform.localScale;
    }

    void Update ()
    {
        if (killedPuppet && !damageAnimGadget.triggeredGadget && transform.localScale == originalScale) DeathAnim(); // let hit anims play before vanishing 
        if (pullOutButton != null)
        {
            if (!battler.activeBattler)
            {
                if (pullOutButton.gameObject.activeSelf) pullOutButton.gameObject.SetActive(false);
            }
            else if (battler.overseer.currentlyInFight)
            {
                if (pullOutButton.interactable && pullOutButton.gameObject.activeSelf) pullOutButton.gameObject.SetActive(false);
            }
            else
            {
                if (battler.overseer.outOfBattleBattler != null && pullOutButton.interactable == true)
                {
                    if (pullOutButton.gameObject.activeSelf) pullOutButton.gameObject.SetActive(false);
                }
                else if (!pullOutButton.gameObject.activeSelf && battler.currentHP < battler.adventurer.HP) pullOutButton.gameObject.SetActive(true);
            }
        }
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
        if (mugshot != null)
        {
            if (battler.isEnemy) mugshot.sprite = battler.adventurer.GetEnemyGraphic();
            else mugshot.sprite = battler.adventurer.GetMugshotGraphic();
        }
        RefreshHPText();
        transform.position = originalPos;
        transform.localScale = originalScale;
        killedPuppet = false;
    }

    public void AliveDisplay()
    {
        if (mugshot != null) mugshot.enabled = true;
        if (panel != null) panel.enabled = true;
        if (conditionText != null) conditionText.enabled = true;
        if (nameText != null) nameText.enabled = true;
        if (titleText != null) titleText.enabled = true;
        if (hpText != null) hpText.enabled = true;
    }

    public void KillDisplay ()
    {
        if (mugshot != null) mugshot.enabled = false;
        if (panel != null) panel.enabled = false;
        if (conditionText != null) conditionText.enabled = false;
        if (nameText != null) nameText.enabled = false;
        if (titleText != null) titleText.enabled = false;
        if (hpText != null) hpText.enabled = false;
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

    public void ReenterBattleAnim ()
    {
        transform.position = originalPos;
    }

    public void DeathAnim ()
    {
        Timing.RunCoroutine(_DeathAnim());
    }

    IEnumerator<float> _DeathAnim ()
    {
        float timer = 0;
        while (timer < deathAnimLength)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, (timer / deathAnimLength));
            yield return 0f;
        }
        KillDisplay();
        yield break;
    }

    public void PullOutAnim ()
    {
        Timing.RunCoroutine(_PullOutAnim());
    }

    IEnumerator<float> _PullOutAnim ()
    {
        pullOutButton.interactable = false;
        float timer = 0;
        while (timer < pullOutAnimLength)
        {
            timer += Time.deltaTime;
            transform.position = originalPos + (Vector3.left * pullOutAnimDist * (timer / pullOutAnimLength));
            yield return 0f;
        }
        pullOutButton.interactable = true;
        yield break;
    }
}
