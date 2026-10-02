using UnityEngine;

// Lost sight of its target. Walks to where it last saw them, then "cheats" and tracks them
// directly for aggroLength seconds. Gives up → Wander. Sees anyone → Chase.
public class HuntState : EscapedState
{
    private bool cheating;
    private float timer;

    public HuntState(Patient patient) : base(patient) { }

    public override void Enter()
    {
        Agent.speed = patient.chaseSpeed;
        cheating = false;
        timer = patient.aggroLength;
        if (patient.aggroedPlayer)
        {
            Agent.SetDestination(patient.aggroedPlayer.transform.position); // last known position
        }
    }

    protected override void EscapedTick()
    {
        GameObject seen = patient.FindTarget();
        if (seen)
        {
            patient.aggroedPlayer = seen;
            patient.ChangeState(patient.Chase);
            return;
        }

        GameObject target = patient.aggroedPlayer;
        if (!target || Patient.IsDead(target))
        {
            patient.ChangeState(patient.Wander);
            return;
        }

        if (!cheating)
        {
            cheating = patient.ReachedDestination(); // start cheating once it reaches the last known spot
            return;
        }

        Agent.SetDestination(target.transform.position);
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            patient.ChangeState(patient.Wander);
        }
    }
}