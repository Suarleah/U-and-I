using UnityEngine;

public class MinigameBase : MonoBehaviour
{
    // Set by whoever opens the minigame (the OperatingTable). later gets the final roll, 1 to 6.
    public System.Action<int> onFinished;

        // Called right before Open() with the patient being operated on.
    // Each minigame overrides this to pull its settings from the patient.
    public virtual void Configure(Patient patient) { }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    // Every minigame reports a roll from 1 (worst) to 6 (best). Anything outside that is clamped,
    // so a minigame can report 0 for a total failure and it counts as a 1.
    protected void Finish(int result)
    {
        gameObject.SetActive(false);

        // Clear the callback before calling it, so a minigame that calls Finish twice
        // in the same frame (e.g. Observe targets adding score) only reports once.
        System.Action<int> callback = onFinished;
        onFinished = null;
        callback?.Invoke(Mathf.Clamp(result, 1, 6));
    }

    // Closes the minigame without reporting a result (player backed out).
    public void Cancel()
    {
        onFinished = null;
        gameObject.SetActive(false);
    }
}