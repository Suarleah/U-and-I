using FishNet.Object;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class ShopZone : MonoBehaviour
{

    public ItemSO item;
    public int cost = 5;
    public Image fillBox;
    public bool endZone;

    private ShopManager shopManager;

    async void Start()
    {
        shopManager = ShopManager.Instance;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            if (!endZone)
            {
            InventoryManager inv = collision.GetComponentInChildren<InventoryManager>();
            
            inv.TryPickUpItem(item, collision.GetComponent<NetworkObject>());
            }
            else
            {
                shopManager.PlayerEnterReadyZone();
            }

        }
            
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            if (!endZone)
            {
                
            } 
            else
            {
                shopManager.PlayerLeftReadyZone();
            }
            
        }
    }
}
