using UnityEngine;
using System.Collections;

/// <summary>
/// Handles basic data management for town buildings.
/// To do: serialization/saving/loading
/// </summary>
public class TownBuilding : MonoBehaviour
{
    private static int[] buildingTypeMaxLevels = { -1, 1, 1, 10, 10, 10, 1, 10 };
    public PopupMenu associatedPopup;
    public BuildingType buildingType;
    public Adventurer associatedAdventurer;
    new public BoxCollider2D collider;
    public SpriteRenderer spriteRenderer;
    public Sprite outbuiltSprite;
    public TextMesh buildingMessage;
    public bool hasOutbuilding;
    public bool forgeOutbuildingIsKobold;
    private bool buildingAlteredSinceLastUpdate = false;

	// Use this for initialization
	void Start ()
    {
        RefreshBuildingAssociations();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (buildingAlteredSinceLastUpdate)
        {
            RefreshBuildingAssociations(); // doing it like this also lets you mark buildings as "dirty" based on timed events
        }
        if (associatedAdventurer != null && !associatedAdventurer.initialized) associatedAdventurer.Reroll(AdventurerClass.Warrior, AdventurerSpecies.Human, false, new int[]{ 0, 0, 0, 0});
	}

    private void OnMouseDown()
    {
        if (associatedPopup != null && !GameStateManager.Instance.popupHasFocus) associatedPopup.Open();
    }

    public void BuildOutbuilding (bool koboldIfForge = false)
    {
        if (hasOutbuilding) throw new System.Exception("Tried to add outbuilding to building, but it already had one: " + gameObject.name);
        forgeOutbuildingIsKobold = koboldIfForge;
        hasOutbuilding = true;
        spriteRenderer.sprite = outbuiltSprite;
        buildingAlteredSinceLastUpdate = true;
    }

    public void BuildingInteraction ()
    {

    }

    void RefreshBuildingAssociations ()
    {
        switch (buildingType)
        {
            case BuildingType.House:
                buildingMessage.text = associatedAdventurer.fullTitle;
                break;
        }
    }
}
