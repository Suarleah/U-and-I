using UnityEngine;
using UnityEngine.InputSystem;

public class PatientInteractable : Interactable
{
    public Patient self;

    public override void Interact()
    {
        interacting = true;
        UIManager.Instance.currentInteraction = this;
        UIManager.Instance.PatientInteractionCanvas.enabled = true;
    }


    public override void Close()
    {
        closePatientInfo();
        base.Close();
        
    }

    public void closePatientInfo()
    {
        if (UIManager.Instance.currentInteraction == this)
        {
            UIManager.Instance.PatientInteractionCanvas.enabled = false;
        }
    }
    
}
