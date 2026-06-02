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
            /*string s;
            if (IsServerStarted)
            {
                 s = "server";
            }
            else
            {
                 s = "client";
            }
            Debug.Log("OnTriggerEnter" + s);*/
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
            /*string s;
            if (IsServerStarted)
            {
                 s = "server";
            }
            else
            {
                 s = "client";
            }
            Debug.Log("OnTriggerExit" + s);*/
            RemoveCorpseFromList(collision.gameObject.GetComponent<Corpse>());
        }
    }

    [Server]
    public void AddCorpseToList(Corpse corpse)
    {
        Debug.Log("addcorpse");
        corpses.Add(corpse);
    }

    [Server]
    public void RemoveCorpseFromList(Corpse corpse)
    {
        if (corpses.Contains(corpse)){
            corpses.Remove(corpse);
        }
        
    }


    [ServerRpc(RequireOwnership = false)]
    public void CloneAll()
    {
        for (int i = corpses.Count - 1; i >= 0; i--) //in case of extra corpses that havent been removed properly, clear all null corpses
        {
            if (!corpses[i]) 
            {
                corpses.Remove(corpses[i]);
            }
            

        }
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
            if (corpses.Count > i)
            {
                corpses.RemoveAt(i); //if not properly removed from ontriggerexit2d
            }
            
            //corpses.RemoveAt(0); //dont need to remove because it removes itself from ontriggerexit2d

        }
    }
}
