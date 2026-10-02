using UnityEngine;

// Just landed a hit: stands still for attackCD seconds so it can't hit every frame, then resumes the chase.
public class AttackCooldownState : EscapedState
{
    private float timer;

    public AttackCooldownState(Patient patient) : base(patient) { }

    public override void Enter()
    {
        timer = patient.attackCD;
        Agent.ResetPath();
    }

    protected override void EscapedTick()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            patient.ChangeState(patient.Chase);
        }
    }
}