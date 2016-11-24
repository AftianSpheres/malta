using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoadManager : Manager<LevelLoadManager>
{
    public static int[] sceneIDs =
    {
        1, // title
        2, // town
        3, // overworld
        4, // fight
    };

    public void EnterLevel (int level, int[] roomCoords, int EntryPointIndex, Direction playerInitialFacingDir)
    {
        int index = sceneIDs[level];
        StartCoroutine(_in_EnterLevel(index, roomCoords, EntryPointIndex, playerInitialFacingDir));
    }

    IEnumerator _in_EnterLevel(int index, int[] roomCoords, int EntryPointIndex, Direction playerInitialFacingDir)
    {
        AsyncOperation loading = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        while (loading.isDone == false)
        {
            yield return null;
        }
        Debug.Log("Loaded scene: " + SceneManager.GetActiveScene().name);
    }
}
