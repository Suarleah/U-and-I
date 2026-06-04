using UnityEngine;

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    int id;

    public bool stackable;
    public int stackMax;
    public int count; //if stackable, this is how many there currently are in the stack 

    public bool cursed;

    public GameObject itemInteractablePrefab;

    public GameObject inventoryItemPrefab;

    public GameObject inventoryVisual; //theres a better /more concise way to do this but since no assets in imma just do this


    
}
