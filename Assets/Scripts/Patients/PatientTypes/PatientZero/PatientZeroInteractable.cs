using UnityEngine;
using FishNet.Object;
using UnityEditor;

public class PatientZeroInteractable : PatientInteractable
{

    
     [ServerRpc(RequireOwnership = false)]
    public override void PatientButtonExecute(string info, GameObject p)
    {
        if (onCD.Value)
        {
            return;
        }
        GiveFeedback("Feedback!!");
        if (info == "money")
        {
            GameManager.Instance.AddCredits(50); //for now just doing it this way, but considering adding the player as a middleman
            //return;
        }
        if (info == "damage")
        {
            p.GetComponent<PlayerStats>().TakeDamage(50, new DamageDetails());
        }

        if (info == "heal")
        {
            p.GetComponent<PlayerStats>().Heal(50, new DamageDetails());
        }

        if (info == "losePatience")
        {
            self.changePatience(-100);
        }
        StartCoroutine(goOnCooldown(10f));
        
    } 

}
