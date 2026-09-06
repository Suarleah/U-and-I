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
using UnityEngine.UI;


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
    public readonly SyncList<ItemInstance> items = new SyncList<ItemInstance>();
    public List<ItemInstance> localItems = new List<ItemInstance>();

    public ItemInstance selectedItem;
    public int selectedSlot; //player currently held Item

    [Header("Visual")]

    public Canvas inventoryVisual;
    public Image[] itemIcons;
    public Image[] selectIcons;



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

        UpdateSelectIcon(0);
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

    private void OnInventoryChange(SyncListOperation op, int index, ItemInstance oldItem, ItemInstance newItem, bool asServer)
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
            }
            else
            {
                localItems[i] = items[i];
            }

        }
        selectedItem = items[selectedSlot];
    }

    public void LeftClick(InputAction.CallbackContext c)
    {
        if (selectedSlot >= items.Count || items[selectedSlot] == null) return;
        UseOnServer(selectedSlot, new UseInfo(FishNet.InstanceFinder.ClientManager.Connection.FirstObject, this, selectedSlot));
    }

    [ServerRpc(RequireOwnership = false)]
    public void UseOnServer(int slot, UseInfo info) => UseItem(slot, info);

    [Server]
    private void UseItem(int slot, UseInfo info)
    {
        if (slot < 0 || slot >= items.Count) return;

        ItemInstance instance = items[slot];
        if (instance == null || instance.OnCooldown) return;

        instance.definition.Use(info, instance);

        if (instance.definition.cooldown > 0f)
            instance.StartCooldown(instance.definition.cooldown);

        if (!instance.definition.reusable)
            RemoveItem(slot);
    }

    [Server]
    public bool TryPreventDeath(UseInfo info)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) continue;
            UseInfo slotInfo = new UseInfo(info.user, this, i);
            if (items[i].definition.TryPreventDeath(slotInfo, items[i])) // if youre carrying. seat belt, prevent your death
            {
                if (!items[i].definition.reusable) RemoveItem(i);
                return true;
            }
        }

        return false;
    }

    [Server]
    public void AddItem(int index, ItemInstance item)
    {
        items[index] = item;
        items.Dirty(index);
    }

    [Server]
    public void AddItemToStack(int index, ItemInstance item)
    {
        items[index].count++;
        items.Dirty(index);
    }

    [Server]
    public void RemoveItem(int index) // dropped the redundant `item` param — items[index] is the source of truth
    {
        ItemInstance slot = items[index];
        if (slot == null) return;

        if (slot.definition.stackable && slot.count > 1)
        {
            slot.count--;
            items.Dirty(index);
        }
        else
        {
            items[index] = null;
            items.Dirty(index);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryPickUpItem(ItemSO item, NetworkObject caller)
    {
        if (!caller || !caller.IsSpawned) return;

        if (item.stackable)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].definition == item && items[i].count < item.stackMax)
                {
                    AddItemToStack(i, items[i]);
                    Despawn(caller);
                    return;
                }
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                AddItem(i, new ItemInstance(item));
                Despawn(caller);
                return;
            }
        }
        // inventory full
    }

    public void DropItem(InputAction.CallbackContext c) => TryDropItem(selectedSlot);

    [ServerRpc(RequireOwnership = false)]
    public void TryDropItem(int index)
    {
        ItemInstance slot = items[index];
        if (slot == null || slot.definition.cursed) return;

        SpawnItemInteractable(slot);
        RemoveItem(index);
    }

    [Server]
    public void SpawnItemInteractable(ItemInstance item)
    {
        GameObject itemInstance = Instantiate(item.definition.itemInteractablePrefab);
        itemInstance.transform.position = gameObject.GetComponentInParent<PlayerMovement>().transform.position;
        base.ServerManager.Spawn(itemInstance);
    }

    [Server]
    public void DropAll()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null) continue;
            int stackCount = items[i].definition.stackable ? items[i].count : 1; // bonus fix: cache before it shrinks
            for (int j = 0; j < stackCount; j++)
                TryDropItem(i);
        }
    }

    [Server]
    public bool Contains(int id)
    {
        for (int i = 0; i < invSize; i++)
        {
            if (items[i] != null && items[i].definition.id == id) // bonus fix: original NRE'd on empty slots
                return true;
        }
        return false;
    }

    public void ScrollWheel(InputAction.CallbackContext c)
    {
        foreach (Image i in selectIcons)
        {
            i.enabled = false;
        }


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

        UpdateSelectIcon(selectedSlot);
        //Debug.Log(selectedSlot);
    }

    public void SelectSlotOne(InputAction.CallbackContext c)
    {
        selectedSlot = 0;
        selectedItem = items[0];
        UpdateSelectIcon(0);
    }

    public void SelectSlotTwo(InputAction.CallbackContext c)
    {
        selectedSlot = 1;
        selectedItem = items[1];
        UpdateSelectIcon(1);
    }

    public void SelectSlotThree(InputAction.CallbackContext c)
    {
        selectedSlot = 2;
        selectedItem = items[2];
        UpdateSelectIcon(2);
    }

    public void SelectSlotFour(InputAction.CallbackContext c)
    {
        selectedSlot = 3;
        selectedItem = items[3];
        UpdateSelectIcon(3);
    }

    private void UpdateSelectIcon(int slot) // Visual
    {
        foreach (Image i in selectIcons)
        {
            i.enabled = false;
        }

        if (slot >= 0 && slot < selectIcons.Length)
        {
            selectIcons[slot].enabled = true;
        }
    }
}
