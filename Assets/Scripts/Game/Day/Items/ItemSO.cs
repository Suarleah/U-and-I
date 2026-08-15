using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;


//fix this later, but the server code cant be called on a scriptable object since it doesnt extend a network object
[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    public int id;

    public bool stackable;
    public int stackMax;
    public int count; //if stackable, this is how many there currently are in the stack 
    public bool reusable;

    public bool cursed;

    public GameObject itemInteractablePrefab;

    public GameObject inventoryVisual; //theres a better /more concise way to do this but since no assets in imma just do this

    //These must only be used on Server
    public bool onCD;
    public float cooldown;



    // This method must only be called on server, cannot make it a serverrpc because it is a scriptable object not a networkobject
    [Server]
    public virtual void TryUse(UseInfo info)
    {
        if (info.userInv) //if the user has an inventory manager
        {
            Debug.Log("InvMan");
            if (info.userInv.items[info.usedSlot].id == this.id) //check if the item in the slot is still the right one
            {
                Debug.Log("usedSlot");
                if (!onCD) //check if the item is on cooldown
                {
                    Debug.Log("onCD");
                    Use(info);
                    if (!reusable)
                    {
                        info.userInv.RemoveItem(info.usedSlot, this);
                    }
                }
                
            }
        }
    }

    [Server]
    public virtual void Use(UseInfo info)
    {
        Debug.Log("Test Item Used!");
        
    }

    //all of these methods must only be called on server
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
        onCD = val;
    }


    
}
