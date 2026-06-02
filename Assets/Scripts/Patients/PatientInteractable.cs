using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

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
    
    public override void UIButtonPressed(string info)
    {
        PatientButtonExecute(info, player);
        Close();
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void PatientButtonExecute(string info, GameObject p){
        
    }
    
}
