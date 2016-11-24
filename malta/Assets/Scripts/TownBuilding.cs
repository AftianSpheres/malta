using UnityEngine;
using System.Collections;

/// <summary>
/// Handles basic data management for town buildings.
/// To do: serialization/saving/loading
/// </summary>
public class TownBuilding : MonoBehaviour
{
    private static int[] buildingTypeMaxLevels = { -1, 1, 1, 10, 10, 10, 1, 10 };
    public BuildingType buildingType;
    public Adventurer associatedAdventurer;
    public Rect rect;
    public SpriteRenderer spriteRenderer;
    public Sprite outbuiltSprite;
    public TextMesh buildingMessage;
    public int level;
    public bool hasOutbuilding;
    public bool forgeOutbuildingIsKobold;
    private bool buildingAlteredSinceLastUpdate = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (buildingAlteredSinceLastUpdate) RefreshMessage(); // doing it like this also lets you mark buildings as "dirty" based on timed events
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

    public void LevelAdjust (int levels)
    {
        if (level + levels <= buildingTypeMaxLevels[(int)buildingType] && level + levels > 0) level += levels;
        else if (level < buildingTypeMaxLevels[(int)buildingType] && levels > 0) level = buildingTypeMaxLevels[(int)buildingType];
        else if (level > 1) level = 1;
        else
        {
            throw new System.Exception("Tried to level building to invalid level: " + gameObject.name);
        }
        buildingAlteredSinceLastUpdate = true;
    }

    void RefreshMessage ()
    {
        switch (buildingType)
        {
            case BuildingType.House:
                buildingMessage.text = "YO DO ME MOTHERFUCKER";
                break;
        }
    }
}
