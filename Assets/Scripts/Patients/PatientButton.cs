using UnityEngine;

public class PatientButton : MonoBehaviour
{
    [SerializeField] public PatientInteractionInfo info;

    public void pressed()
    {
        UIManager.Instance.UIButtonPressed(info);
        info.rollValue = Random.Range(1, 7); // roll the dice
    }
    
}


