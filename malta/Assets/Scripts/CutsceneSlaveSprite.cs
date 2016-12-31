using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MovementEffects;

public class CutsceneSlaveSprite : MonoBehaviour
{
    public CutscenePlayer master;
    public Image image;
    public string myTag;
    private float lifetime;
    private Vector3 anchorPos;
    private Vector3 originalScale;
    private string lastHandle;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void Update ()
    {
        if (lifetime < 0.0f) Kill();
        else lifetime -= Time.deltaTime;
	}

    public void AnchorAt(Vector3 pos)
    {
        transform.position = anchorPos = pos;
    }

    public void Birth (string handle, float t, Sprite s, Vector3 pos)
    {
        Birth(handle, t, s, pos, Color.white);
    }

    public void Birth (string handle, float t, Sprite s, Vector3 pos, Color c)
    {
        gameObject.SetActive(true);
        master.activeSlaves.Add(handle, this);
        lastHandle = handle;
        lifetime = t;
        DrawAt(s, pos, c);
    }

    public void ChangeGraphic (Sprite s)
    {
        ChangeGraphic(s, Color.white);
    }

    public void ChangeGraphic (Sprite s, Color c)
    {
        image.sprite = s;
        image.color = c;
    }

    public void DrawAt (Sprite s, Vector3 pos)
    {
        DrawAt(s, pos, Color.white);
    }

    public void DrawAt (Sprite s, Vector3 pos, Color c)
    {
        ChangeGraphic(s, c);
        transform.localScale = originalScale;
        AnchorAt(pos);
    }

    public void Kill ()
    {
        Timing.KillCoroutines(myTag);
        master.activeSlaves.Remove(lastHandle);
        master.slaveSprites.Enqueue(this);
        gameObject.SetActive(false);
    }

    IEnumerator<float> _MoveOverTime(Vector3 moveVector, Vector3 speed, float time)
    {
        float elapsedTime = 0;
        Vector3 modifiedMV = new Vector3(moveVector.x * speed.x, moveVector.y * speed.y, moveVector.z * speed.z);
        while (elapsedTime < time)
        {
            transform.position += modifiedMV * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return 0f;
        }
    }

    public void MoveOverTime (Vector3 moveVector, Vector3 speed, float time)
    {
        Timing.RunCoroutine(_MoveOverTime(moveVector, speed, time), myTag);
    }

    public void MoveInstantly (Vector3 newLocalPos)
    {
        transform.localPosition = newLocalPos;
    }

    public void ResetToAnchorPos ()
    {
        transform.position = anchorPos;
    }

    IEnumerator<float> _ScaleOverTime(Vector3 scaleMod, float time)
    {
        float elapsedTime = 0;
        Vector3 lastScale = transform.localScale;
        while (elapsedTime < time)
        {
            transform.localScale = new Vector3(lastScale.x * scaleMod.x * (time / elapsedTime), lastScale.y * scaleMod.y * (time / elapsedTime), lastScale.z * scaleMod.z * (time / elapsedTime));
            elapsedTime += Time.deltaTime;
            yield return 0f;
        }
    }

    public void ScaleOverTime (Vector3 scaleMod, float time)
    {
        Timing.RunCoroutine(_ScaleOverTime(scaleMod, time), myTag);
    }

}
