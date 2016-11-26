using UnityEngine;
using UnityEngine.UI;

public class BuildingInfoPanel : MonoBehaviour
{
    public BuildingType buildingType;
    public int nonUniqueBuildingsIndex;
    public Text clayResCounter;
    public Text woodResCounter;
    public Text oreResCounter;
    public Text bricksResCounter;
    public Text planksResCounter;
    public Text metalResCounter;
    public Text devStatusLabel;
    public Text devTypeLabel;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    switch (buildingType)
        {
            case BuildingType.Docks:
                break;
        }
	}
}
