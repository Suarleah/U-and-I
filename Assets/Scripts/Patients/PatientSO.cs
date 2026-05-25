using UnityEngine;


//this is a scriptable object for each patient, for storing their data/info/descriptions
[CreateAssetMenu(fileName = "PatientSO", menuName = "Scriptable Objects/PatientSO")]
public class PatientSO : ScriptableObject
{
    public GameObject prefab;
    public int id; // useful for finding the right patient
    public string namee; // the name of the patient, two "e"s so it doesnt hide Object.name

    public string desc; // a brief description of what the patient is for when the players are voting


    int observationLevel; //at higher observation levels, players will be able to read more info about how to manage them

    //these are the logs that they will unlock. We'll probably implement them in another way, but these are here temporarily.
    public string log1;
    public string log2;
    public string log3;
    public string log4;
    public string log5;



}
