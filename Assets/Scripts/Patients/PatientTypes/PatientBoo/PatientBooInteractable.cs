using UnityEngine;
using FishNet.Object;
using UnityEditor;

public class PatientBooInteractable : PatientInteractable
{

    
     [ServerRpc(RequireOwnership = false)]
    public override void PatientButtonExecute(PatientInteractionInfo info, GameObject p)
    {
        if (onCD.Value)
        {
            return;
        }
        GiveFeedback("Rolled a " + info.rollValue);
        if (info.interactionName == "money")
        {
            GameManager.Instance.AddCredits(50); //for now just doing it this way, but considering adding the player as a middleman
            //return;
        }
        if (info.interactionName == "damage")
        {
            p.GetComponent<PlayerStats>().TakeDamage(50, new DamageDetails());
        }

        if (info.interactionName == "heal")
        {
            p.GetComponent<PlayerStats>().Heal(50, new DamageDetails());
        }

        if (info.interactionName == "losePatience")
        {
            self.changePatience(-100);
        }
        StartCoroutine(goOnCooldown(10f));
        
    } 

}
