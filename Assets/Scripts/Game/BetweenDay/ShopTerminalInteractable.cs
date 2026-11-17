using UnityEngine;

public class ShopTerminalInteractable : Interactable
{
    void Update()
    {
        localOnCD = onCD.Value;
        if (closest) 
        {
            c.gameObject.SetActive(true);
            if (Vector2.Distance(gameObject.transform.position, player.transform.position) > 5f)
            {
                closest = false;
            }

        } else
        {
            c.gameObject.SetActive(false);
            
        }

        if (Vector3.Distance(player.transform.position, transform.position) > 5f)
        {
            Close();
        }

        

    }
    public override void Interact()
    {
        if (!ShopMenuUI.Instance)
        {
            ShopManager.Instance.shopCanvas.SetActive(true); //the first time, it needs to be called through shop manager since it hasnt awaked yet
            return;
        }
        if (ShopMenuUI.Instance.gameObject.activeSelf)
        {
            CloseShop();
        }
        else
        {
            ShopMenuUI.Instance.gameObject.SetActive(true);
        }
    }

    public override void Close() // called by the base class's own E-to-close handling
    {
        base.Close();
        CloseShop();
    }

    private void CloseShop()
    {
        if (ShopMenuUI.Instance) ShopMenuUI.Instance.gameObject.SetActive(false);
    }
}
