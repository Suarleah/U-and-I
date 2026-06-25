using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class CleaningSection : NetworkBehaviour
{
    List<GameObject> necessaryDoors = new List<GameObject>(); //this section cant be activated unless these doors are already down
    List<Patient> zonePatients = new List<Patient>(); //list of patients currently in the zone

    [ServerRpc]
    void CleanUp()
    {
        //clean all patients in the zone
        zonePatients.Clear();
    }
    

    [Server]
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.GetComponent<Patient>())
        {
            zonePatients.Add(other.gameObject.GetComponent<Patient>());
        }
    }

    [Server]
    private void OnCollisionExit2D(Collision2D other) {
        if (other.gameObject.GetComponent<Patient>())
        {
            if (zonePatients.Contains(other.gameObject.GetComponent<Patient>()))
            {
                zonePatients.Remove(other.gameObject.GetComponent<Patient>());
            }
            
        }
    }
}
