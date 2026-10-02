using UnityEngine;

// In its room: loses patience over time and wanders around the room. Escapes when patience runs out.
public class ContainedState : PatientState
{
    private float wanderTimer;

    public ContainedState(Patient patient) : base(patient) { }

    public override void Enter()
    {
        Agent.speed = patient.speed;
        wanderTimer = 0f;
    }

    public override void Tick()
    {
        if (patient.followingPlayer)
        {
            patient.ChangeState(patient.Following);
            return;
        }
        if (patient.DrainPatience()) return; // escaped

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            Agent.SetDestination(patient.RandomPointInRoom());
            wanderTimer = Random.Range(patient.wanderCooldownMin, patient.wanderCooldownMax);
        }
    }
}