using UnityEngine;

[CreateAssetMenu(fileName = "MapItemSO", menuName = "Scriptable Objects/Items/Map")]
public class MapItemSO : ItemSO
{
    public override void Use(UseInfo info, ItemInstance slot)
    {
        info.userInv.ToggleMinimapClient(info.userInv.Owner);
    }
}