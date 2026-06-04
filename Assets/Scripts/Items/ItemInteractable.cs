using FishNet.Object;
using UnityEngine;

public class ItemInteractable : Interactable
{
    public ItemSO itemSO;
    

    public override void Interact()
    {
        InventoryManager inv = player.GetComponentInChildren<InventoryManager>();
        TryPickUpItem(itemSO, inv); 
    }

    [ServerRpc(RequireOwnership = false)] 
    public void TryPickUpItem(ItemSO item, InventoryManager inv) //if this runs, the object will be destroyed, so it cant be called anymore from server (this is the validity check)
    {
        //get current client's item inventory
        ItemSO[] items = inv.items.Value;

        //check for stackable items, if theres a stack they can stack onto
        if (item.stackable)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i])
                {
                    if (items[i].inventoryItemPrefab == item.inventoryItemPrefab && items[i].count < items[i].stackMax)
                    {
                        inv.AddItemToStack(i, item);
                        Despawn(gameObject);
                        return;
                    }
                }
                
            }
        }
        //otherwise, check for first open spot
        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i])
            {
                inv.AddItem(i, item);
                Despawn(gameObject);
                return;
            }
        }

        //inventory full
    }


    
}
