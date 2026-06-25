using UnityEngine;

public class SecurityTerminal : Interactable
{


    public override void Interact()
    {
        interacting = true;
        UIManager.Instance.currentInteraction = this;
        UIManager.Instance.MinimapCanvas.gameObject.SetActive(true);
        UIManager.Instance.MinimapCanvas.GetComponent<MinimapManager>().setFloor(floor);
    }


    public override void Close()
    {
        closeMinimap();
        base.Close();
        
    }

    public void closeMinimap()
    {
        if (UIManager.Instance.currentInteraction == this)
        {
            UIManager.Instance.MinimapCanvas.gameObject.SetActive(false);
        }
    }

    public override void UIButtonPressed(string info)
    {
        //probably do something door related
    }
}
