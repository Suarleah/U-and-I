using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class ItemInteractable : Interactable
{
    public ItemSO itemSO;
    

    public override void Interact()
    {
        InventoryManager inv = player.GetComponentInChildren<InventoryManager>();
        inv.TryPickUpItem(itemSO, base.NetworkObject); 
    }

    


    
}
