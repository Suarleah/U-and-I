using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;
using TMPro;

public class PatientInteractable : Interactable
{
    public Patient self;
    

    public override void Interact()
    {
        interacting = true;
        //UIManager.Instance.currentInteraction = this;
        if (self.followingPlayer == player) //if currently following, unfollow
        {
            GiveFeedback("Stopped Following!");
            player.GetComponent<PlayerMovement>().followingPatient = null;
            self.followingPlayer = null;
            interacting = false;
            Close();
        } else
        {
            GiveFeedback("Started Following!");
            if (player.GetComponent<PlayerMovement>().followingPatient) //only one follower a a time.
            {
                player.GetComponent<PlayerMovement>().releaseFollower();
            }
           
            self.followingPlayer = player;
            
        }
        
    }

    public void Update() 
    {
        localOnCD = onCD.Value;
        if (closest) 
        {
            //Debug.Log("OPEN INTERACTION CANVAS!!");
            //UIManager.Instance.PatientInteractionCanvas.enabled = true;
            c.gameObject.SetActive(true);
            if (Vector2.Distance(transform.position, player.transform.position) > 5f)
            {
                closest = false;
            }

        } else
        {
            //UIManager.Instance.PatientInteractionCanvas.enabled = false;
            c.gameObject.SetActive(false);
        }

        /*if (Vector3.Distance(player.transform.position, transform.position) > 10f)
        {
            Close();
        }*/
    } 

    public override void Close()
    {
        closest = false;
        interacting = false;
        if (self.followingPlayer)
        {
            self.followingPlayer.GetComponent<PlayerMovement>().releaseFollower();
            self.followingPlayer = null;
        }
        
        //unfollow
        
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
            GiveFeedback("On Cooldown!");
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


    
    
}
