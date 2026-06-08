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
        player.GetComponent<PlayerMovement>().stats.ClockOut();
        goOnCooldown(1); //1 second cooldown to stop accidental double presses essentially, theres no real way that I can think of for them to double clockout
    }
}
