using UnityEngine;
using UnityEngine.UI;

public class SovereignInfoPanel : MonoBehaviour
{
    public PopupMenu shell;
    public PopupMenu sovereignSpecialPopup;
    public PopupMenu sovereignTacticsPopup;
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
    private const string dividerString = " | ";
	
	// Update is called once per frame
	void Update ()
    {
        if (cachedSovereignName != GameDataManager.Instance.sovereignName)
        {
            cachedSovereignName = GameDataManager.Instance.sovereignName;
            sovereignNameLabel.text = strings[0] + GameDataManager.Instance.sovereignName;
        }
        if (cachedSovereignTactic != GameDataManager.Instance.sovereignTactic)
        {
            cachedSovereignTactic = GameDataManager.Instance.sovereignTactic;
            sovereignTacticDesc.text = Adventurer.GetAttackDescription(GameDataManager.Instance.sovereignTactic);
        }
        if (cachedSovereignSpecial != GameDataManager.Instance.sovereignSkill)
        {
            cachedSovereignSpecial = GameDataManager.Instance.sovereignSkill;
            sovereignSpecialDesc.text = Adventurer.GetSpecialDescription(GameDataManager.Instance.sovereignSkill);
        }
        if (cachedSovereignTitle != GameDataManager.Instance.sovereignAdventurer.fullTitle)
        {
            cachedSovereignTitle = GameDataManager.Instance.sovereignAdventurer.fullTitle;
            sovereignTitleArea.text = cachedSovereignTitle;
        }
        if (cachedSovereignStats[0] != GameDataManager.Instance.sovereignAdventurer.HP ||
            cachedSovereignStats[1] != GameDataManager.Instance.sovereignAdventurer.Martial ||
            cachedSovereignStats[2] != GameDataManager.Instance.sovereignAdventurer.Magic ||
            cachedSovereignStats[3] != GameDataManager.Instance.sovereignAdventurer.Speed)
        {
            cachedSovereignStats = new int[] { GameDataManager.Instance.sovereignAdventurer.HP, GameDataManager.Instance.sovereignAdventurer.Martial,
                GameDataManager.Instance.sovereignAdventurer.Magic, GameDataManager.Instance.sovereignAdventurer.Speed };
            sovereignStatsArea.text = strings[2] + cachedSovereignStats[0] + dividerString + strings[3] + cachedSovereignStats[1] + dividerString +
                                      strings[4] + cachedSovereignStats[2] + dividerString + strings[5] + cachedSovereignStats[3];
        }
        if (cachedSovereignAttacks[0] != GameDataManager.Instance.sovereignAdventurer.attacks[0] ||
            cachedSovereignAttacks[1] != GameDataManager.Instance.sovereignAdventurer.attacks[1])
        {
            cachedSovereignAttacks = GameDataManager.Instance.sovereignAdventurer.attacks.Clone() as AdventurerAttack[];
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
