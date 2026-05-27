using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using FishNet;
using FishNet.Connection;


public class PlayerStats : NetworkBehaviour
{
    public readonly SyncVar<string> playerName = new SyncVar<string>();
    public readonly SyncVar<float> speed = new SyncVar<float>();
    public readonly SyncVar<int> maxHealth = new SyncVar<int>();
    public readonly SyncVar<int> health = new SyncVar<int>();

    //public Item[] inventory;

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Initialize the server variable
        playerName.Value = "";
        speed.Value = 5f;
        maxHealth.Value = 100;
        health.Value = 100;
    }

}


