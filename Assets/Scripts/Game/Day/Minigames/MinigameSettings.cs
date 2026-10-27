using System;
using System.Collections.Generic;
using UnityEngine;

// Per-patient difficulty for each minigame. Every Patient has one of each,
// and the minigame reads them in Configure() right before it opens.
// The defaults here are what a patient gets if you never touch its Inspector.

[Serializable]
public class ObserveSettings
{
    public float spotlightSize = 1f;
    public float spotlightFollowSpeed = 6f;
    public float patientSpeed = 200f;
    public Vector2 wanderInterval = new Vector2(1f, 3f);
    public float parTime = 10f;
    public float secondsPerRollLost = 3f;

    public ObserveSettings Clone() => (ObserveSettings)MemberwiseClone();
}

[Serializable]
public class WiresSettings
{
    public int wireCount = 4;

    public WiresSettings Clone() => (WiresSettings)MemberwiseClone();
}

[Serializable]
public class LobotomySettings
{
    public float irritationSpeed = 0.15f;
    public float maxOffCurve = 30f;
    public float twitchThreshold = 0.6f;
    public float blinkDuration = 0.75f;
    public float maxSpeed = 400f;

    [Tooltip("Curve to trace, local to the eye. Leave empty to use the default curve (the eye's children). " +
             "Author one with the minigame's \"Copy Children To Vector Points\" context menu, then paste it here.")]
    public List<Vector2> curve = new List<Vector2>();

    public LobotomySettings Clone() => (LobotomySettings)MemberwiseClone();
}

[Serializable]
public class TherapySettings
{
    public float timeGiven = 10f;
    public float amplitudeTolerance = 0.1f;
    public float frequencyTolerance = 0.1f;

    public TherapySettings Clone() => (TherapySettings)MemberwiseClone();
}