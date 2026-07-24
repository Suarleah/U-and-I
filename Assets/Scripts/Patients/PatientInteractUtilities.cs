using UnityEngine;

public class PatientInteractUtilities : MonoBehaviour
{

    public static void RollResult(Patient patient, GameObject player, int creditsChange, float patienceChange, int healthChange)
    {
        if (creditsChange > 0)
        {
            GameManager.Instance.AddCredits(creditsChange);
        } else if (creditsChange < 0)
        {
            GameManager.Instance.SubtractCredits(creditsChange*-1);
        }
        
        if (healthChange > 0)
        {
            player.GetComponent<PlayerStats>().Heal (healthChange, new DamageDetails());
        } else if (healthChange < 0)
        {
            player.GetComponent<PlayerStats>().TakeDamage(healthChange * -1, new DamageDetails());
        }
        
        patient.changePatience(patienceChange);
    }
    
    //overload that also gives the player an item
    public static void RollResult(Patient patient, GameObject player, int creditsChange, float patienceChange, int healthChange, ItemSO item)
    {
        
        RollResult(patient, player, creditsChange, patienceChange, healthChange);
        //spawn item here
    }
    
}
