using UnityEngine;
using UnityEngine.UI;

public class SovereignInfoPanel : MonoBehaviour
{
    public PopupMenu shell;
    public PopupMenu sovereignSpecialPopup;
    public PopupMenu sovereignTacticsPopup;
    public Image sovereignMugshot;
    public Text sovereignNameLabel;
    public Text sovereignSpecialDesc;
    public Text sovereignTitleArea;
    public Text sovereignAttacksArea;
    public Text sovereignStatsArea;
    public Text sovereignTacticDesc;
    public string[] strings;
    private int[] cachedSovereignStats = { -1, -1, -1, -1 };
    private string cachedSovereignName = "¡²¤€¼½¾‘’¥×äåé®®þüúíó«»áß";
    private string cachedSovereignTitle = "¡²¤€¼½¾‘’¥×äåé®®þüúíó«»áß";
    private AdventurerAttack cachedSovereignTactic = AdventurerAttack.None;
    private AdventurerAttack[] cachedSovereignAttacks = { AdventurerAttack.None, AdventurerAttack.None };
    private AdventurerSpecial cachedSovereignSpecial = AdventurerSpecial.LoseBattle;
    private AdventurerMugshot cachedSovereignMugshot;
    private const string dividerString = " | ";
	
	// Update is called once per frame
	void Update ()
    {
        if (cachedSovereignName != GameDataManager.Instance.dataStore.sovereignName)
        {
            cachedSovereignName = GameDataManager.Instance.dataStore.sovereignName;
            sovereignNameLabel.text = strings[0] + GameDataManager.Instance.dataStore.sovereignName;
        }
        if (cachedSovereignMugshot != GameDataManager.Instance.dataStore.sovereignMugshot)
        {
            cachedSovereignMugshot = GameDataManager.Instance.dataStore.sovereignMugshot;
            sovereignMugshot.sprite = GameDataManager.Instance.dataStore.sovereignAdventurer.GetMugshotGraphic();
        }
        if (cachedSovereignTactic != GameDataManager.Instance.dataStore.sovereignTactic)
        {
            cachedSovereignTactic = GameDataManager.Instance.dataStore.sovereignTactic;
            sovereignTacticDesc.text = Adventurer.GetAttackDescription(GameDataManager.Instance.dataStore.sovereignTactic);
        }
        if (cachedSovereignSpecial != GameDataManager.Instance.dataStore.sovereignSkill)
        {
            cachedSovereignSpecial = GameDataManager.Instance.dataStore.sovereignSkill;
            sovereignSpecialDesc.text = Adventurer.GetSpecialDescription(GameDataManager.Instance.dataStore.sovereignSkill);
        }
        if (cachedSovereignTitle != GameDataManager.Instance.dataStore.sovereignAdventurer.fullTitle)
        {
            cachedSovereignTitle = GameDataManager.Instance.dataStore.sovereignAdventurer.fullTitle;
            sovereignTitleArea.text = cachedSovereignTitle;
        }
        if (cachedSovereignStats[0] != GameDataManager.Instance.dataStore.sovereignAdventurer.HP ||
            cachedSovereignStats[1] != GameDataManager.Instance.dataStore.sovereignAdventurer.Martial ||
            cachedSovereignStats[2] != GameDataManager.Instance.dataStore.sovereignAdventurer.Magic ||
            cachedSovereignStats[3] != GameDataManager.Instance.dataStore.sovereignAdventurer.Speed)
        {
            cachedSovereignStats = new int[] { GameDataManager.Instance.dataStore.sovereignAdventurer.HP, GameDataManager.Instance.dataStore.sovereignAdventurer.Martial,
                GameDataManager.Instance.dataStore.sovereignAdventurer.Magic, GameDataManager.Instance.dataStore.sovereignAdventurer.Speed };
            sovereignStatsArea.text = strings[2] + cachedSovereignStats[0] + dividerString + strings[3] + cachedSovereignStats[1] + dividerString +
                                      strings[4] + cachedSovereignStats[2] + dividerString + strings[5] + cachedSovereignStats[3];
        }
        if (cachedSovereignAttacks[0] != GameDataManager.Instance.dataStore.sovereignAdventurer.attacks[0] ||
            cachedSovereignAttacks[1] != GameDataManager.Instance.dataStore.sovereignAdventurer.attacks[1])
        {
            cachedSovereignAttacks = GameDataManager.Instance.dataStore.sovereignAdventurer.attacks.Clone() as AdventurerAttack[];
            if (cachedSovereignAttacks[1] == AdventurerAttack.None) sovereignAttacksArea.text = Adventurer.GetAttackName(cachedSovereignAttacks[0]);
            else sovereignAttacksArea.text = Adventurer.GetAttackName(cachedSovereignAttacks[0]) + dividerString + Adventurer.GetAttackName(cachedSovereignAttacks[1]);
        }
    }

    public void OpenSovereignSpecialPopup()
    {
        shell.SurrenderFocus();
        sovereignSpecialPopup.Open();
    }

    public void OpenSovereignTacticsPopup()
    {
        shell.SurrenderFocus();
        sovereignTacticsPopup.Open();
    }
}
