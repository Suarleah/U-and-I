using System.Collections.Generic;
using UnityEngine;


//this is the scriptable object holding a reference to the other itemSOs, so that they can be synced
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public ItemSO[] allItems; // drag every ItemSO asset in here

    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            //for this to work, there needs to be a scriptable object in the resources folder because thats where it loads from
            if (_instance == null)
                _instance = Resources.Load<ItemDatabase>("ItemDatabase"); // one load, cached
            return _instance;
        }
    }

    private Dictionary<int, ItemSO> _byId;
    public ItemSO GetById(int id)
    {
        if (_byId == null)
        {
            _byId = new Dictionary<int, ItemSO>();
            foreach (ItemSO item in allItems)
                _byId[item.id] = item;
        }
        _byId.TryGetValue(id, out ItemSO result);
        return result;
    }
}