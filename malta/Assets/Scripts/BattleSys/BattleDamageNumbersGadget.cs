using UnityEngine;
using UnityEngine.UI;

public class BattleDamageNumbersGadget : MonoBehaviour
{
    public Text uiText;
    public Direction direction;
    public float moveDist;
    public float lifespan;
    private float timeAlive;
    public Color colorDmg;
    public Color colorHeal;
    private Vector3 originalPosition;
    private int dmg;
    private bool triggeredGadget = false;


	// Use this for initialization
	void Start ()
    {
        originalPosition = transform.position;
        uiText.enabled = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (triggeredGadget)
        {
            timeAlive += Time.deltaTime;
            switch (direction)
            {
                case Direction.Down:
                    transform.position = transform.position + (Vector3.down * moveDist * Time.deltaTime);
                    break;
                case Direction.DownLeft:
                    transform.position = transform.position + (((Vector3.down + Vector3.left) / 2) * moveDist * Time.deltaTime);
                    break;
                case Direction.DownRight:
                    transform.position = transform.position + (((Vector3.down + Vector3.right) / 2) * moveDist * Time.deltaTime);
                    break;
                case Direction.Up:
                    transform.position = transform.position + (Vector3.up * moveDist * Time.deltaTime);
                    break;
                case Direction.UpLeft:
                    transform.position = transform.position + (((Vector3.up + Vector3.left) / 2) * moveDist * Time.deltaTime);
                    break;
                case Direction.UpRight:
                    transform.position = transform.position + (((Vector3.up + Vector3.right) / 2) * moveDist * Time.deltaTime);
                    break;
                case Direction.Left:
                    transform.position = transform.position + (Vector3.left * moveDist * Time.deltaTime);
                    break;
                case Direction.Right:
                    transform.position = transform.position + (Vector3.right * moveDist * Time.deltaTime);
                    break;
            }
            Color c;
            if (dmg < 0) c = colorHeal;
            else c = colorDmg;
            uiText.color = Color.Lerp(c, Color.clear, timeAlive / lifespan);
            if (timeAlive > lifespan)
            {
                uiText.enabled = false;
                triggeredGadget = false;
            }
        }
	}

    public void Trigger(int _dmg)
    {
        dmg = _dmg;
        if (dmg == 0) return;
        uiText.enabled = true;
        uiText.text = dmg.ToString();
        Color c;
        if (dmg < 0)
        {
            c = colorHeal;
            uiText.text = (-1 * dmg).ToString();
        }
        else
        {
            c = colorDmg;
            uiText.text = dmg.ToString();
        }
        uiText.color = c;
        transform.position = originalPosition;
        timeAlive = 0;
        triggeredGadget = true;
    } 
}
