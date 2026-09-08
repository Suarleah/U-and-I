using System;
using UnityEngine;

[Serializable]
public class ItemInstance //instance of the item info for each slot
{
    public ItemSO definition;
    public int count = 1;

    private float cooldownEndTime;
    public bool OnCooldown => Time.time < cooldownEndTime;
    public void StartCooldown(float seconds) => cooldownEndTime = Time.time + seconds;

    public ItemInstance() { }
    public ItemInstance(ItemSO definition, int count = 1)
    {
        this.definition = definition;
        this.count = count;
    }
}