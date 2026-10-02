using UnityEngine;


[System.Serializable]
public struct PatientInteractionInfo
{
    public string interactionName;
    public int rollValue;
    
}

public static class PatientInteractions
{
    public const string Observe = "Observe";
    public const string Bribe = "Bribe";
    public const string Therapy = "Therapy";
    public const string ElectricChair = "Electric Chair";
}