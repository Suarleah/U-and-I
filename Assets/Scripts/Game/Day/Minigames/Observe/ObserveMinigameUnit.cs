using UnityEngine;

//the thing you are supposed to be observing
public class ObserveMinigameTarget : MonoBehaviour
{



    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "spotlight")
        {
            Debug.Log("enter");
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "spotlight")
        {
            Debug.Log("exit");
        }
    }
}
