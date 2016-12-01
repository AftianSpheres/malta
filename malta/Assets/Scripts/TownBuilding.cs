using UnityEngine;
using System.Collections;

enum BuildingStates_House
{
    Base,
    WithOutbuilding,
    Foundation
}

/// <summary>
/// Handles basic data management for town buildings.
/// To do: serialization/saving/loading
/// </summary>
public class TownBuilding : MonoBehaviour
{
    public static int[] buildingTypeMaxLevels = { 1, 1, 10, 10, 10, 1, 10, 10, 10, 10, 10 };
    public static int[] houseConstructionCosts = { 0, 0, 0, 10, 10, 10 };
    public static int[] houseOutbuildingCosts = { 0, 0, 0, 20, 20, 20 };
    public int buildingStateIndex;
    public int nonUniqueBuildingsIndex;
    public PopupMenu[] associatedPopups;
    public HousePopup housePopup;
    public BuildHousePopup buildHousePopup;
    public BuildingType buildingType;
    public Adventurer associatedAdventurer;
    new public BoxCollider2D collider;
    public SpriteRenderer spriteRenderer;
    public Sprite[] buildingSprites;
    public TextMesh buildingMessage;
    public bool hasOutbuilding;
    public bool isUndeveloped;
    public bool forgeOutbuildingIsKobold;
    private bool buildingAlteredSinceLastUpdate = true;

    // Update is called once per frame
    void Update ()
    {
        if (GameDataManager.Instance != null) // only matters in editor, but prevents silly timing-related crashes
        {
            if (buildingAlteredSinceLastUpdate) RefreshBuildingAssociations(); // doing it like this also lets you mark buildings as "dirty" based on timed events
            if (associatedAdventurer != null && !associatedAdventurer.initialized)
            {
                associatedAdventurer.Reroll(AdventurerClass.Warrior, AdventurerSpecies.Human, hasOutbuilding, new int[] { 0, 0, 0, 0 });
            }
        }
    }

    private void OnMouseDown()
    {
        if (associatedPopups[buildingStateIndex] != null && !GameStateManager.Instance.popupHasFocus)
        {
            OpenPopupOnBuilding();
        }
    }

    public void BuildFromFoundation()
    {
        if (!isUndeveloped) throw new System.Exception("Can't build from foundation because building " + gameObject.name + " is already developed!");
        GameDataManager.Instance.housesBuilt[nonUniqueBuildingsIndex] = true;
        buildingStateIndex = 0;
        isUndeveloped = false;
        buildingAlteredSinceLastUpdate = true;
    }

    public void BuildOutbuilding (bool koboldIfForge = false)
    {
        if (hasOutbuilding) throw new System.Exception("Tried to add outbuilding to building, but it already had one: " + gameObject.name);
        switch (buildingType)
        {
            case BuildingType.House:
                GameDataManager.Instance.housesOutbuildingsBuilt[nonUniqueBuildingsIndex] = true;
                associatedAdventurer.Promote();
                buildingAlteredSinceLastUpdate = true;
                break;
            case BuildingType.Forge:
                if (koboldIfForge) GameDataManager.Instance.unlock_Taskmaster = true;
                else
                {
                    associatedAdventurer = GameDataManager.Instance.forgeAdventurer = ScriptableObject.CreateInstance<Adventurer>();
                    associatedAdventurer.Reroll(GameDataManager.Instance.warriorClassUnlock, AdventurerSpecies.Human, false, new int[] { 0, 0, 0, 0 });
                }
                buildingAlteredSinceLastUpdate = true;
                break;
            default:
                throw new System.Exception("Can't add outbuilding to building of type " + buildingType.ToString());
        }
    }

