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
            return;
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
            self.changePatience(-50);
        }
        StartCoroutine(goOnCooldown(10f));
        
    } 

}
