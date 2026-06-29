using FishNet.CodeAnalysis.Annotations;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Door : Interactable
{

    public readonly SyncVar<bool> closed = new SyncVar<bool>();


    public override void OnStartServer()
    {
        base.OnStartServer();

        closed.Value = false;
    }

    void Update()
    {
        localOnCD = onCD.Value;
        if (closest) 
        {
            c.gameObject.SetActive(true);
            if (Vector2.Distance(gameObject.transform.position, player.transform.position) > 5f)
            {
                closest = false;
            }

        } else
        {
            c.gameObject.SetActive(false);
            
        }

        if (closed.Value)
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
        } else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OpenDoor()
    {
        if (closed.Value && !onCD.Value)
        {
            closed.Value = false;
            StartCoroutine(goOnCooldown(1));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CloseDoor()
    {
        if (!closed.Value && !onCD.Value)
        {
            closed.Value = true;
            Close();
            StartCoroutine(goOnCooldown(1));
        }
        
    }

    //closeds the door even if its on cooldown
    [ServerRpc(RequireOwnership = false)]
    public void OverrideDoor()
    {
        if (closed.Value)
        {
            closed.Value = false;
            StartCoroutine(goOnCooldown(1));
        }
    }


    public override void Interact()
    {
        OverrideDoor();   
    }
}
