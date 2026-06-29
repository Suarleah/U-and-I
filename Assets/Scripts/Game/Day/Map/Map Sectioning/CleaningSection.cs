using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class CleaningSection : NetworkBehaviour
{
    public List<Door> necessaryDoors; //this section cant be activated unless these doors are already down
    
    public List<Patient> zonePatients = new List<Patient>(); //list of patients currently in the zone, only has to be known on server

    public readonly SyncVar<bool> activatable = new SyncVar<bool>();

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
                    activatable.Value = false;
                    break;
                }
            }
        }

        activatable.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void CleanUp()
    {
        //clean all patients in the zone
        for (int i = 0; i < zonePatients.Count; i++)
        {
            if (zonePatients[i])
            {
                StartCoroutine(zonePatients[i].Contain());
            }
        }
        zonePatients.Clear();
        
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
    }
}
