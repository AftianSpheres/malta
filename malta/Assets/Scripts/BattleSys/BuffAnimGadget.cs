using UnityEngine;
using UnityEngine.UI;

public class BuffAnimGadget : MonoBehaviour
{
    public Image uiImage;
    public float lifespan;
    public AudioClip clip;
    public AudioSource audioSource;
    private float timeAlive;
    private Vector3 originalScale;
    public bool triggeredGadget = false;

    // Use this for initialization
    void Start()
    {
        originalScale = transform.localScale;
        uiImage.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (triggeredGadget)
        {
            timeAlive += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 2, timeAlive);
            if (timeAlive > lifespan)
            {
                uiImage.enabled = false;
                triggeredGadget = false;
            }
        }
    }

    public void Trigger ()
    {
        audioSource.PlayOneShot(clip);
        uiImage.enabled = true;
        transform.localScale = originalScale;
        timeAlive = 0;
        triggeredGadget = true;
    }
}
