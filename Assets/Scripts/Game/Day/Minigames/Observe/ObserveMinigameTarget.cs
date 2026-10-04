using UnityEngine;

//the thing you are supposed to be observing
public class ObserveMinigameUnit : MonoBehaviour
{
    


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "spotlight")
        {
            
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        
    }
}
