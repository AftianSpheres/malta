using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoadManager : Manager<LevelLoadManager>
{
    public static int[] sceneIDs =
    {
        1, // town
        2, // overworld
        3, // fight
    };

    public void EnterLevel (int level)
    {
        int index = sceneIDs[level];
        StartCoroutine(_in_EnterLevel(index));
    }

    IEnumerator _in_EnterLevel(int index)
    {
        AsyncOperation loading = SceneManager.LoadSceneAsync(index, LoadSceneMode.Single);
        while (loading.isDone == false)
        {
            yield return null;
        }
    }
}
