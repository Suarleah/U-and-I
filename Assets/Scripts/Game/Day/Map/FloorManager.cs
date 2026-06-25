using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    //patient spawns
    public List<Transform> patientSpawns;
    //patient room bounds
    public List<RectTransform> patientRooms;

    //doors 
    public List<Door> doors;
    //janitor zones (include the necessary doors to use them, those doors must stay locked during the janitor animation)
    public List<CleaningSection> janitorZones;
    //elevator spawn zone
    public Transform elevatorSpawn;

    public Transform offset; //the offset position of this floor

    void Awake()
    {
        MapManager.Instance.floors.Add(this);
    }
}
