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
    public static int[] buildingTypeMaxLevels = { 1, 1, 10, 10, 10, 1, 10, 10, 10, 10, 10, -1 };
    public static int[] houseConstructionCosts = { 0, 0, 0, 10, 10, 10 };
    public static int[] houseOutbuildingCosts = { 0, 0, 0, 20, 20, 20 };
    public int buildingStateIndex;
    public int nonUniqueBuildingsIndex;
    public PopupMenu[] associatedPopups;
    public HousePopup housePopup;
    public BuildingType buildingType;
    new public BoxCollider2D collider;
    public SpriteRenderer spriteRenderer;
    public Sprite[] buildingSprites;
    public TextMesh buildingMessage;
    public bool hasOutbuilding;
    public bool isUndeveloped;
    public bool forgeOutbuildingIsKobold;
    private AdventurerClass _cachedAdvClass;
    private bool buildingAlteredSinceLastUpdate = true;

    // Update is called once per frame
    void Update ()
    {
        if (GameDataManager.Instance != null) // only matters in editor, but prevents silly timing-related crashes
        {  
            if (buildingType != BuildingType.Forest)
            {
                if ((GameDataManager.Instance.HasFlag(ProgressionFlags.Tutorial1_Complete)) != spriteRenderer.enabled) spriteRenderer.enabled = (GameDataManager.Instance.HasFlag(ProgressionFlags.Tutorial1_Complete));
                if (spriteRenderer.enabled) RefreshBuildingAssociations();
            }
            switch (buildingType)
            {
                case BuildingType.Tower:
                    if (GameDataManager.Instance.HasFlag(ProgressionFlags.TowerUnlock) != spriteRenderer.enabled) spriteRenderer.enabled = GameDataManager.Instance.HasFlag(ProgressionFlags.TowerUnlock);
                    if (spriteRenderer.enabled) RefreshBuildingAssociations();
                    break;
            }
            if (buildingAlteredSinceLastUpdate) RefreshBuildingAssociations(); // doing it like this also lets you mark buildings as "dirty" based on timed events
        }
    }

    private void OnMouseDown()
    {
        if (associatedPopups.Length > 0 && associatedPopups[buildingStateIndex] != null && !GameStateManager.Instance.somethingHasFocus && spriteRenderer.enabled)
        {
            OpenPopupOnBuilding();
        }
    }

    public void BuildOutbuilding ()
    {
        if (hasOutbuilding) throw new System.Exception("Tried to add outbuilding to building, but it already had one: " + gameObject.name);
        switch (buildingType)
        {
            case BuildingType.Forge:
                GameDataManager.Instance.SetFlag(ProgressionFlags.TaskmasterUnlock);
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
                costs = new int[] { 2, 2, 2 };
                break;
            case 1:
                costs = new int[] { 4, 4, 4 };
                break;
            case 2:
                costs = new int[] { 8, 8, 8 };
                break;
            case 3:
                costs = new int[] { 16, 16, 16 };
                break;
            case 4:
                costs = new int[] { 32, 32, 32 };
                break;
            case 5:
                costs = new int[] { 64, 64, 64 };
                break;
            case 6:
                costs = new int[] { 128, 128, 128 };
                break;
            case 7:
                costs = new int[] { 256, 256, 256 };
                break;
            case 8:
                costs = new int[] { 512, 512, 512 };
                break;
            case 9:
                costs = new int[] { 1024, 1024, 1024};
                break;
            default:
                costs = new int[] { 0, 0, 0 };
                break;
        }
        return costs;
    }

    /// <summary>
    /// Checks how much a new class upgrade should cost. Second is 3x cost of first.
    /// </summary>
    public static int[] GetUpgradeCost_Forge ()
    {
        int[] costs;
        if (GameDataManager.Instance.dataStore.warriorClassUnlock == AdventurerClass.Warrior && GameDataManager.Instance.dataStore.mysticClassUnlock == AdventurerClass.Mystic)
        {
            costs = new int[] { 10, 10, 10 };
        }
        else if (GameDataManager.Instance.dataStore.warriorClassUnlock == AdventurerClass.Warrior || GameDataManager.Instance.dataStore.mysticClassUnlock == AdventurerClass.Mystic)
        {
            costs = new int[] { 30, 30, 30 };
        }
        else
        {
            costs = new int[] { 90, 90, 90 };
        }
        return costs;
    }

    /// <summary>
    /// This is a gross lookup table implemented in code because it lets us do
    /// balancing tweaks with more precision than an actual algorithmic approach.
    /// </summary>
    public static int[] GetUpgradeCost_House(int level)
    {
        int[] costs; // clay, wood, ore
        switch (level)
        {
            case 0:
                costs = new int[] { 3, 3, 3 };
                break;
            case 1:
                costs = new int[] { 6, 6, 6 };
                break;
            case 2:
                costs = new int[] { 12, 12, 12 };
                break;
            case 3:
                costs = new int[] { 24, 24, 24 };
                break;
            case 4:
                costs = new int[] { 48, 48, 48 };
                break;
            case 5:
                costs = new int[] { 96, 96, 96 };
                break;
            case 6:
                costs = new int[] { 192, 192, 192 };
                break;
            case 7:
                costs = new int[] { 384, 384, 384 };
                break;
            case 8:
                costs = new int[] { 768, 768, 768 };
                break;
            case 9:
                costs = new int[] { 1536, 1536, 1536 };
                break;
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
                costs = new int[] { 3072, 3072, 3072 };
                break;
            case 15:
            case 16:
            case 17:
            case 18:
            case 19:
                costs = new int[] { 6144, 6144, 6144 };
                break;
            default:
                costs = new int[] { 0, 0, 0 };
                break;
        }
        return costs;
    }

    public static int[] GetCapUpCost_MasonClaypit(int level)
    {
        int[] costs = GetUpgradeCost_MasonClaypit(level);
        for (int i = 0; i < costs.Length; i++)
        {
            if (costs[i] > 0) costs[i] = Mathf.CeilToInt(costs[i] * 0.25f);
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
                costs = new int[] { 2, 0, 0 };
                break;
            case 1:
                costs = new int[] { 4, 0, 0 };
                break;
            case 2:
                costs = new int[] { 8, 0, 0 };
                break;
            case 3:
                costs = new int[] { 16, 0, 0 };
                break;
            case 4:
                costs = new int[] { 32, 0, 0 };
                break;
            case 5:
                costs = new int[] { 64, 0, 0 };
                break;
            case 6:
                costs = new int[] { 128, 0, 0 };
                break;
            case 7:
                costs = new int[] { 256, 0, 0 };
                break;
            case 8:
                costs = new int[] { 512, 0, 0};
                break;
            case 9:
                costs = new int[] { 1024, 0, 0 };
                break;
            case 10:
                costs = new int[] { 2048, 0 , 0 };
                break;
            default:
                costs = new int[] { 0, 0, 0 };
                break;
        }
        return costs;
    }

    public static int[] GetCapUpCost_SawmillWoodlands(int level)
    {
        int[] costs = GetUpgradeCost_SawmillWoodlands(level);
        for (int i = 0; i < costs.Length; i++)
        {
            if (costs[i] > 0) costs[i] = Mathf.CeilToInt(costs[i] * 0.25f);
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
                costs = new int[] { 0, 2, 0 };
                break;
            case 1:
                costs = new int[] { 0, 4, 0 };
                break;
            case 2:
                costs = new int[] { 0, 8, 0 };
                break;
            case 3:
                costs = new int[] { 0, 16, 0 };
                break;
            case 4:
                costs = new int[] { 0, 32, 0 };
                break;
            case 5:
                costs = new int[] { 0, 64, 0 };
                break;
            case 6:
                costs = new int[] { 0, 128, 0 };
                break;
            case 7:
                costs = new int[] { 0, 256, 0 };
                break;
            case 8:
                costs = new int[] { 0, 512, 0 };
                break;
            case 9:
                costs = new int[] { 0, 1024, 0 };
                break;
            case 10:
                costs = new int[] { 0, 2048, 0 };
                break;
            default:
                costs = new int[] { 0, 0, 0 };
                break;
        }
        return costs;
    }

    public static int[] GetCapUpCost_SmithMines(int level)
    {
        int[] costs = GetUpgradeCost_SmithMines(level);
        for (int i = 0; i < costs.Length; i++)
        {
            if (costs[i] > 0) costs[i] = Mathf.CeilToInt(costs[i] * 0.25f);
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
                costs = new int[] { 0, 0, 2 };
                break;
            case 1:
                costs = new int[] { 0, 0, 4 };
                break;
            case 2:
                costs = new int[] { 0, 0, 8 };
                break;
            case 3:
                costs = new int[] { 0, 0, 16 };
                break;
            case 4:
                costs = new int[] { 0, 0, 32 };
                break;
            case 5:
                costs = new int[] { 0, 0, 64 };
                break;
            case 6:
                costs = new int[] { 0, 0, 128 };
                break;
            case 7:
                costs = new int[] { 0, 0, 256 };
                break;
            case 8:
                costs = new int[] { 0, 0, 512 };
                break;
            case 9:
                costs = new int[] { 0, 0, 1024 };
                break;
            case 10:
                costs = new int[] { 0, 0, 2048 };
                break;
            default:
                costs = new int[] { 0, 0, 0 };
                break;
        }
        return costs;
    }

    /// <summary>
    /// This is a gross lookup table implemented in code because it lets us do
    /// balancing tweaks with more precision than an actual algorithmic approach.
    /// also I think the economy seems kinda busted right now, lol - level^2 isn't near what the resource-gain buildings demand for upgrades
    /// </summary>
    public static int GetUpgradeCost_WizardsTower(int level)
    {
        int costs;
        switch (level)
        {
            case 0:
                costs = 10;
                break;
            case 1:
                costs = 20;
                break;
            case 2:
                costs = 30;
                break;
            case 3:
                costs = 40;
                break;
            case 4:
                costs = 50;
                break;
            case 5:
                costs = 60;
                break;
            case 6:
                costs = 70;
                break;
            case 7:
                costs = 80;
                break;
            case 8:
                costs = 90;
                break;
            case 9:
                costs = 100;
                break;
            default:
                costs = 0;
                break;
        }
        return costs;
    }

    public void OpenPopupOnBuilding ()
    {
        associatedPopups[buildingStateIndex].Open();
    }

    void RefreshBuildingAssociations ()
    {
        switch (buildingType)
        {
            case BuildingType.Portal:
                if (GameDataManager.Instance.dataStore.buildingLv_WizardsTower < buildingTypeMaxLevels[(int)BuildingType.Tower]) buildingStateIndex = 0;
                else buildingStateIndex = 1;
                break;
        }
        if (buildingSprites.Length > 0)
        {
            spriteRenderer.sprite = buildingSprites[buildingStateIndex];
        }
    }
}
