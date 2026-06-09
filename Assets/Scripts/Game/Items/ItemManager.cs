using UnityEngine;
using FishNet.Object;


public class ItemManager : NetworkBehaviour
{
    public GameObject testItem;

    public static ItemManager Instance;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Instance = this;
        for(int i = 0; i < 5; i++)
        {
            GameObject go = Instantiate(testItem);
            go.transform.position = new Vector3();
            Spawn(go);
        }
        

    }
}
