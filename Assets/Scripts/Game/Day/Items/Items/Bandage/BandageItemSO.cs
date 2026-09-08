using Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "BandageItemSO", menuName = "Scriptable Objects/BandageItemSO")]
public class BandageItemSO : ItemSO
{
    public int healAmount = 15;
    public override void Use(UseInfo info, ItemInstance slot)
        => info.user.GetComponent<PlayerStats>().Heal(healAmount, new DamageDetails());
}