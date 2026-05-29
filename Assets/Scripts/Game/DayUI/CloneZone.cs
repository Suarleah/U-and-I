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
            AddCorpseToList(collision.gameObject.GetComponent<Corpse>());
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
            Debug.Log("OnTriggerExit");
            RemoveCorpseFromList(collision.gameObject.GetComponent<Corpse>());
        }
    }

    //[ServerRpc(RequireOwnership = false)]
    public void AddCorpseToList(Corpse corpse)
    {
        corpses.Add(corpse);
    }

    //[ServerRpc(RequireOwnership = false)]
    public void RemoveCorpseFromList(Corpse corpse)
    {
        if (corpses.Contains(corpse)){
            corpses.Remove(corpse);
        }
        
    }


    [ServerRpc(RequireOwnership = false)]
    public void CloneAll()
    {
        if (corpses.Count <= 0)
        {
            return;
        }
        for (int i = corpses.Count - 1; i >= 0; i--)
        {
            if (corpses.Count > i + 1) //remove extras if the ontrigger doesnt trigger in time
            {
                corpses.Remove(corpses[i+1]);
            }
            corpses[i].corpseOwner.Value.gameObject.GetComponent<PlayerStats>().Respawn(corpses[i].transform.position);
            corpses[i].NetworkObject.Despawn();
            Debug.Log("Despawned");
            //corpses.RemoveAt(0); //dont need to remove because it removes itself from ontriggerexit2d

        }
    }
}
