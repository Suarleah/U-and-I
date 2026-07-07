using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using UnityEngine;

public class ClockOutInteractable : Interactable
{
    public override void Interact()
    {
        ClockOut(player);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClockOut(GameObject p){ //has to pass this in otherwise it will only affect server's player
        if (onCD.Value)
        {
            return;
        }
        p.GetComponent<PlayerMovement>().stats.ClockOut();
        StartCoroutine(goOnCooldown(1)); //1 second cooldown to stop accidental double presses essentially, theres no real way that I can think of for them to double clockout
    }
}
