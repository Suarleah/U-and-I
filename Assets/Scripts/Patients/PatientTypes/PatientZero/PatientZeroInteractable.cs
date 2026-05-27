using UnityEngine;
using FishNet.Object;

public class PatientZeroInteractable : PatientInteractable
{
    [ServerRpc(RequireOwnership = false)]
    public override void UIButtonPressed(string info)
    {
        if (info == "money")
        {
            return;
        }
        if (info == "damage")
        {
            player.GetComponent<PlayerStats>().health.Value -= 20;
        }

        if (info == "heal")
        {
            player.GetComponent<PlayerStats>().health.Value += 20;
        }

        if (info == "losePatience")
        {
            self.patience.Value -= 20;
        }


    }
}
