using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour
{
    public GameObject continueButton;
    public GameObject eraseButton;

    void Update()
    {
        if (GameDataManager.Instance != null)
        {
            if (continueButton.activeSelf != GameDataManager.Instance.saveExisted) continueButton.SetActive(GameDataManager.Instance.saveExisted);
            if (eraseButton.activeSelf != GameDataManager.Instance.saveExisted) eraseButton.SetActive(GameDataManager.Instance.saveExisted);
        }
    }

    public void NewGameButtonInteraction ()
    {
        GameDataManager.Instance.RegenerateDataStore();
        LevelLoadManager.Instance.EnterLevel(SceneIDType.OverworldScene);
    }

    public void ContinueButtonInteraction ()
    {
        LevelLoadManager.Instance.EnterLevel(GameDataManager.Instance.dataStore.lastScene);
    }

    public void ExitButtonInteraction ()
    {
        Application.Quit();
        if (Application.isEditor) Debug.Log("would quit if we weren't in editor");
    }
}
