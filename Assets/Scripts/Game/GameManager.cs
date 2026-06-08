using UnityEngine;
using FishNet;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public readonly SyncVar<float> money = new SyncVar<float>(); //money is shared across all players
    public static GameManager Instance;
    public int day; //the day #

    public List<PlayerMovement> players; //list of all players in the game



    void Awake()
    {
        Instance = this;
    }
    
    
    [ServerRpc(RequireOwnership = false)]
    public void playerDied(GameObject player) //players call this whenever they die
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].stats.isDead.Value)
            {
                return;
            }
        }


        //if every player is dead, end game here but i have no method to end the game so i have nothing here
        //StartCoroutine(EndGame());
        Debug.Log("Game over!");
    }
}
