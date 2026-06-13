using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using UnityEngine;

public class ClockOutInteractable : Interactable
{
    public override void Interact()
    {
        ClockOut();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ClockOut(){
        if (onCD.Value)
        {
            return;
        }
        player.GetComponent<PlayerMovement>().stats.ClockOut();
        StartCoroutine(goOnCooldown(1)); //1 second cooldown to stop accidental double presses essentially, theres no real way that I can think of for them to double clockout
    }
}
