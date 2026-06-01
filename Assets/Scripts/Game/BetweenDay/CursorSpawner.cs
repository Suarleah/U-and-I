using UnityEngine;

public class CursorSpawner : MonoBehaviour
{
    //there might be a more elegant way of doing this, but I created this script so that each client would own their own cursor.
    [SerializeField] GameObject cursorPrefab;
    [SerializeField] Canvas c;


    private void Awake()
    {
        GameObject cursor = Instantiate(cursorPrefab, c.transform);
        //cursor.transform.SetParent(c.transform);     Replaced with the ,c.transform above
        FishNet.InstanceFinder.ServerManager.Spawn(cursor, FishNet.InstanceFinder.ClientManager.Connection); //spawn a cursor, give ownership to client
    }
}
