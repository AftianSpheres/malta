using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Another dumb "gadget" class.
/// </summary>
public class MyTownLabeler : MonoBehaviour
{
    public TextMesh textMesh;
    public Text uiText;
	
	// Update is called once per frame
	void Update ()
    {
	    if (GameDataManager.Instance != null)
        {
            if (textMesh != null && textMesh.text != GameDataManager.Instance.yourTownName) textMesh.text = GameDataManager.Instance.yourTownName;
            if (uiText != null && uiText.text != GameDataManager.Instance.yourTownName) uiText.text = GameDataManager.Instance.yourTownName;
        }
	}
}
