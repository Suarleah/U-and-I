using FishNet.Object.Synchronizing;
using FishNet.Object;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using FishNet.Component.Prediction;
using System.Collections.Generic;
using GameKit.Dependencies.Utilities;
using UnityEngine.SocialPlatforms;
using UnityEngine.Rendering;


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

    public int invSize;
    public readonly SyncList<ItemSO> items = new SyncList<ItemSO>();

    public List<ItemSO> localItems = new List<ItemSO>();
    public int selectedSlot; //player currently held Item
    public ItemSO selectedItem; //player currently held Item

    public void Awake()
    {
        
        //set keybinds
        dropItem = inputs.FindAction("Drop");
        scrollWheel = inputs.FindAction("ScrollWheel");
        leftClick = inputs.FindAction("Click"); // it will use the item unless the mouse is hovered over an item slot. If mouse is hovered over an item, itll select that item instead.
        selectSlotOne = inputs.FindAction("SelectInvSlot1");
        selectSlotTwo = inputs.FindAction("SelectInvSlot2");
        selectSlotThree = inputs.FindAction("SelectInvSlot3");
        selectSlotFour = inputs.FindAction("SelectInvSlot4");

        dropItem.performed += DropItem;
        leftClick.performed += LeftClick;
        scrollWheel.performed += ScrollWheel;
        selectSlotOne.performed += SelectSlotOne;
        selectSlotTwo.performed += SelectSlotTwo;
        selectSlotThree.performed += SelectSlotThree;
        selectSlotFour.performed += SelectSlotFour;


        items.OnChange += OnInventoryChange;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (items.Count >= invSize) //return if inv has already been populated
            return;
        //populate inventory until it's the correct size on server
        
        for (int i = 0; i < invSize; i++)
        {
            items.Add(null);
        }
    }

    private void OnInventoryChange(SyncListOperation op, int index, ItemSO oldItem, ItemSO newItem, bool asServer)
    {
        /*switch (op)
        {
            case SyncListOperation.Add:
                Debug.Log("added to inv!");
                localItems.Add(newItem);
                break;
            case SyncListOperation.Insert:
                // newItem was inserted at [index]
                localItems.Insert(index, newItem);
                break;
            case SyncListOperation.Set:
                localItems[index] = newItem;
                // oldItem was replaced by newItem at [index]
                break;
            case SyncListOperation.RemoveAt:
                localItems.RemoveAt(index);
                // oldItem was removed from [index]
                break;
            case SyncListOperation.Clear:
                localItems.Clear();
                // The list was entirely cleared
                break;
            
        }*/
        for (int i = 0; i < items.Count; i++)
        {
            if (localItems.Count <= i)
            {
                localItems.Add(items[i]);
            } else
            {
                localItems[i] = items[i];
            }
            
        }
        selectedItem = items[selectedSlot];
    }

    public void DropItem(InputAction.CallbackContext c)
    {
        TryDropItem(selectedSlot, selectedItem); // so technically selected item does like nothing but i have it just in case
    }

    public void LeftClick(InputAction.CallbackContext c)
    {
        //first check if its hovered over an inventory slot, if so, select that inventory slot

        //use it from the player
        if (selectedSlot>= items.Count || !items[selectedSlot])
        {
            return;
        }
        
        UseOnServer(items[selectedSlot], new UseInfo(FishNet.InstanceFinder.ClientManager.Connection.FirstObject, this, selectedSlot));
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseOnServer(ItemSO item, UseInfo info)
    {
        Debug.Log("TryUse");
        item.TryUse(info);
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
        if (selectedSlot >= items.Count)
        {
            selectedSlot = items.Count - 1;
        }
        selectedItem = items[selectedSlot];
    }

    public void SelectSlotOne(InputAction.CallbackContext c)
    {
        selectedSlot = 0;
        selectedItem = items[0];
    }

    public void SelectSlotTwo(InputAction.CallbackContext c)
    {
        selectedSlot = 1;
        selectedItem = items[1];
    }
    
    public void SelectSlotThree(InputAction.CallbackContext c)
    {
        selectedSlot = 2;
        selectedItem = items[2];
    }

    public void SelectSlotFour(InputAction.CallbackContext c)
    {
        selectedSlot = 3;
        selectedItem = items[3];
    }


    [ServerRpc(RequireOwnership = false)] 
    public void TryPickUpItem(ItemSO item, NetworkObject caller) //if this runs, the object will be destroyed, so it cant be called anymore from server (this is the validity check)
    {
        if (!caller)//check if the object which called this method exists + is spawned on server
        {
            return;
        } else
        {
            if (!caller.IsSpawned)
            {
                return;
            }
        }

        //check for stackable items, if theres a stack they can stack onto
        if (item.stackable)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i])
                {
                    if (items[i].itemInteractablePrefab == item.itemInteractablePrefab && items[i].count < items[i].stackMax)
                    {
                        AddItemToStack(i, item);
                        Despawn(caller);
                        return;
                    }
                }
                
            }
        }
        //otherwise, check for first open spot
        for (int i = 0; i < items.Count; i++)
        {
            if (!items[i])
            {
                AddItem(i, item);
                Despawn(caller);
                return;
            }
        }
        //inventory full
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryDropItem(int index, ItemSO item)
    {
        if (items[index])
        {
            if (items[index].cursed) //cursed items cant be dropped normally
            {
                return;
            }
            
            SpawnItemInteractable(items[index]); //first spawn item from inventory, then remove it from inventory
            RemoveItem(index, item);
        }
    }


    [Server]
    public void AddItem(int index, ItemSO item) //add item to empty inventory slot
    {
        items[index] = item;
        items.Dirty(index);
    }

    [Server]
    public void AddItemToStack(int index, ItemSO item) //adds to a stack
    {
        items[index].count++;
        items.Dirty(index);
    }

    [Server]
    public void RemoveItem(int index, ItemSO item) //removes the item, or one stack of the item
    {
        if (!items[index])
        {
            return;
        }
        if (items[index].stackable)
        {
            if (items[index].count <= 1)
            {
                items[index] = null;
                items.Dirty(index);
            }
            else
            {
                items[index].count--;
                items.Dirty(index);
            }
        } else
        {
            items[index] = null;
            items.Dirty(index);
        }
        
    }

    [Server]
    public void SpawnItemInteractable(ItemSO item) //removes the item, or one stack of the item
    {
        if (!item)
        {
            return;
        }
        GameObject itemInstance = Instantiate(item.itemInteractablePrefab);
        itemInstance.transform.position = transform.position;//spawns it directly on the player, since inventory manager is attached to the player
        
        base.ServerManager.Spawn(itemInstance);
    }

    [Server]
    public void DropAll()
    {
        for (int i = 0; i < items.Count;i++)
        {
            if (!items[i])
            {
                continue;
            }
            if (items[i].stackable)
            {
                for (int j = 0; j < items[i].count; j++)
                {
                    TryDropItem(i, items[i]);
                }
            } else
            {
                TryDropItem(i, items[i]);
            }
                
        }
    }
}
