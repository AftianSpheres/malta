using UnityEngine;
using UnityEngine.UI;

public class BattleDamageAnimGadget : MonoBehaviour
{
    public Image uiImage;
    public Vector3 strayRange;
    public float lifespan;
    public AudioClip clip;
    public AudioSource audioSource;
    private float timeAlive;
    private Color originalColor;
    private Vector3 originalPosition;
    public bool triggeredGadget = false;

    // Use this for initialization
    void Start ()
    {
        originalColor = uiImage.color;
        originalPosition = transform.position;
        uiImage.enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (triggeredGadget)
        {
            timeAlive += Time.deltaTime;
            uiImage.color = Color.Lerp(originalColor, Color.clear, timeAlive / lifespan);
            if (timeAlive > lifespan)
            {
                uiImage.enabled = false;
                triggeredGadget = false;
            }
        }
    }

    public void Trigger(int damage)
    {
        if (damage > 0)
        {
            audioSource.PlayOneShot(clip);
            uiImage.enabled = true;
            uiImage.color = originalColor;
            transform.position = originalPosition + new Vector3(Random.Range(-strayRange.x, strayRange.x), Random.Range(-strayRange.y, strayRange.y), transform.position.z);
            timeAlive = 0;
            triggeredGadget = true;
        }

    }
}
