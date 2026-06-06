using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;


//fix this later, but the server code cant be called on a scriptable object since it doesnt extend a network object
[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    int id;

    public bool stackable;
    public int stackMax;
    public int count; //if stackable, this is how many there currently are in the stack 
    public bool reusable;

    public bool cursed;

    public GameObject itemInteractablePrefab;

    public GameObject inventoryVisual; //theres a better /more concise way to do this but since no assets in imma just do this

    //public readonly SyncVar<bool> onCD = new SyncVar<bool>();
    public bool localOnCD;
    public float cooldown;


    public virtual void Use(UseInfo info) //the user is going to be a player 99/100 times but just in case I want to make it more flexible
    {
        UseOnServer(info);
    }

    //[ServerRpc(RequireOwnership = false)] 
    public virtual void UseOnServer(UseInfo info)
    {
        if (info.userInv) //if the user has an inventory manager
        {
            if (info.userInv.items[info.usedSlot] == this) //check if the item in the slot is still the right one
            {
                Debug.Log("Test Item Used!");
                info.userInv.RemoveItem(info.usedSlot, this);
            {
                
            }
            }
            
            
        }
    }


    /*//for items which have a cooldown (idk what items)
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
        localOnCD = next;
    }*/
    
}
