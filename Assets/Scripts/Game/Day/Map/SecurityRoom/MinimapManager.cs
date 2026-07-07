using System.Collections.Generic;
using System.Linq;
using FishNet.Demo.AdditiveScenes;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    //just the code that display all the entitires on the minimap (includes alive players, their corpses and patients, but you cant tell the diff on minimap)

    //FYI this code must have setfloor called before its awake or else it will cause an error

    // the patients' floors can be told by their order in patientManager

    public GameObject entityMarkerPrefab;
    public float scale; //
    public Transform minimapTransform;

    public int floorNum = 0; //the floor # this minimap is for (since each minimap will only be able to show its own floor)
    public FloorManager floor;

    private float tickTime = 0.5f;
    private float tickTimer = 0;


    List<GameObject> markers = new List<GameObject>();

    

    void Update()
    {
        if (tickTimer <= 0) //only updates once every half second, maybe less intensive and seems a little more immersive
        {
            tickTimer = tickTime;
            UpdateMarkers();
        }
        tickTimer -= Time.deltaTime;
        
    }

    void OnEnable() 
    {
        UpdateMarkers();
        
    }

    public void setFloor(int num)
    {
        floorNum = num;
        floor = MapManager.Instance.floors[floorNum];
    }

    public void UpdateMarkers()
    {
        if (!floor)
        {
            return;
        }

        //first, delete all the old markers
        for (int i = 0; i < markers.Count; i++)
        {
            Destroy(markers[i]);
        }
        markers.Clear();

         //loop through, mark all players
        List<PlayerMovement> players = GameManager.Instance.GetPlayers();
        for (int i = 0; i < players.Count; i++)
        {
            
            if (players[i].stats.isDead.Value) //if theyre dead track their corpse
            {
                GameObject corpse = players[i].stats.myCorpse.Value;
                if (corpse)
                {
                    if (corpse.GetComponent<Corpse>().floor == floorNum) //if its on the same floor
                    {
                        GameObject marker = Instantiate(entityMarkerPrefab, minimapTransform);
                        marker.transform.localPosition = (corpse.transform.position - floor.offset.position) * scale; //translate realworld position to minimap position
                        markers.Add(marker);
                    }
                }
                
                
                
            } else //otherwise track their player
            {
                if (players[i].stats.floor == floorNum)
                {
                    GameObject marker = Instantiate(entityMarkerPrefab, minimapTransform);
                    marker.transform.localPosition = (players[i].transform.position - floor.offset.position) * scale; //translate realworld position to minimap position
                    markers.Add(marker);
                }
            }
        }

    
        //loop through, mark all patients
        for (int i = 0; i < PatientManager.Instance.spawnedPatients.Count; i++)
        {
            List<Patient> patients = PatientManager.Instance.GetAllSpawnedPatients();
            if (patients[i])
            {
                if (patients[i].floor == floorNum)
                {
                GameObject marker = Instantiate(entityMarkerPrefab, minimapTransform);
                marker.transform.localPosition = (patients[i].transform.position - floor.offset.position) * scale; //translate realworld position to minimap position
                markers.Add(marker);
                } 
            }
           
        }
    }

    
}
