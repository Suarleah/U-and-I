using UnityEngine;
using System.Collections.Generic;

//this is a script to keep track of patients which the players have accumulated so far
public class PatientManager : MonoBehaviour
{
    public static PatientManager Instance;
    public List<PatientSO> allPatients; //all patients that exist in the game

    public List<PatientSO> currentPatients; //patients the players currently have

    public List<PatientSO> unusedPatients; //patients the players do not currently have
    private List<PatientSO> availPatients; //used only for the GetRandomUnusedPatients method, keeps track of which patients are being selected as well (so that they dont have the same option repeated)

    private void Awake()
    {
        Instance = this;
       // unusedPatients.AddRange(allPatients);
       availPatients = new List<PatientSO>();
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

}
