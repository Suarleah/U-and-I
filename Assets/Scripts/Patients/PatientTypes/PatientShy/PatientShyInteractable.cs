using UnityEngine;
using FishNet.Object;
using UnityEditor;

public class PatientShyInteractable : PatientInteractable
{

    [ServerRpc(RequireOwnership = false)]
    public override void InteractObserve(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    PatientInteractUtilities.RollResult(self, p, 20,  -50,  -50);
                    break;
                case (2):
                    PatientInteractUtilities.RollResult(self, p, 20,  -50,  -25);
                    break;
                case (3):
                    PatientInteractUtilities.RollResult(self, p, 20,  -25,  -10);
                    break;
                case (4):
                    PatientInteractUtilities.RollResult(self, p, 30,  -15,  0);
                    break;
                case (5):
                    PatientInteractUtilities.RollResult(self, p, 30,  -10,  0);
                    break;
                case (6):
                    PatientInteractUtilities.RollResult(self, p, 30,  0,  0);
                    break;
            }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void InteractBribe(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    PatientInteractUtilities.RollResult(self, p, 10,  -30,  -50);
                    break;
                case (2):
                    PatientInteractUtilities.RollResult(self, p, 10,  -20,  -30);
                    break;
                case (3):
                    PatientInteractUtilities.RollResult(self, p, 10,  0,  -20);
                    break;
                case (4):
                    PatientInteractUtilities.RollResult(self, p, 10,  10,  -10);
                    break;
                case (5):
                    PatientInteractUtilities.RollResult(self, p, 10,  10,  0);
                    break;
                case (6):
                    PatientInteractUtilities.RollResult(self, p, 10,  20,  0);
                    break;
            }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void InteractTherapy(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    PatientInteractUtilities.RollResult(self, p, 20,  -30,  -30);
                    break;
                case (2):
                    PatientInteractUtilities.RollResult(self, p, 20,  -15,  -10);
                    break;
                case (3):
                    PatientInteractUtilities.RollResult(self, p, 20,  10,  0);
                    break;
                case (4):
                    PatientInteractUtilities.RollResult(self, p, 25,  10,  0);
                    break;
                case (5):
                    PatientInteractUtilities.RollResult(self, p, 30,  20,  10);
                    break;
                case (6):
                    PatientInteractUtilities.RollResult(self, p, 30,  30,  20);
                    break;
            }
    }

    [ServerRpc(RequireOwnership = false)]
    public override void InteractElectricChair(int rollValue, GameObject p)
    {
        switch (rollValue)
            {
                case (1):
                    PatientInteractUtilities.RollResult(self, p, 20,  -30,  -30);
                    break;
                case (2):
                    PatientInteractUtilities.RollResult(self, p, 20,  -5,  -10);
                    break;
                case (3):
                    PatientInteractUtilities.RollResult(self, p, 20,  0,  0);
                    break;
                case (4):
                    PatientInteractUtilities.RollResult(self, p, 20,  0,  0);
                    break;
                case (5):
                    PatientInteractUtilities.RollResult(self, p, 35,  0,  0);
                    break;
                case (6):
                    PatientInteractUtilities.RollResult(self, p, 50,  20,  0);
                    break;
            }
    }

}
