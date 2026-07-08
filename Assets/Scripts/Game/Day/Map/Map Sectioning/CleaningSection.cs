using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections;

public class CleaningSection : NetworkBehaviour
{
    public List<Door> necessaryDoors; //this section cant be activated unless these doors are already down
    
    public List<Patient> zonePatients = new List<Patient>(); //list of patients currently in the zone, only has to be known on server

    public List<PlayerStats> zonePlayers = new List<PlayerStats>();

    public readonly SyncVar<bool> activatable = new SyncVar<bool>();

    public Coroutine preparation;

    void Update()
    {
        if (!IsServerStarted)
        {
            return;
        }
        //if any of the necessary doors are open, this cant be used
        for (int i = 0; i < necessaryDoors.Count; i++)
        {
            if (necessaryDoors[i])
            {
                if (!necessaryDoors[i].closed.Value)
                {
                    if (preparation != null)
                    {
                        preparation = null;
                    }
                    activatable.Value = false;
                    return;
                }
            }
        }
        preparation = StartCoroutine(PrepareCleaning(5)); //5 seconds after doors are closed, it can be cleaned
    }

    [ServerRpc(RequireOwnership = false)]
    public void CleanUp()
    {
        if (!activatable.Value)
        {
            return;
        }
        if (zonePatients.Count <= 0 && zonePlayers.Count <= 0) //if there arent any entities in the zone, it wont be fired
        {
            return;
        }
        GameManager.Instance.SubtractCredits(50);
        //clean all patients in the zone
        for (int i = 0; i < zonePatients.Count; i++)
        {
            if (zonePatients[i])
            {
                StartCoroutine(zonePatients[i].Contain());
            }
        }
        zonePatients.Clear();

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
        activatable.Value = false;
        
    }

    [Server]
    public IEnumerator PrepareCleaning(int seconds) //zone cant be cleared until all doors have been down for 5 seconds
    {
        yield return new WaitForSeconds(seconds);
        activatable.Value = true;
    }
    

    //[Server]
    private void OnTriggerEnter2D(Collider2D other) {
        if (!IsServerStarted)
        {
            return;
        }
        if (other.gameObject.GetComponent<Patient>())
        {
            zonePatients.Add(other.gameObject.GetComponent<Patient>());
        }
        if (other.gameObject.GetComponent<PlayerStats>())
        {
            zonePlayers.Add(other.gameObject.GetComponent<PlayerStats>());
        }
    }

    //[Server]
    private void OnTriggerExit2D(Collider2D other) {
        if (!IsServerStarted)
        {
            return;
        }
        if (other.gameObject.GetComponent<Patient>())
        {
            if (zonePatients.Contains(other.gameObject.GetComponent<Patient>()))
            {
                zonePatients.Remove(other.gameObject.GetComponent<Patient>());
            }
            
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
