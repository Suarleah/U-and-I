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

    public string localPlayerName;
    public float localspeed;
    public int localmaxHealth;
    public int localHealth;
    public bool localisDead;
    //public Item[] inventory;

    public GameObject corpsePrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Initialize the server variable
        playerName.Value = "";
        speed.Value = 10f;
        maxHealth.Value = 100;
        health.Value = 100;
        isDead.Value = false;
    }

    private void Update()
    {
        localHealth = health.Value;
        localmaxHealth = maxHealth.Value;
        localPlayerName = playerName.Value;
        localspeed = speed.Value;
        localisDead = isDead.Value;

        
    }

    [ServerRpc]
    public void TakeDamage(int amt, DamageDetails deets)
    {
        health.Value -= amt;
        if (health.Value <= 0 && !isDead.Value)
        {
            Die();
        }
    }

    [ServerRpc]
    public void Heal(int amt, DamageDetails deets)
    {
        health.Value += amt;
        if (health.Value >= maxHealth.Value)
        {
            health.Value = maxHealth.Value;
        }
    }

    public void Die()
    {
        
        isDead.Value = true;

        CloseLocalUIManager(base.Owner);
        GameManager.Instance.playerDied(gameObject);
        corpsePrefab.transform.position = transform.position;
        GameObject corpse = Instantiate(corpsePrefab);
        corpse.GetComponent<Corpse>().corpseOwner.Value = (base.GetComponent<PlayerStats>());
        SetCorpseOwner(corpse.GetComponent<Corpse>());
        FishNet.InstanceFinder.ServerManager.Spawn(corpse);
 
    }

    
    [ObserversRpc]
    public void SetCorpseOwner(Corpse corpse)
    {
        
        corpse.nameplate.text = base.GetComponent<PlayerStats>().playerName.Value + "'s corpse";
    }


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
    }

    [ObserversRpc]
    public void moveClient(Vector2 pos)
    {
        transform.position = pos;
    }
}


