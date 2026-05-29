using UnityEngine;

public class CloningController : Interactable
{
    public CloneZone cloneZone;

    public override void Interact()
    {
        cloneZone.CloneAll();
    }
}
