using UnityEngine;
using FishNet;
using FishNet.Object;

public class UIManager : MonoBehaviour
{
    public GameObject player;
    public Interactable currentInteraction; //the interactable that is currently being interacted with, if any (mostly because interactables can be spawned in, the UImanager must keep track of this)

    public static UIManager Instance;

    public Canvas PatientInteractionCanvas;
    public Canvas MinimapCanvas;
    public PatientInteractable interactingPatient; //if its currently interactign with a patient, this is the one



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
        ProximityUICheck();
            
    }

    public void Close()
    {
        if (currentInteraction)
        {
            currentInteraction.Close();
        }
    }

    
    public void UIButtonPressed(PatientInteractionInfo info) //info is usually just the button name, 
    {
        if (currentInteraction)
        {
            currentInteraction.UIButtonPressed(info);
        } else if (interactingPatient)
        {   
            interactingPatient.UIButtonPressed(info);
        }
        
    }


    //some objects have UI popups when you're near them. This checks nearby objects and displays the UI of the nearest one within a certain range.
    void ProximityUICheck()
    {
        LayerMask mask;
        if (player.GetComponent<PlayerStats>().isDead.Value) //some interactables can only be interacted with when alliv  or dead, note clock out can always be interacted with
        {
            mask = LayerMask.GetMask("DeadInteractable") + LayerMask.GetMask("ClockOut");
        } else
        {
            mask = LayerMask.GetMask("AliveInteractable") + LayerMask.GetMask("ItemInteractable") + LayerMask.GetMask("ClockOut");
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.transform.position, 5f, mask);
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
                if (!colliders[i].gameObject.GetComponent<Interactable>().enabled || colliders[i].gameObject.GetComponent<Interactable>().onCD.Value)
                {
                    colliders[i].gameObject.GetComponent<Interactable>().closest = false;
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

        
        PatientInteractionCanvas.enabled = false;
        interactingPatient = null;
        if (curobj) //if an interactable is in range and the closest, and the player isnt already interacting with something else
        {
            /*if (currentInteraction)
            {
                curobj.GetComponent<Interactable>().closest = false;
            } else
            {
                curobj.GetComponent<Interactable>().closest = true;
            }*/
            curobj.GetComponent<Interactable>().closest = true;
            if (curobj.GetComponent<PatientInteractable>()){
                PatientInteractionCanvas.enabled = true;
                interactingPatient = curobj.GetComponent<PatientInteractable>();
            }
        } 
    }
}
