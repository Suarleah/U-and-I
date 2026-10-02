using UnityEngine;

// Escaped with no target: roams nearby, and switches to Chase as soon as it finds someone.
public class EscapedWanderState : EscapedState
{
    private float wanderTimer;

    public EscapedWanderState(Patient patient) : base(patient) { }

    public override void Enter()
    {
        Agent.speed = patient.speed;
        patient.aggroedPlayer = null;
        wanderTimer = 0f;
    }

    protected override void EscapedTick()
    {
        GameObject target = patient.FindTarget();
        if (target)
        {
            patient.aggroedPlayer = target;
            patient.ChangeState(patient.Chase);
            return;
        }

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f && patient.TryGetEscapedWanderPoint(out Vector3 point))
        {
            Agent.SetDestination(point);
            wanderTimer = Random.Range(patient.escapedWanderCooldownMin, patient.escapedWanderCooldownMax);
        }
    }
}