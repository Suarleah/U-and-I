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


    public override void Close()
    {
        closePatientInfo();
        base.Close();
        
    }

    public void closePatientInfo()
    {
        if (uimanager.currentInteraction == this)
        {
            uimanager.PatientInteractionCanvas.enabled = false;
        }
    }
    
}
