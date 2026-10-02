using UnityEngine;

// Chasing aggroedPlayer. When in range it attacks and goes to Recover.
// Target out of sight → Hunt (unless ContinueWithoutSight). Target dead → Wander.
public class ChaseState : EscapedState
{
    public ChaseState(Patient patient) : base(patient) { }

    // Override to true for patients that never lose track of their target.
    protected virtual bool ContinueWithoutSight => false;

    public override void Enter()
    {
        Agent.speed = patient.chaseSpeed;
    }

    protected override void EscapedTick()
    {
        GameObject seen = patient.FindTarget();
        if (seen)
        {
            patient.aggroedPlayer = seen; // switch to whoever is closest and visible
        }

        GameObject target = patient.aggroedPlayer;
        if (!target || Patient.IsDead(target))
        {
            patient.ChangeState(patient.Wander);
            return;
        }
        if (!seen && !ContinueWithoutSight)
        {
            patient.ChangeState(patient.Hunt);
            return;
        }

        Agent.SetDestination(target.transform.position);
        if (patient.TryAttack(target))
        {
            patient.ChangeState(patient.AttackCooldown);
        }
    }
}