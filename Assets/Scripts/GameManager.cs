using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject[] spawnPoints;
    private int spawnNum = 0;

    public static GameManager Instance;


    //not really necessary and can be renamed but i did it just in case
    
    [SerializeField] private FishNet.Managing.NetworkManager NetworkManagerInstance;


    //I added an awake function, because I think the server starts after this object wakes (so it wont move the players in time for the scene load)
    private void Awake()
    {
        NetworkManagerInstance = InstanceFinder.NetworkManager;
        NetworkManagerInstance.SceneManager.OnLoadEnd += test;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Instance = this;

        //NetworkManager.ServerManager.OnRemoteConnectionState += MovePlayer;
        // When someone's connection changes (When they join), move them to the spawn

    }


    private void test(SceneLoadEndEventArgs args)
    {
        LoadQueueData d = args.QueueData;
        Debug.Log("tried to move player onclientload");
        if (!d.AsServer)
            return;
        Debug.Log("tried to move player onclientload passed server check");
        PlayerMovement player;

        //coded spawn number like this because the host doesnt consider itself to be a connection, hence the first "connection" is actually spawnNum-1
        if (spawnNum == 0)
        {
             player = base.ClientManager.Connection.FirstObject.GetComponent<PlayerMovement>();
        } else
        {
            player = d.Connections[spawnNum - 1].FirstObject.GetComponent<PlayerMovement>();
        }
        if (player == null)
        {
            Debug.Log("player was null onclientload");
            return;
        }

        player.transform.position = spawnPoints[spawnNum].transform.position;
        spawnNum++;
    }


    public void MovePlayer(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        Debug.Log("tried to move player");
        PlayerMovement player = connection.FirstObject.GetComponent<PlayerMovement>();
        // Get player
        if (player == null)
        {
            Debug.Log("player was null");
            return;
        }

        player.transform.position = spawnPoints[spawnNum].transform.position;
        spawnNum++;
    }
    // Update is called once per frame
    void Update()
    {

    }

    //[ServerRpc]



    //[ObserversRpc]
}
