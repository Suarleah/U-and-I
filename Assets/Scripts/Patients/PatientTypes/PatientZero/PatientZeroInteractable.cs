using UnityEngine;
using FishNet.Object;
using UnityEditor;


//deprecated class, kept just in case
/*
public class PatientZeroInteractable : PatientInteractable
{

    
    [ServerRpc(RequireOwnership = false)]
    public override void InteractObserve(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    PatientInteractUtilities.RollResult(self, p, 10,  -20,  -20);
                    break;
                case (2):
                    PatientInteractUtilities.RollResult(self, p, 10,  -20,  -10);
                    break;
                case (3):
                    PatientInteractUtilities.RollResult(self, p, 10,  -10,  0);
                    break;
                case (4):
                    PatientInteractUtilities.RollResult(self, p, 15,  -5,  0);
                    break;
                case (5):
                    PatientInteractUtilities.RollResult(self, p, 15,  0,  0);
                    break;
                case (6):
                    PatientInteractUtilities.RollResult(self, p, 20,  0,  0);
                    break;
            }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void InteractBribe(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    PatientInteractUtilities.RollResult(self, p, 10,  -30,  -15);
                    break;
                case (2):
                    PatientInteractUtilities.RollResult(self, p, 10,  -20,  0);
                    break;
                case (3):
                    PatientInteractUtilities.RollResult(self, p, 10,  -10,  0);
                    break;
                case (4):
                    PatientInteractUtilities.RollResult(self, p, 10,  0,  0);
                    break;
                case (5):
                    PatientInteractUtilities.RollResult(self, p, 10,  5,  5);
                    break;
                case (6):
                    PatientInteractUtilities.RollResult(self, p, 10,  10,  10);
                    break;
            }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void InteractTherapy(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    PatientInteractUtilities.RollResult(self, p, 10,  -10,  -20);
                    break;
                case (2):
                    PatientInteractUtilities.RollResult(self, p, 10,  0,  -10);
                    break;
                case (3):
                    PatientInteractUtilities.RollResult(self, p, 10,  10,  0);
                    break;
                case (4):
                    PatientInteractUtilities.RollResult(self, p, 10,  15,  0);
                    break;
                case (5):
                    PatientInteractUtilities.RollResult(self, p, 10,  20,  0);
                    break;
                case (6):
                    PatientInteractUtilities.RollResult(self, p, 20,  30,  0);
                    break;
            }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void InteractElectricChair(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    PatientInteractUtilities.RollResult(self, p, 10,  -50,  -50);
                    break;
                case (2):
                    PatientInteractUtilities.RollResult(self, p, 15,  -35,  -35);
                    break;
                case (3):
                    PatientInteractUtilities.RollResult(self, p, 20,  -20,  -20);
                    break;
                case (4):
                    PatientInteractUtilities.RollResult(self, p, 20,  -15,  -10);
                    break;
                case (5):
                    PatientInteractUtilities.RollResult(self, p, 25,  -10,  -10);
                    break;
                case (6):
                    PatientInteractUtilities.RollResult(self, p, 35,  -10,  -10);
                    break;
            }
    }
}
*/