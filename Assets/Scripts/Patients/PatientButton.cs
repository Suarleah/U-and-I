using UnityEngine;

public class PatientButton : MonoBehaviour
{
    [SerializeField] public PatientInteractionInfo info;

    public void pressed()
    {
        info.rollValue = Random.Range(1, 7); // roll the dice
        UIManager.Instance.UIButtonPressed(info);
    }
    
}


