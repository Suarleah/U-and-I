using UnityEngine;

// Following a player around. Loses patience slower than when contained.
public class FollowingState : PatientState
{
    public FollowingState(Patient patient) : base(patient) { }

    public override void Enter()
    {
        Agent.speed = patient.followingSpeed;
    }

    public override void Tick()
    {
        if (!patient.followingPlayer)
        {
            patient.ChangeState(patient.Contained);
            return;
        }
        if (patient.DrainPatience(patient.followingDrainDivisor)) return; // escaped

        Vector3 target = patient.followingPlayer.transform.position;
        if (Vector2.Distance(target, patient.transform.position) > patient.followDistance)
        {
            Agent.SetDestination(target);
        }
        else
        {
            Agent.ResetPath();
        }
    }
}