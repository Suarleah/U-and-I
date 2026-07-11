using UnityEngine;
using FishNet.Object;
using UnityEditor;

public class PatientZeroInteractable : PatientInteractable
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
            //GameManager.Instance.AddCredits(50); //for now just doing it this way, but considering adding the player as a middleman
            switch (info.rollValue)
            {
                case (1):
                    p.GetComponent<PlayerStats>().TakeDamage(25, new DamageDetails());
                    self.changePatience(-25);
                    break;
                case (2):
                    p.GetComponent<PlayerStats>().TakeDamage(20, new DamageDetails());
                    self.changePatience(-20);
                    GameManager.Instance.AddCredits(20);
                    break;
                case (3):
                    p.GetComponent<PlayerStats>().TakeDamage(10, new DamageDetails());
                    self.changePatience(-10);
                    GameManager.Instance.AddCredits(30);
                    break;
                case (4):
                    GameManager.Instance.AddCredits(30);
                    break;
                case (5):
                    GameManager.Instance.AddCredits(40);
                    break;
                case (6):
                    GameManager.Instance.AddCredits(50);
                    break;
            }
            
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
