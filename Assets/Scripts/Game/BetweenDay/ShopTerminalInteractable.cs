using UnityEngine;

public class ShopTerminalInteractable : Interactable
{
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
