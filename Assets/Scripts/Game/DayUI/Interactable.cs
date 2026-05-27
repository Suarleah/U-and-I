using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;
using FishNet.Object;

public class Interactable : MonoBehaviour
{
    [SerializeField]Canvas c; //own canvas
    [SerializeField] GameObject spawnedObject; // just a temporary thing for testing
    public bool closest = false; // if its the closest interactable to the player

    public InputActionAsset inputAsset;

    GameObject player;
    InputAction interactAction; // right now this is the E key


    private void Awake()
    {
        interactAction = inputAsset.FindAction("Interact");
        player = FishNet.InstanceFinder.ClientManager.Connection.FirstObject.gameObject;
        interactAction.canceled += released;
    }

    // Update is called once per frame
    void Update()
    {
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

    public void released(InputAction.CallbackContext c)
    {
        if (closest)
        {
            //Debug.Log("interacted!");
            GameObject netObj = Instantiate(spawnedObject, gameObject.transform.position, gameObject.transform.rotation);
            FishNet.InstanceFinder.ServerManager.Spawn(netObj);
        }
    }
   

    //action when in proximity
    public void proximity()
    {
        c.gameObject.SetActive(true);
    }
        
}
