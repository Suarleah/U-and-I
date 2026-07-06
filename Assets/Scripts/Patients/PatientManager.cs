using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Managing.Scened;
using FishNet;

//this is a script to keep track of patients which the players have accumulated so far
public class PatientManager : NetworkBehaviour
{
    public static PatientManager Instance;
    public List<PatientSO> allPatients; //all patients that exist in the game

    public List<PatientSO> currentPatients; //patients the players currently have

    public List<PatientSO> unusedPatients; //patients the players do not currently have
    private List<PatientSO> availPatients; //used only for the GetRandomUnusedPatients method, keeps track of which patients are being selected as well (so that they dont have the same option repeated)

    public readonly SyncList<int> spawnedPatients = new SyncList<int>();  //list of patients spawned in the game

    public List<Patient> localSpawnedPatients = new List<Patient>(); //just for testing purposes

    
    private void Awake()
    {
        Instance = this;
        unusedPatients.AddRange(allPatients);
        availPatients = new List<PatientSO>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // This ensures the object is fully initialized on the network 
        // before you read from or write to SyncVars/SyncObjects
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (IsServerInitialized)
        {
        //    selectPatient(allPatients[0]); //this was just used for testing
        }
        spawnedPatients.OnChange += OnSpawnedPatientsChange;
        FishNet.InstanceFinder.NetworkManager.SceneManager.OnLoadEnd += OnLoadEnd;
        
    }

    private void OnSpawnedPatientsChange(SyncListOperation op, int index, int oldItem, int newItem, bool asServer)
    {
        
        for (int i = 0; i < spawnedPatients.Count; i++)
        {
            if (localSpawnedPatients.Count <= i)
            {
                localSpawnedPatients.Add(GetPatientFromId(spawnedPatients[i]));
            } else
            {
                localSpawnedPatients[i] = GetPatientFromId(spawnedPatients[i]);
            }
            
        }
    }

    [Server] // This call only be done on the server, and it automatically does because it subscribe to ebvent :D
    public void OnLoadEnd(SceneLoadEndEventArgs args)
    {
        if (!IsServerInitialized)
        {
            return;
        }
        string targetSceneName = "entity test scene"; 

        foreach (UnityEngine.SceneManagement.Scene scene in args.LoadedScenes)
        {
            if (scene.name == targetSceneName)
            {
                SpawnAllPatients();
            }
        }
        
    }

    public List<PatientSO> getRandomUnusedPatients (int count) //gets x patients that the players dont currently have
    {
        if (count >= unusedPatients.Count)
        {
            count = unusedPatients.Count; //hopefully will never happen, but if there arent enough unusedpatients left in the pool theyll just have less to choose from
        }
        
        List<PatientSO> ret = new List<PatientSO>();
        if (count == 0)
        {
            return ret;//hopefully will never happen, but if there arent 0 unusedpatients left in the pool they will just have no option (automatically proceed)
        }
        availPatients.AddRange(unusedPatients);


        for (int i = 0; i < count; i++)
        {
            int r = Random.Range(0, availPatients.Count);
            ret.Add(availPatients[r]);

            availPatients.Remove(availPatients[r]);
        }
        Debug.Log(ret);
        return ret;
    }

    public void selectPatient(PatientSO patient) //after a patient has been voted on, select that patient, remove it from unused patients, and add it to current patients
    {
        currentPatients.Add(patient);
        unusedPatients.Remove(patient);
    }

    public List<Patient> GetAllSpawnedPatients()
    {
        List<Patient> result = new List<Patient>();
        foreach (int id in spawnedPatients)
        {
            Patient patient = GetPatientFromId(id);
            if (patient)
            {
                result.Add(patient);
            }
        }
        return result;
    }

    [Server]
    public void SpawnAllPatients() //only call to reset
    {
        spawnedPatients.Clear(); 
        for (int i = 0; i < currentPatients.Count; i++)
        {
            SpawnPatient(i);
        }
    }

    [Server]
    public void DespawnPatient(Patient patient)
    {
        spawnedPatients.Remove(patient.GetComponent<NetworkObject>().ObjectId);
        Despawn(patient);
    }

    [Server]
    public void SpawnPatient(int index)
    {
        GameObject go = Instantiate(currentPatients[index].prefab);
        //set spawn and roombounds here, when I create the mapmanager
        go.GetComponent<Patient>().spawn = MapManager.Instance.floors[index / MapManager.patientsperfloor].patientSpawns[index % MapManager.patientsperfloor];
        go.GetComponent<Patient>().roomBounds = MapManager.Instance.floors[index / MapManager.patientsperfloor].patientRooms[index % MapManager.patientsperfloor];
        go.transform.position = go.GetComponent<Patient>().spawn.position;
        Spawn(go);
        spawnedPatients.Add(go.GetComponent<NetworkObject>().ObjectId);
    }

    [Server] // this is for patients which are spawned in during the day, ie: if a patient spawns in other patients
    public void SpawnPatientAt(GameObject patient, Transform trans)
    {
        GameObject go = Instantiate(patient);
        go.transform.position = trans.position;
        Spawn(go);
        spawnedPatients.Add(go.GetComponent<NetworkObject>().ObjectId);
    }


    public Patient GetPatientFromId(int id)
    {
        NetworkObject netObj;
        FishNet.InstanceFinder.NetworkManager.ClientManager.Objects.Spawned.TryGetValue(id, out netObj);

        if (netObj != null)
        {
            return netObj.GetComponent<Patient>();
        }
        return null;
    }
}
