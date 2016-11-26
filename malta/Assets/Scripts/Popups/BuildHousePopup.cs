using UnityEngine;
using UnityEngine.UI;

public class BuildHousePopup : MonoBehaviour
{
    public TownBuilding associatedHouse;
    public PopupMenu housePopup;
    public PopupMenu insufficientResourcesPopup;
    public PopupMenu shell;

    public void BuildIfPossible ()
    {
        if (GameDataManager.Instance.SpendResourcesIfPossible(0, 0, 0, 10, 10, 10))
        {
            associatedHouse.BuildFromFoundation();
            associatedHouse.OpenPopupOnBuilding();
            shell.Close();
        }
        else
        {
            shell.SurrenderFocus();
            insufficientResourcesPopup.Open();
        }
    }

}
