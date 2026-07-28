using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;
using TMPro;

public class PatientInteractable : Interactable
{
    public Patient self;
    public GameObject feedbackText;

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
    
    public override void UIButtonPressed(PatientInteractionInfo info)
    {
        PatientButtonExecute(info, player);
        Close();
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void PatientButtonExecute(PatientInteractionInfo info, GameObject p){
        if (onCD.Value)
        {
            return;
        }
        GiveFeedback("Rolled a " + info.rollValue);
        StartCoroutine(goOnCooldown(cooldown));
        if (info.interactionName == "Observe")
        {
            InteractObserve(info.rollValue, p);
        }
        if (info.interactionName == "Bribe")
        {
            InteractBribe(info.rollValue, p);
        }

        if (info.interactionName == "Therapy")
        {
            InteractTherapy(info.rollValue, p);
        }

        if (info.interactionName == "Electric Chair")
        {
            InteractElectricChair(info.rollValue, p);
        }
        
    }
    [ServerRpc(RequireOwnership = false)]
    public virtual void InteractObserve(int rollValue, GameObject p)
    {
        
    }
    [ServerRpc(RequireOwnership = false)]
    public virtual void InteractBribe(int rollValue, GameObject p)
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void InteractTherapy(int rollValue, GameObject p)
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void InteractElectricChair(int rollValue, GameObject p)
    {
        
    }


    [ServerRpc(RequireOwnership = false)]
    public virtual void GiveFeedback(string feedback){
        GameObject go = Instantiate(feedbackText);
        go.transform.position = transform.position;
        go.GetComponentInChildren<TextMeshProUGUI>().text = feedback;
        base.ServerManager.Spawn(go);
        go.GetComponentInChildren<PatientFeedbackText>().Begin();
    }
    
}