    /// <summary>
    /// This is a gross lookup table implemented in code because it lets us do
    /// balancing tweaks with more precision than an actual algorithmic approach.
    /// </summary>
    public static int[] GetUpgradeCost_Docks (int level)
    {
        int[] costs; // clay, wood, ore, brick, plank, metal
        switch (level)
        {
            case 0:
                costs = new int[] { 2, 2, 2, 0, 0, 0 };
                break;
            case 1:
                costs = new int[] { 4, 4, 4, 0, 0, 0 };
                break;
            case 2:
                costs = new int[] { 4, 4, 4, 4, 4, 4 };
                break;
            case 3:
                costs = new int[] { 8, 8, 8, 8, 8, 8 };
                break;
            case 4:
                costs = new int[] { 16, 16, 16, 16, 16, 16 };
                break;
            case 5:
                costs = new int[] { 32, 32, 32, 32, 32, 32 };
                break;
            case 6:
                costs = new int[] { 64, 64, 64, 64, 64, 64 };
                break;
            case 7:
                costs = new int[] { 128, 128, 128, 128, 128, 128 };
                break;
            case 8:
                costs = new int[] { 205, 205, 205, 307, 307, 307 };
                break;
            case 9:
                costs = new int[] { 410, 410, 410, 614, 614, 614 };
                break;
            default:
                costs = new int[] { 0, 0, 0, 0, 0, 0 };
                break;
        }
        return costs;
    }

    /// <summary>
    /// This is a gross lookup table implemented in code because it lets us do
    /// balancing tweaks with more precision than an actual algorithmic approach.
    /// </summary>
    public static int[] GetUpgradeCost_MasonClaypit(int level)
    {
        int[] costs; // clay, wood, ore, brick, plank, metal
        switch (level)
        {
            case 0:
                costs = new int[] { 2, 0, 0, 0, 0, 0 };
                break;
            case 1:
                costs = new int[] { 4, 0, 0, 0, 0, 0 };
                break;
            case 2:
                costs = new int[] { 4, 0, 0, 4, 0, 0 };
                break;
            case 3:
                costs = new int[] { 8, 0, 0, 8, 0, 0 };
                break;
            case 4:
                costs = new int[] { 16, 0, 0, 16, 0, 0 };
                break;
            case 5:
                costs = new int[] { 32, 0, 0, 32, 0, 0 };
                break;
            case 6:
                costs = new int[] { 64, 0, 0, 64, 0, 0 };
                break;
            case 7:
                costs = new int[] { 128, 0, 0, 128, 0, 0 };
                break;
            case 8:
                costs = new int[] { 154, 26, 26, 230, 38, 38 };
                break;
            case 9:
                costs = new int[] { 308, 52, 52, 460, 76, 76 };
                break;
            default:
                costs = new int[] { 0, 0, 0, 0, 0, 0 };
                break;
        }
        return costs;
    }

    /// <summary>
    /// This is a gross lookup table implemented in code because it lets us do
    /// balancing tweaks with more precision than an actual algorithmic approach.
    /// </summary>
    public static int[] GetUpgradeCost_SawmillWoodlands(int level)
    {
        int[] costs; // clay, wood, ore, brick, plank, metal
        switch (level)
        {
            case 0:
                costs = new int[] { 0, 2, 0, 0, 0, 0 };
                break;
            case 1:
                costs = new int[] { 0, 4, 0, 0, 0, 0 };
                break;
            case 2:
                costs = new int[] { 0, 4, 0, 0, 4, 0 };
                break;
            case 3:
                costs = new int[] { 0, 8, 0, 0, 8, 0 };
                break;
            case 4:
                costs = new int[] { 0, 16, 0, 0, 16, 0 };
                break;
            case 5:
                costs = new int[] { 0, 32, 0, 0, 32, 0 };
                break;
            case 6:
                costs = new int[] { 0, 64, 0, 0, 64, 0 };
                break;
            case 7:
                costs = new int[] { 0, 128, 0, 0, 128, 0 };
                break;
            case 8:
                costs = new int[] { 26, 154, 26, 38, 230, 38 };
                break;
            case 9:
                costs = new int[] { 52, 308, 52, 76, 460, 76 };
                break;
            default:
                costs = new int[] { 0, 0, 0, 0, 0, 0 };
                break;
        }
        return costs;
    }

