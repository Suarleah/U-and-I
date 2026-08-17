using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;


public class OperatingRoomFloor : NetworkBehaviour
{
    public List<PlayerStats> zonePlayers = new List<PlayerStats>(); //list of players currently in the zone



    //switch animation + visuals + kill any players inside
    // mode is the mode it's switching to.
    [ServerRpc(RequireOwnership = false)]
    public void Switch(OperatingTable.Operation mode) 
    {
        if ( zonePlayers.Count <= 0) //if there arent any entities in the zone, it wont be fired
        {
            return;
        }

        for (int i = 0; i < zonePlayers.Count; i++)
        {
            if (zonePlayers[i])
            {
                if (!zonePlayers[i].isDead.Value) //if theyre alive, kill them
                {
                    zonePlayers[i].Die();
                }
                
            }
        }
        
    }



    private void OnTriggerEnter2D(Collider2D other) {
        if (!IsServerStarted)
        {
            return;
        }
        if (other.gameObject.GetComponent<PlayerStats>())
        {
            zonePlayers.Add(other.gameObject.GetComponent<PlayerStats>());
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (!IsServerStarted)
        {
            return;
        }

        if (other.gameObject.GetComponent<PlayerStats>())
        {
            if (zonePlayers.Contains(other.gameObject.GetComponent<PlayerStats>()))
            {
                zonePlayers.Remove(other.gameObject.GetComponent<PlayerStats>());
            }
            
        }
    }
}
