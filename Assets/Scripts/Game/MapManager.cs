using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public static int patientsperfloor = 5;

    //manager for each floor
    public List<FloorManager> floors;

    void Awake()
    {
        Instance = this;
    }
}
