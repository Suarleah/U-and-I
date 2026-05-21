using UnityEngine;
using FishNet;

public class UiManager : MonoBehaviour
{
    public GameObject player;
    //public Canvas c;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = FishNet.InstanceFinder.ClientManager.Connection.FirstObject.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        ProximityUICheck();
    }
   

    //some objects have UI popups when you're near them. This checks nearby objects and displays the UI of the nearest one within a certain range.
    void ProximityUICheck()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, 5f);
        if (colliders.Length <= 0)
        {
            return; 
        }
        float mindist = float.MaxValue; 
        GameObject curobj = null; //current closest interactable object
        

        for (int i = 0; i < colliders.Length; i++) //find closest
        {
            if (colliders[i].gameObject.GetComponent<Interactable>())
            {
                if (Vector2.Distance(colliders[i].transform.position, player.transform.position)< mindist)
                {
                    if (curobj)
                    {
                        curobj.GetComponent<Interactable>().closest = false; //set previous closest to false
                    }
                    
                    curobj = colliders[i].gameObject;
                    mindist = Vector2.Distance(colliders[i].transform.position, player.transform.position);
                } else
                {
                    // if its within range, but not the closest, set closest to false
                    colliders[i].gameObject.GetComponent<Interactable>().closest = false;
                }

            }
        }

        if (curobj) //if an interactable is in range and the closest
        {
            curobj.GetComponent<Interactable>().closest = true;
        } 
    }
}
