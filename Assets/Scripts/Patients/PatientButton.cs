using UnityEngine;

public class PatientButton : MonoBehaviour
{
    [SerializeField] public PatientInteractionInfo info;

    public void pressed()
    {

        if (info.interactionName == "Electric Chair")
        {
            ConnectWires.Instance.Open(info);
            return;
        }


        info.rollValue = Random.Range(1, 7); // roll the dice
        UIManager.Instance.UIButtonPressed(info);
    }

}


