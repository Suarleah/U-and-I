using System.Linq;
using FishNet;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : NetworkBehaviour
{
    public GameObject[] spawnPoints;
    private int spawnNum = 0;

    public static SpawnManager Instance;

    



    //not really necessary and can be renamed but i did it just in case
    [SerializeField] private FishNet.Managing.NetworkManager NetworkManagerInstance;

    private void Awake()
    {
        NetworkManagerInstance = InstanceFinder.NetworkManager;  
    }


    

    public override void OnStartServer()
    {
        base.OnStartServer();
        Instance = this;
        NetworkManagerInstance.SceneManager.OnClientPresenceChangeEnd += MovePlayer;
        //" Called when a client presence changes within a scene, after the server rebuilds observers."

    }

    [Server] // This call only be done on the server, and it automatically does because it subscribe to ebvent :D
    public void MovePlayer(ClientPresenceChangeEventArgs arghhhh)
    {
        Debug.Log("tried to move player");
        PlayerMovement player = arghhhh.Connection.FirstObject.GetComponent<PlayerMovement>();
        // Get player
        if (player == null)
        {
            Debug.Log("player was null");
            return;
        }

        GameManager.Instance.players.Add(player.GetComponent<NetworkObject>().ObjectId);

        player.transform.position = spawnPoints[spawnNum].transform.position;
        //NetworkTransform test = player.gameObject.GetComponent<NetworkTransform>();
        Vector2 pos = new Vector2(spawnPoints[spawnNum].transform.position.x, spawnPoints[spawnNum].transform.position.y);
        InitialPosUpdate(player, pos);
        spawnNum++;
        
    }

    [ObserversRpc]
    public void InitialPosUpdate(PlayerMovement player, Vector2 newPos)
    {
        // Perhaps can be replaced with something related to each players NetworkTransform
        player.transform.position = newPos;
    }


}
