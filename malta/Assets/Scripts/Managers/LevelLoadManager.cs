using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum SceneIDType
{
    UniverseBootstrapScene,
    TownScene,
    OverworldScene
}

public class LevelLoadManager : Manager<LevelLoadManager>
{
    public TopLevelMenuSystem topLevelMenuSystem;
    public const int sceneID_Overworld = 2;
    public const int sceneID_Town = 1;

    public void EnterLevel (SceneIDType level, BuildingType checkBuildingOnLoad = BuildingType.None, int buildingIndex = 0)
    {
        StartCoroutine(_in_EnterLevel((int)level, checkBuildingOnLoad, buildingIndex));
    }

    IEnumerator _in_EnterLevel(int sceneIndex, BuildingType checkBuildingOnLoad, int buildingIndex)
    {
        topLevelMenuSystem.loadingPopup.Open();
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);
        while (loading.isDone == false)
        {
            yield return null;
        }
        if (checkBuildingOnLoad != BuildingType.None)
        {
            float tlmsTimeout = 1.0f;
            while (topLevelMenuSystem == null)
            {
                tlmsTimeout -= Time.deltaTime;
                if (tlmsTimeout < 0) throw new System.Exception("Tried to enter building on load in scene: " + SceneManager.GetActiveScene().name + " but there isn't a popup menu system in place in that scene!");
                yield return null;
            }
            switch (checkBuildingOnLoad)
            {
                case BuildingType.House:
                    throw new System.Exception("We don't support opening house popups from scene load yet");
                case BuildingType.Forge:
                    topLevelMenuSystem.forgePopup.Open();
                    break;
                case BuildingType.Docks:
                    topLevelMenuSystem.docksPopup.Open();
                    break;
                case BuildingType.Smith:
                    topLevelMenuSystem.smithPopup.Open();
                    break;
                case BuildingType.Sawmill:
                    topLevelMenuSystem.sawmillPopup.Open();
                    break;
                case BuildingType.Palace:
                    topLevelMenuSystem.palacePopup.Open();
                    break;
                case BuildingType.Mason:
                    topLevelMenuSystem.masonPopup.Open();
                    break;
                case BuildingType.Mine:
                    topLevelMenuSystem.minesPopup.Open();
                    break;
                case BuildingType.Woodlands:
                    topLevelMenuSystem.woodlandsPopup.Open();
                    break;
                case BuildingType.ClayPit:
                    topLevelMenuSystem.clayPitPopup.Open();
                    break;
            }
        }
    }
}
