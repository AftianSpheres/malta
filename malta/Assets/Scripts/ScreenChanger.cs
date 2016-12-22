using UnityEngine;
using System.Collections;

public enum SceneChanegAnimType
{
    None,
    ZoomIn,
    ZoomOut
}

public class ScreenChanger : MonoBehaviour
{
    public float animLength;
    public float zoomDistance;
    public SceneIDType targetSceneID;
    public Camera mainCamera;
    public AudioSource bgmSource;
    public AudioClip whooshySoundOrSomething;
    public SceneChanegAnimType sceneChangeAnim;
    public bool allowClickActivate = true;

    private void OnMouseDown()
    {
        if (!GameStateManager.Instance.somethingHasFocus && allowClickActivate) Activate();
    }

    public void Activate ()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
            if (whooshySoundOrSomething != null) bgmSource.PlayOneShot(whooshySoundOrSomething);
        }
        switch (sceneChangeAnim)
        {
            case SceneChanegAnimType.None:
                LevelLoadManager.Instance.EnterLevel(targetSceneID);
                break;
            case SceneChanegAnimType.ZoomIn:
                StartCoroutine(anim_Zoom(animLength, zoomDistance));
                break;
            case SceneChanegAnimType.ZoomOut:
                StartCoroutine(anim_Zoom(animLength, zoomDistance, true));
                break;
        }
    }

    IEnumerator anim_Zoom (float seconds, float distance, bool zoomOut = false)
    {
        float elapsedSeconds = 0;
        mainCamera.orthographic = false;
        Vector3 originalPos = mainCamera.transform.position;
        Vector3 finalPos;
        if (zoomOut)
        {
            finalPos = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z  - 1 * distance);
        }
        else
        {
            finalPos = new Vector3(transform.position.x, transform.position.y, mainCamera.transform.position.z + 1 * distance);
        }
        while (seconds > elapsedSeconds)
        {
            mainCamera.transform.position = Vector3.Lerp(originalPos, finalPos, (elapsedSeconds / seconds));
            elapsedSeconds += Time.deltaTime;
            yield return null;
        }
        mainCamera.orthographic = true;
        LevelLoadManager.Instance.EnterLevel(targetSceneID);
    }

}
