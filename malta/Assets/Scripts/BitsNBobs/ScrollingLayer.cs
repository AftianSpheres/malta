using UnityEngine;
using System.Collections;

public class ScrollingLayer : MonoBehaviour
{
    public bool alternateScrollingRatePossible;
    public int alternateScrollMulti = 1;
    public int alternateScrollOdds = 0;
    public int loopDistance;
    public int scrollInterval;
    public int scrollingSpeed;
    public Vector3 scrollVector;
    public Vector2 alternateScrollMultiVector;
    public AudioSource associatedAudioSource;
    private bool useAlternateScrolling = false;
    private int ctr;
    private int i;
    private Vector3 originalTransform;

    void Start()
    {
        originalTransform = transform.position;
    }

	// Update is called once per frame
	void Update ()
    {
        if (ctr >= scrollInterval)
        {
            ctr = 0;
            i += scrollingSpeed;
            if (i >= loopDistance)
            {
                ResetScrolling();
            }
            else
            {
                transform.position += scrollVector * scrollingSpeed;
            }
        }
        else
        {
            ctr++;
        }
    }

    void ResetScrolling ()
    {
        transform.position = originalTransform;
        i = 0;
        bool switchThisTime = false;
        if (Random.Range(0, alternateScrollOdds) == 0) 
        {
            useAlternateScrolling = !useAlternateScrolling;
            switchThisTime = true;
        }
        if (alternateScrollingRatePossible == true && switchThisTime == true)
        {
            if (useAlternateScrolling == true)
            {
                scrollingSpeed *= alternateScrollMulti;
                scrollVector = new Vector3(scrollVector.x * alternateScrollMultiVector.x, scrollVector.y * alternateScrollMultiVector.y, scrollVector.z);
                loopDistance *= alternateScrollMulti;
                if (associatedAudioSource != null)
                {
                    associatedAudioSource.pitch *= alternateScrollMulti;
                }
            }
            else
            {
                scrollingSpeed /= alternateScrollMulti;
                scrollVector = new Vector3(scrollVector.x / alternateScrollMultiVector.x, scrollVector.y / alternateScrollMultiVector.y, scrollVector.z);
                loopDistance /= alternateScrollMulti;
                if (associatedAudioSource != null)
                {
                    associatedAudioSource.pitch /= alternateScrollMulti;
                }
            }
        }
    }
}
