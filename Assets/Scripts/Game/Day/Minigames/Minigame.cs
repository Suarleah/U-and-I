using UnityEngine;

public class MinigameBase : MonoBehaviour
{
    protected PatientInteractionInfo currentInfo;

    public virtual void Open(PatientInteractionInfo info)
    {
        currentInfo = info;
        gameObject.SetActive(true);
    }

    protected void Finish(int result)
    {
        currentInfo.rollValue = result;
        gameObject.SetActive(false);
        UIManager.Instance.UIButtonPressed(currentInfo);
    }
}