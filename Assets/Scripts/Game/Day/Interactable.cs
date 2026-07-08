using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using System.Collections;
using FishNet.Object.Synchronizing;
using System.Globalization;
using FishNet.Connection;

public class Interactable : NetworkBehaviour
{
    public Canvas c; //own canvas

    //[SerializeField] GameObject spawnedObject; // just a temporary thing for testing
    public bool closest = false; // if its the closest interactable to the player

    public InputActionAsset inputAsset;

    public GameObject player;
    InputAction interactAction; // right now this is the E key

    public bool interacting; // particularly for opening menus, only 1 interactable should be interacting at a time, but any signal given 

    public bool deadUI; // if this UI is meant to be accessed while dead or alive.

    public readonly SyncVar<bool> onCD = new SyncVar<bool>();
    public bool localOnCD;
    public float cooldown = 0;

    public int floor; //the floor this interactable is currently on

    public virtual void Awake()
    {
        interactAction = inputAsset.FindAction("Interact");
        player = FishNet.InstanceFinder.ClientManager.Connection.FirstObject.gameObject;

        onCD.OnChange += OnCDChanged;
        interactAction.performed += released;
        closest = false;
    }

    void OnDestroy()
    {
        interactAction.performed -= released;
    }
    // Update is called once per frame
    void Update()
    {
        localOnCD = onCD.Value;
        if (closest) 
        {
            c.gameObject.SetActive(true);
            if (Vector2.Distance(gameObject.transform.position, player.transform.position) > 5f)
            {
                closest = false;
            }

        } else
        {
            c.gameObject.SetActive(false);
            
        }

        

    }

    public virtual void released(InputAction.CallbackContext c)
    {
        if (closest && !UIManager.Instance.currentInteraction) // cant overlap the patient overlays
        {
            //Debug.Log("closest = " + closest + "\n entity id: " + gameObject.GetEntityId());
            Interact();
            //Debug.Log("interacted!");
            //GameObject netObj = Instantiate(spawnedObject, gameObject.transform.position, gameObject.transform.rotation);
            //FishNet.InstanceFinder.ServerManager.Spawn(netObj);

        } else if (UIManager.Instance.currentInteraction) //pressing interact in the interaction menu closes it. Idk if its stupid or not to have this keybind
        {
            if (UIManager.Instance.currentInteraction == this)
            {
                Close();
            }
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
        if (UIManager.Instance.currentInteraction == this) //pressing interact in the interaction menu closes it. Idk if its stupid or not to have this keybind
        {
            closest = false;
            interacting = false;
            UIManager.Instance.currentInteraction = null;
        }
    }

    [ObserversRpc]
    public virtual void CloseAllClients()
    {
        Close();
    }

    public virtual void UIButtonPressed(PatientInteractionInfo info) //when a UI button is pressed, this can be called. 
    {

    }

    
    [Server]
    public virtual IEnumerator goOnCooldown(float seconds)
    {
        setCD(true);

        yield return new WaitForSeconds(seconds);

        setCD(false);
    }
    
    [Server]
    public virtual void setCD(bool val)
    {
        onCD.Value = val;
    }

    public void OnCDChanged(bool prev, bool next, bool asServer)
    {
        if (next) // if its going on cooldown
        {
            Close();
        }
    }
    
}
