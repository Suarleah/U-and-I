using UnityEngine;

public class SecurityTerminal : Interactable
{


    public override void Interact()
    {
        Debug.Log("Open Minimap!");
        interacting = true;
        UIManager.Instance.currentInteraction = this;
        UIManager.Instance.MinimapCanvas.gameObject.SetActive(true);

        SecurityMenu securityMenu = UIManager.Instance.MinimapCanvas.GetComponent<SecurityMenu>();
        securityMenu.enabled = true; // re-enable in case the Map item left it off

        UIManager.Instance.MinimapCanvas.GetComponent<MinimapManager>().setFloor(floor);
        securityMenu.setFloor(floor);
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

    
    public override void UIButtonPressed(PatientInteractionInfo info)
    {
        //probably do something door related
    }
}
