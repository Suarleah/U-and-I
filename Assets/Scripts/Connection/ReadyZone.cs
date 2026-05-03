using UnityEngine;

public class ReadyZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            ReadyManager.Instance.PlayerEnter();
            Debug.Log("eek");
        }
            
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            ReadyManager.Instance.PlayerExit();
        }
    }
}
