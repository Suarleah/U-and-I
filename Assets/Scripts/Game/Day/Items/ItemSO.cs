using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;


//fix this later, but the server code cant be called on a scriptable object since it doesnt extend a network object
[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public abstract class ItemSO : ScriptableObject
{
    public int id;

    public bool stackable; //if stackable, this is how many there currently are in the stack 
    public int stackMax = 1;

        //These must only be used on Server
    public bool reusable;
    public float cooldown; // seconds, 0 = none
    public bool cursed;

    public GameObject itemInteractablePrefab;
    public GameObject inventoryVisual; //theres a better /more concise way to do this but since no assets in imma just do this

    // Override if the item does something when clicked.
    [Server]
    public virtual void Use(UseInfo info, ItemInstance slot) { }

    // override this if your name is seatbelt so 
    [Server]
    public virtual bool TryPreventDeath(UseInfo info, ItemInstance slot) => false;
}