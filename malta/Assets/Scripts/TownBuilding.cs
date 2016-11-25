using UnityEngine;
using System.Collections;

/// <summary>
/// Handles basic data management for town buildings.
/// To do: serialization/saving/loading
/// </summary>
public class TownBuilding : MonoBehaviour
{
    private static int[] buildingTypeMaxLevels = { -1, 1, 1, 10, 10, 10, 1, 10 };
    public int nonUniqueBuildingsIndex;
    public PopupMenu associatedPopup;
    public BuildingType buildingType;
    public Adventurer associatedAdventurer;
    new public BoxCollider2D collider;
    public SpriteRenderer spriteRenderer;
    public Sprite outbuiltSprite;
    public TextMesh buildingMessage;
    public bool hasOutbuilding;
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
        if (associatedPopup != null && !GameStateManager.Instance.popupHasFocus) associatedPopup.Open();
    }

    public void BuildOutbuilding (bool koboldIfForge = false)
    {
        if (hasOutbuilding) throw new System.Exception("Tried to add outbuilding to building, but it already had one: " + gameObject.name);
        if (buildingType == BuildingType.House) GameDataManager.Instance.housesOutbuildingsBuilt[nonUniqueBuildingsIndex] = true;
        associatedAdventurer.Promote();
        buildingAlteredSinceLastUpdate = true;
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
                if (hasOutbuilding == false && GameDataManager.Instance.housesOutbuildingsBuilt[nonUniqueBuildingsIndex] == true)
                {
                    hasOutbuilding = true;
                    spriteRenderer.sprite = outbuiltSprite;
                }
                buildingMessage.text = associatedAdventurer.fullTitle;
                break;
        }
    }
}
