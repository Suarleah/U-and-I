using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using FishNet;
using FishNet.Connection;
using FishNet.Demo.AdditiveScenes;


public class PlayerStats : NetworkBehaviour
{
    public readonly SyncVar<string> playerName = new SyncVar<string>();
    public readonly SyncVar<float> speed = new SyncVar<float>();
    public readonly SyncVar<int> maxHealth = new SyncVar<int>();
    public readonly SyncVar<int> health = new SyncVar<int>();
    public readonly SyncVar<bool> isDead = new SyncVar<bool>();
    public readonly SyncVar<bool> isClockedOut = new SyncVar<bool>();

    public readonly SyncVar<GameObject> myCorpse = new SyncVar<GameObject>(); //the current corpse of this player


    public string localPlayerName;
    public float localspeed;
    public int localmaxHealth;
    public int localHealth;
    public bool localisDead;
    public bool localisClockedOut;

    public int floor; //what floor this player is currently on
    //public Item[] inventory;

    public GameObject corpsePrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Initialize the server variable
        ResetPlayer();
    }

    private void Update()
    {
        localHealth = health.Value;
        localmaxHealth = maxHealth.Value;
        localPlayerName = playerName.Value;
        localspeed = speed.Value;
        localisDead = isDead.Value;
        localisClockedOut = isClockedOut.Value;

        
    }

    [Server]
    public void ResetPlayer()
    {
        playerName.Value = "";
        speed.Value = 10f;
        maxHealth.Value = 100;
        health.Value = 100;
        isDead.Value = false;
        isClockedOut.Value = false;
        myCorpse.Value = null;
    } 

    [Server]
    public void TakeDamage(int amt, DamageDetails deets)
    {
        health.Value -= amt;
        if (health.Value <= 0 && !isDead.Value)
        {
            Die();
        }
    }

    public void DamageWrapperDebug()
    {
        DamageDetails ethan = new DamageDetails();
        
        TakeDamage(10, ethan);
    }

    [Server]
    public void Heal(int amt, DamageDetails deets)
    {
        health.Value += amt;
        if (health.Value >= maxHealth.Value)
        {
            health.Value = maxHealth.Value;
        }
    }

    [Server]
    public void Die()
    {
        
        isDead.Value = true;
        CloseLocalUIManager(base.Owner);
        GameManager.Instance.playerDied(gameObject);

        corpsePrefab.transform.position = transform.position;
        GameObject corpse = Instantiate(corpsePrefab);
        
        InventoryManager inv = gameObject.GetComponentInChildren<InventoryManager>();
        inv.DropAll();

        FishNet.InstanceFinder.ServerManager.Spawn(corpse);
        corpse.GetComponent<Corpse>().setCorpseOwner(base.NetworkObject);
        myCorpse.Value = corpse;
        /*corpse.GetComponent<Corpse>().corpseOwner.Value = gameObject;
        SetCorpseName(corpse.GetComponent<Corpse>());*/
 
    }

    [Server]
    public void ClockOut()
    {
        Debug.Log("player clocked out!");
        isClockedOut.Value = true;  //lets just say once you clock out you cant unclock out
        GameManager.Instance.PlayerClockedOut(gameObject);
    }

    
    /*[ObserversRpc]
    public void SetCorpseName(Corpse corpse)
    {
        
        corpse.nameplate.text = this.playerName.Value + "'s Corpse";
    }*/
 

    [TargetRpc]
    public void CloseLocalUIManager(NetworkConnection conn)
    {
         UIManager.Instance.Close();
    }

    [ServerRpc(RequireOwnership = false)]
    public void Respawn(Vector3 pos)
    {
        health.Value = maxHealth.Value;
        moveClient(pos);
        isDead.Value = false;
        myCorpse.Value = null;
    }

    [ObserversRpc]
    public void moveClient(Vector2 pos)
    {
        transform.position = pos;
    }
}


