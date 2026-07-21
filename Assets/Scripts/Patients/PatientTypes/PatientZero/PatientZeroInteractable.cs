using UnityEngine;
using FishNet.Object;
using UnityEditor;

public class PatientZeroInteractable : PatientInteractable
{

    
    [ServerRpc(RequireOwnership = false)]
    public override void InteractObserve(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    GameManager.Instance.AddCredits(10);
                    p.GetComponent<PlayerStats>().TakeDamage(25, new DamageDetails());
                    self.changePatience(-25);
                    break;
                case (2):
                    GameManager.Instance.AddCredits(20);
                    p.GetComponent<PlayerStats>().TakeDamage(20, new DamageDetails());
                    self.changePatience(-20);
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
    }

    [ServerRpc(RequireOwnership = false)]
    public override void InteractBribe(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    GameManager.Instance.AddCredits(10);
                    p.GetComponent<PlayerStats>().TakeDamage(25, new DamageDetails());
                    self.changePatience(-25);
                    break;
                case (2):
                    GameManager.Instance.AddCredits(20);
                    p.GetComponent<PlayerStats>().TakeDamage(20, new DamageDetails());
                    self.changePatience(-20);
                    
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
    }

    [ServerRpc(RequireOwnership = false)]
    public override void InteractTherapy(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    GameManager.Instance.AddCredits(10);
                    p.GetComponent<PlayerStats>().TakeDamage(25, new DamageDetails());
                    self.changePatience(-25);
                    break;
                case (2):
                    GameManager.Instance.AddCredits(20);
                    p.GetComponent<PlayerStats>().TakeDamage(20, new DamageDetails());
                    self.changePatience(-20);
                    
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
    }

    [ServerRpc(RequireOwnership = false)]
    public override void InteractElectricChair(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    GameManager.Instance.AddCredits(10);
                    p.GetComponent<PlayerStats>().TakeDamage(25, new DamageDetails());
                    self.changePatience(-25);
                    break;
                case (2):
                    GameManager.Instance.AddCredits(20);
                    p.GetComponent<PlayerStats>().TakeDamage(20, new DamageDetails());
                    self.changePatience(-20);
                    
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
    }
}
