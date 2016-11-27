using UnityEngine;
using System.Collections;

public class PalacePopup : MonoBehaviour
{
    public TownBuilding[] houses;
    public PopupMenu shell;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OpenDocksPopup ()
    {
        Debug.Log("Not yet implemented");
    }

    public void OpenMasonPopup ()
    {
        Debug.Log("Not yet implemented");
    }

    public void OpenSawmillPopup ()
    {
        Debug.Log("Not yet implemented");
    }

    public void OpenSmithPopup ()
    {
        Debug.Log("Not yet implemented");
    }

    public void OpenClayPitPopup ()
    {
        Debug.Log("ffs we don't even have a peninsular map scene yet");
    }

    public void OpenMinesPopup()
    {
        Debug.Log("ffs we don't even have a peninsular map scene yet");
    }

    public void OpenWoodlandsPopup()
    {
        Debug.Log("ffs we don't even have a peninsular map scene yet");
    }

    public void OpenHousePopup (int index)
    {
        shell.Close();
        houses[index].OpenPopupOnBuilding();
    }
}
