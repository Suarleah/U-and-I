using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;
using FishNet.Object;

public class Interactable : NetworkBehaviour
{
    public Canvas c; //own canvas
    public UIManager uimanager;

    //[SerializeField] GameObject spawnedObject; // just a temporary thing for testing
    public bool closest = false; // if its the closest interactable to the player

    public InputActionAsset inputAsset;

    public GameObject player;
    InputAction interactAction; // right now this is the E key

    public bool interacting; // particularly for opening menus, only 1 interactable should be interacting at a time, but any signal given 

    public virtual void Awake()
    {
        interactAction = inputAsset.FindAction("Interact");
        player = FishNet.InstanceFinder.ClientManager.Connection.FirstObject.gameObject;

        interactAction.canceled += released;

        uimanager = FindFirstObjectByType<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (closest && !uimanager.currentInteraction) 
        {
            c.gameObject.SetActive(true);
            

        } else
        {
            c.gameObject.SetActive(false);
            
        }

        if (Vector2.Distance(gameObject.transform.position, player.transform.position) > 5f)
        {
            closest = false;
        }

    }

    public virtual void released(InputAction.CallbackContext c)
    {
        if (closest && !uimanager.currentInteraction) // cant overlap the patient overlays
        {
            Interact();
            //Debug.Log("interacted!");
            //GameObject netObj = Instantiate(spawnedObject, gameObject.transform.position, gameObject.transform.rotation);
            //FishNet.InstanceFinder.ServerManager.Spawn(netObj);

        } else if (uimanager.currentInteraction == this) //pressing interact in the interaction menu closes it. Idk if its stupid or not to have this keybind
        {
            Close();
        }

    }

    public virtual void Interact() //what happens when you actually interact with the object
    {

    }
   

    //action when in proximity
    public virtual void proximity()
    {
        c.gameObject.SetActive(true);
    }

    public void closeInteractionPrompt() //close the little tooltip that says "press E to interact!"
    {
        c.gameObject.SetActive(false);
    }

    public virtual void Close() // close anything that opens up when interacting with this object, ie: patient info
    {
        if (uimanager.currentInteraction == this) //pressing interact in the interaction menu closes it. Idk if its stupid or not to have this keybind
        {
            interacting = false;
            uimanager.currentInteraction = null;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void UIButtonPressed(string info) //when a UI button is pressed, this can be called. 
    {

    }

        
}
