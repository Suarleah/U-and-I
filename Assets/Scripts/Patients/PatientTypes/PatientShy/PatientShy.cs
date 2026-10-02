using System.Collections.Generic;
using UnityEngine;
using static PatientInteractions;

// Hates company: loses patience for each player or patient within shyRange, and nothing otherwise.
// When escaped, it only goes after players who look at it, and never loses them once it does.
public class PatientShy : Patient
{
    public float shyRange;

    protected override void BuildStates()
    {
        base.BuildStates();
        Chase = new RelentlessChaseState(this);
    }

    public override float GetPatienceDrain() => patienceDecay * CountEntitiesWithin(shyRange);

    public override GameObject FindTarget()
    {
        List<PlayerMovement> watchers = GetPlayersLookingAtMe();
        return watchers.Count > 0 ? watchers[0].gameObject : null;
    }

    protected override PatientState PickEscapeState() => Wander; // waits until someone looks at it

    protected override void DefineInteractions()
    {
        DefineInteraction(Observe,       Numbers(20, -50, -50), Numbers(20, -50, -25), Numbers(20, -25, -10), Numbers(30, -15, 0),  Numbers(30, -10, 0), Numbers(30, 0, 0));
        DefineInteraction(Bribe,         Numbers(10, -30, -50), Numbers(10, -20, -30), Numbers(10, 0, -20),   Numbers(10, 10, -10), Numbers(10, 10, 0),  Numbers(10, 20, 0));
        DefineInteraction(Therapy,       Numbers(20, -30, -30), Numbers(20, -15, -10), Numbers(20, 10, 0),    Numbers(25, 10, 0),   Numbers(30, 20, 10), Numbers(30, 30, 20));
        DefineInteraction(ElectricChair, Numbers(20, -30, -30), Numbers(20, -5, -10),  Numbers(20, 0, 0),     Numbers(20, 0, 0),    Numbers(35, 0, 0),   Numbers(50, 20, 0));
    }
}

// Chase that keeps going even when the target isn't looking anymore.
public class RelentlessChaseState : ChaseState
{
    public RelentlessChaseState(Patient patient) : base(patient) { }
    protected override bool ContinueWithoutSight => true;
}