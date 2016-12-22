using UnityEngine;
using System.Collections;

/// <summary>
/// Literally a class that implements functionality for Potemkin villages
/// </summary>
public class FakeTown : MonoBehaviour
{
    public FakeTownPopup fakeTownPopup;
    public int fakeTownsIndex;
    public TextMesh nameLabel;
	
	// Update is called once per frame
	void Update ()
    {
	    if (GameDataManager.Instance != null)
        {
            if (nameLabel.text != GameDataManager.Instance.fakeTownNames[fakeTownsIndex]) nameLabel.text = GameDataManager.Instance.fakeTownNames[fakeTownsIndex];
        }
	}


    private void OnMouseDown()
    {
        if (!GameStateManager.Instance.somethingHasFocus)
        {
            fakeTownPopup.Open(fakeTownsIndex);
        }
    }
}
