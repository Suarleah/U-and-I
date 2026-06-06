using FishNet.Object;
using UnityEngine;

public class UseInfo //information pased to an itemSO when its used
{
    public UseInfo(NetworkObject u, InventoryManager uInv, int uSlot)
    {
        user = u;
        userInv = uInv;
        usedSlot = uSlot;
    }

    public NetworkObject user;
    public InventoryManager userInv;
    public int usedSlot; //the slot the item was in when it was used

}
