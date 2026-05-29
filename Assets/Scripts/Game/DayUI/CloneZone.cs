using UnityEngine;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;

public class CloneZone : NetworkBehaviour
{
    public List<Corpse> corpses; //list of corpses currently within the clonezone

    private void Awake()
    {
        corpses = new List<Corpse>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServerStarted)
        {
            return;
        }
        if (collision.gameObject.GetComponent<Corpse>())
        {
            corpses.Add(collision.gameObject.GetComponent<Corpse>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsServerStarted)
        {
            return;
        }
        if (collision.gameObject.GetComponent<Corpse>())
        {
            corpses.Remove(collision.gameObject.GetComponent<Corpse>());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CloneAll()
    {
        while (corpses.Count > 0)
        {
            corpses[0].corpseOwner.Value.Respawn(corpses[0].transform.position);
            corpses[0].NetworkObject.Despawn();
            //corpses.RemoveAt(0); //dont need to remove because it removes itself from ontriggerexit2d

        }
    }
}
