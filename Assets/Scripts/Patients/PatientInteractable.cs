using UnityEngine;
using UnityEngine.InputSystem;

public class PatientInteractable : Interactable
{
    public Patient self;

    public override void Interact()
    {
        interacting = true;
        uimanager.currentInteraction = this;
        uimanager.PatientInteractionCanvas.enabled = true;
    }

    public override void released(InputAction.CallbackContext c)
    {
        if (closest && !uimanager.currentInteraction) // cant overlap the patient overlays
        {
            Interact();
            //Debug.Log("interacted!");
            //GameObject netObj = Instantiate(spawnedObject, gameObject.transform.position, gameObject.transform.rotation);
            //FishNet.InstanceFinder.ServerManager.Spawn(netObj);

        }
        else if (uimanager.currentInteraction == this) //pressing interact in the interaction menu closes it. Idk if its stupid or not to have this keybind
        {
            Close();
        }

    }

    public override void Close()
    {
        base.Close();
        closePatientInfo();
    }

    public void closePatientInfo()
    {
        if (uimanager.currentInteraction == this)
        {
            interacting = false;
            uimanager.PatientInteractionCanvas.enabled = false;
            uimanager.currentInteraction = null;
        }
    }
    
}
