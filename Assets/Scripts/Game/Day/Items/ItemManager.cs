using UnityEngine;
using FishNet.Object;
using System.Linq;


public class ItemManager : NetworkBehaviour
{
    public GameObject[] testItems;

    public static ItemManager Instance;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Instance = this;
        for(int i = 0; i < testItems.Length; i++)
        {
            GameObject go = Instantiate(testItems[i]);
            go.transform.position = new Vector3();
            Spawn(go);
        }
        

    }
}
