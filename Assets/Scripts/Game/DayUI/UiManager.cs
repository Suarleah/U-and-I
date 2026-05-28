using UnityEngine;
using FishNet;
using FishNet.Object;

public class UIManager : MonoBehaviour
{
    public GameObject player;
    public Interactable currentInteraction; //the interactable that is currently being interacted with, if any (mostly because interactables can be spawned in, the UImanager must keep track of this)

    public static UIManager Instance;

    public Canvas PatientInteractionCanvas;



    //public Canvas c;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = FishNet.InstanceFinder.ClientManager.Connection.FirstObject.gameObject;
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.GetComponent<PlayerStats>().isDead.Value)
        {
            ProximityUICheck();
        } else
        {
            //fun dead ui stuff check
        }
            
    }

    
    public void UIButtonPressed(string info) //info is usually just the button name, 
    {
        currentInteraction.UIButtonPressed(info);
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
                if (!colliders[i].gameObject.GetComponent<Interactable>().enabled)
                {
                    continue;
                }
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

        

        if (curobj) //if an interactable is in range and the closest, and the player isnt already interacting with something else
        {
            if (currentInteraction)
            {
                curobj.GetComponent<Interactable>().closest = false;
            } else
            {
                curobj.GetComponent<Interactable>().closest = true;
            }
            
        } 
    }
}
