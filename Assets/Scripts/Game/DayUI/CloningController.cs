using UnityEngine;

public class CloningController : Interactable
{
    public CloneZone cloneZone;

    public override void Interact()
    {
        if (cloneZone.corpses.Count > 0)
        {
            cloneZone.CloneAll();
        }
    }
}
