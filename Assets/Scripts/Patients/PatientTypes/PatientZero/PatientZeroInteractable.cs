using UnityEngine;
using FishNet.Object;

public class PatientZeroInteractable : PatientInteractable
{
    public override void UIButtonPressed(string info)
    {
        if (info == "money")
        {
            return;
        }
        if (info == "damage")
        {
            player.GetComponent<PlayerStats>().TakeDamage(20, new DamageDetails());
        }

        if (info == "heal")
        {
            player.GetComponent<PlayerStats>().Heal(20, new DamageDetails());
        }

        if (info == "losePatience")
        {
            self.patience.Value -= 20;
        }

    }
}
