using static PatientInteractions;

// Freezes, and stops losing patience, while any player is looking at it.
public class PatientBoo : Patient
{
    private bool isWatched;

    protected override void ServerPreUpdate()
    {
        isWatched = GetPlayersLookingAtMe().Count > 0;
        agent.isStopped = isWatched;
    }

    public override float GetPatienceDrain() => isWatched ? 0f : base.GetPatienceDrain();

    protected override void DefineInteractions()
    {
        DefineInteraction(Observe,       Numbers(10, -10, -10), Numbers(10, 0, -10),   Numbers(10, 10, 0),  Numbers(10, 20, 0),   Numbers(10, 30, 0),  Numbers(10, 35, 0));
        DefineInteraction(Bribe,         Numbers(10, -30, -50), Numbers(10, -20, -30), Numbers(10, 0, -20), Numbers(10, 10, -10), Numbers(10, 10, 0),  Numbers(10, 20, 0));
        DefineInteraction(Therapy,       Numbers(10, -10, -10), Numbers(10, 0, 0),     Numbers(15, 10, 0),  Numbers(20, 10, 0),   Numbers(20, 20, 0),  Numbers(30, 30, 10));
        DefineInteraction(ElectricChair, Numbers(10, -10, -10), Numbers(10, -5, -10),  Numbers(10, 0, 0),   Numbers(10, 5, 0),    Numbers(10, 10, 0),  Numbers(10, 20, 10));
    }
}