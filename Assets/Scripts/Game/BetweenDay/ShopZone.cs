using FishNet.Object;
using UnityEngine;

public class ShopZone : MonoBehaviour
{

    public ItemSO item;
    public int cost = 5;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            InventoryManager inv = collision.GetComponentInChildren<InventoryManager>();
            
            inv.TryPickUpItem(item, collision.GetComponent<NetworkObject>());
        }
            
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            ReadyManager.Instance.PlayerExit();
        }
    }
}
