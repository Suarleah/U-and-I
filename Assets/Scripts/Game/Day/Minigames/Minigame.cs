using UnityEngine;

public class MinigameBase : MonoBehaviour
{
    // protected PatientInteractionInfo currentInfo;  REPLACE WITH NEW SYSTEM

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    protected void Finish(int result)
    {
        //currentInfo.rollValue = result; REPLACE WITH NEW SYSTEM
        gameObject.SetActive(false);
        //UIManager.Instance.UIButtonPressed(currentInfo);  REPLACE WITH NEW SYSTEM
    }
}