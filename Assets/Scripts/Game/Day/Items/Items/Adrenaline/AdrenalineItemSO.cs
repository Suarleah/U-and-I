using System.Collections;
using Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "AdrenalineItemSO", menuName = "Scriptable Objects/AdrenalineItemSO")]
public class AdrenalineItemSO : ItemSO
{
    public float speedBoost = 5f;
    public float duration = 10f;

    public override void Use(UseInfo info, ItemInstance slot)
    {
        PlayerStats stats = info.user.GetComponent<PlayerStats>();
        stats.speed.Value += speedBoost;
        info.userInv.StartCoroutine(Revert(stats));
    }

    private IEnumerator Revert(PlayerStats stats)
    {
        yield return new WaitForSeconds(duration);
        stats.speed.Value -= speedBoost;
    }
}