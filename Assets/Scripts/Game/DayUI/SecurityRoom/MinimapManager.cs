using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    //just the code that display all the entitires on the minimap (includes alive players, their corpses and patients, but you cant tell the diff on minimap)


    // the patients' floors can be told by their order in patientManager

    GameObject entityMarkerPrefab;


    void Update()
    {
        //loop through, mark all players
        
        List<PlayerMovement> players = GameManager.Instance.players;
        for (int i = 0; i < players.Count; i++)
        {
            
            if (!players[i].stats.isDead.Value)
            {
                
            }
        }

        //loop through, mark all patients
        for (int i = 0; i < PatientManager.Instance.currentPatients.Count; i++)
        {
            
        }
    }

    
}
