using System.Linq;
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

    public override void OnStartServer()
    {
        base.OnStartServer();
        Instance = this;

        NetworkManager.ServerManager.OnRemoteConnectionState += MovePlayer;
        // When someone's connection changes (When they join), move them to the spawn
    
    }

    public void MovePlayer(NetworkConnection connection, RemoteConnectionStateArgs args)
    {

        PlayerMovement player = connection.FirstObject.GetComponent<PlayerMovement>();
        // Get player
        if (player == null)
        {
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
