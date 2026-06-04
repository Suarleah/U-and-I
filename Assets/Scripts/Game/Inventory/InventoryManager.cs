using FishNet.Object.Synchronizing;
using FishNet.Object;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using FishNet.Component.Prediction;


public class InventoryManager : NetworkBehaviour
{
    public InputActionAsset inputs;
    InputAction dropItem;
    InputAction scrollWheel;
    InputAction leftClick; // it will use the item unless the mouse is hovered over an item slot. If mouse is hovered over an item, itll select that item instead.
    InputAction selectSlotOne;
    InputAction selectSlotTwo;
    InputAction selectSlotThree;
    InputAction selectSlotFour;

    public readonly SyncVar<ItemSO[]> items = new SyncVar<ItemSO[]>();

    public ItemSO[] localItems = new ItemSO[4];
    public int selectedSlot; //player currently held Item
    public ItemSO selectedItem; //player currently held Item

    public void Awake()
    {
        items.Value = new ItemSO[4];
        //set keybinds
        dropItem = inputs.FindAction("Drop");
        scrollWheel = inputs.FindAction("ScrollWheel");
        leftClick = inputs.FindAction("Click"); // it will use the item unless the mouse is hovered over an item slot. If mouse is hovered over an item, itll select that item instead.
        selectSlotOne = inputs.FindAction("SelectInvSlot1");
        selectSlotTwo = inputs.FindAction("SelectInvSlot2");
        selectSlotThree = inputs.FindAction("SelectInvSlot3");
        selectSlotFour = inputs.FindAction("SelectInvSlot4");

        dropItem.canceled += DropItem;
        scrollWheel.performed += ScrollWheel;
    }

    void Update()
    { 
        //localItems = items.Value; //purely for the sake of testing. Delegates dont work on syncvar arrays, they work on synclists, but i tried a synclist and it was really fucked up so i gave up
    }

    public void DropItem(InputAction.CallbackContext c)
    {
        TryDropItem(selectedSlot, selectedItem);
    }

    public void ScrollWheel(InputAction.CallbackContext c)
    {
        
        Vector2 scroll = c.ReadValue<Vector2>();
        //Debug.Log(scroll + "");
        selectedSlot += (int)(scroll.y); 
        if (selectedSlot < 0)
        {
            selectedSlot = 0;
        }
        if (selectedSlot >= items.Value.Length)
        {
            selectedSlot = items.Value.Length - 1;
        }
    }

    



    [ServerRpc(RequireOwnership = false)]
    public void TryDropItem(int index, ItemSO item)
    {
        if (items.Value[index] )
        {
            if (items.Value[index].cursed) //cursed items cant be dropped normally
            {
                return;
            }
            RemoveItem(index, item);
            SpawnItemInteractable(item);
        }
    }


    [Server]
    public void AddItem(int index, ItemSO item) //add item to empty inventory slot
    {
        items.Value[index] = item;
    }

    [Server]
    public void AddItemToStack(int index, ItemSO item) //adds to a stack
    {
        items.Value[index].count++;
    }

    [Server]
    public void RemoveItem(int index, ItemSO item) //removes the item, or one stack of the item
    {
        if (!items.Value[index])
        {
            return;
        }
        if (items.Value[index].stackable)
        {
            
        } else
        {
            Destroy(items.Value[index]);
            items.Value[index] = null;
        }
        
    }

    [Server]
    public void SpawnItemInteractable(ItemSO item) //removes the item, or one stack of the item
    {
        GameObject itemInstance = Instantiate(item.itemInteractablePrefab);
        itemInstance.transform.position = transform.position;//spawns it directly on the player, since inventory manager is attached to the player
        
        base.ServerManager.Spawn(itemInstance);
    }


}