    /// <summary>
    /// This is a gross lookup table implemented in code because it lets us do
    /// balancing tweaks with more precision than an actual algorithmic approach.
    /// </summary>
    public static int[] GetUpgradeCost_SmithMines(int level)
    {
        int[] costs; // clay, wood, ore, brick, plank, metal
        switch (level)
        {
            case 0:
                costs = new int[] { 0, 0, 2, 0, 0, 0 };
                break;
            case 1:
                costs = new int[] { 0, 0, 4, 0, 0, 0 };
                break;
            case 2:
                costs = new int[] { 0, 0, 4, 0, 0, 4 };
                break;
            case 3:
                costs = new int[] { 0, 0, 8, 0, 0, 8 };
                break;
            case 4:
                costs = new int[] { 0, 0, 16, 0, 0, 16 };
                break;
            case 5:
                costs = new int[] { 0, 0, 32, 0, 0, 32 };
                break;
            case 6:
                costs = new int[] { 0, 0, 64, 0, 0, 64 };
                break;
            case 7:
                costs = new int[] { 0, 0, 128, 0, 0, 128 };
                break;
            case 8:
                costs = new int[] { 26, 26, 154, 38, 38, 230 };
                break;
            case 9:
                costs = new int[] { 52, 52, 308, 76, 76, 460 };
                break;
            default:
                costs = new int[] { 0, 0, 0, 0, 0, 0 };
                break;
        }
        return costs;
    }

    /// <summary>
    /// This is a gross lookup table implemented in code because it lets us do
    /// balancing tweaks with more precision than an actual algorithmic approach.
    /// also I think the economy seems kinda busted right now, lol - level^2 isn't near what the resource-gain buildings demand for upgrades
    /// </summary>
    public static int[] GetUpgradeCost_WizardsTower(int level)
    {
        int[] costs; // clay, wood, ore, brick, plank, metal
        switch (level)
        {
            case 0:
                costs = new int[] { 0, 0, 0, 0, 0, 0 };
                break;
            case 1:
                costs = new int[] { 0, 0, 0, 1, 1, 1 };
                break;
            case 2:
                costs = new int[] { 0, 0, 0, 4, 4, 4 };
                break;
            case 3:
                costs = new int[] { 0, 0, 0, 9, 9, 9 };
                break;
            case 4:
                costs = new int[] { 0, 0, 0, 16, 16, 16 };
                break;
            case 5:
                costs = new int[] { 0, 0, 0, 25, 25, 25 };
                break;
            case 6:
                costs = new int[] { 0, 0, 0, 36, 36, 36 };
                break;
            case 7:
                costs = new int[] { 0, 0, 0, 49, 49, 49 };
                break;
            case 8:
                costs = new int[] { 0, 0, 0, 64, 64, 64 };
                break;
            case 9:
                costs = new int[] { 0, 0, 0, 81, 81, 81 };
                break;
            default:
                costs = new int[] { 0, 0, 0, 0, 0, 0 };
                break;
        }
        return costs;
    }

    public void OpenPopupOnBuilding ()
    {
        associatedPopups[buildingStateIndex].Open();
        if (buildingType == BuildingType.House)
        {
            if (housePopup.gameObject.activeInHierarchy) housePopup.associatedHouse = this;
            else if (buildHousePopup.gameObject.activeInHierarchy) buildHousePopup.associatedHouse = this;
        }
    }

    void RefreshBuildingAssociations ()
    {
        switch (buildingType)
        {
            case BuildingType.House:
                if (GameDataManager.Instance.houseAdventurers[nonUniqueBuildingsIndex] != associatedAdventurer)
                {
                    if (GameDataManager.Instance.houseAdventurers[nonUniqueBuildingsIndex] == null) GameDataManager.Instance.houseAdventurers[nonUniqueBuildingsIndex] = associatedAdventurer;
                    else associatedAdventurer = GameDataManager.Instance.houseAdventurers[nonUniqueBuildingsIndex];
                }
                if (GameDataManager.Instance.housesBuilt[nonUniqueBuildingsIndex] == true && isUndeveloped)
                {
                    isUndeveloped = false;
                    buildingStateIndex = 0;
                }
                if (hasOutbuilding == false && GameDataManager.Instance.housesOutbuildingsBuilt[nonUniqueBuildingsIndex])
                {
                    hasOutbuilding = true;
                    buildingStateIndex = 1;
                }
                if (buildingStateIndex == (int)BuildingStates_House.Foundation)
                {
                    buildingMessage.text = "";
                }
                else
                {
                    buildingMessage.text = associatedAdventurer.fullTitle;
                }
                break;
        }
        spriteRenderer.sprite = buildingSprites[buildingStateIndex];
    }
}
