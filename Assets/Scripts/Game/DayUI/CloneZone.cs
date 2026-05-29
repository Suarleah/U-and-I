using UnityEngine;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;

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
            AddCorpseFromList(collision.gameObject.GetComponent<Corpse>());
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
            RemoveCorpseFromList(collision.gameObject.GetComponent<Corpse>());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddCorpseFromList(Corpse corpse)
    {
        corpses.Add(corpse);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveCorpseFromList(Corpse corpse)
    {
        corpses.Remove(corpse);
    }


    [ServerRpc(RequireOwnership = false)]
    public void CloneAll()
    {
        if (corpses.Count <= 0)
        {
            return;
        }
        while (corpses.Count > 0)
        {
            corpses[0].corpseOwner.Value.GetComponent<PlayerStats>().Respawn(corpses[0].transform.position);
            corpses[0].NetworkObject.Despawn();
            //corpses.RemoveAt(0); //dont need to remove because it removes itself from ontriggerexit2d

        }
    }
}
