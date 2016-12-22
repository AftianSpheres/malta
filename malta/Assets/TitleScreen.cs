using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour
{

    public void NewGameButtonInteraction ()
    {
        LevelLoadManager.Instance.EnterLevel(SceneIDType.OverworldScene);
    }

    public void ExitButtonInteraction ()
    {
        Application.Quit();
        if (Application.isEditor) Debug.Log("would quit if we weren't in editor");
    }
}
