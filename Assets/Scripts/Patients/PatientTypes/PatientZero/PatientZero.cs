using static PatientInteractions;

// The baseline patient. Uses every default state and hook.
public class PatientZero : Patient
{
    protected override void DefineInteractions()
    {
        // Each line: outcomes for rolls 1 through 6. Numbers(credits, patience, playerHealth).
        DefineInteraction(Observe,       Numbers(10, -20, -20), Numbers(10, -20, -10), Numbers(10, -10, 0), Numbers(15, -5, 0),   Numbers(15, 0, 0),    Numbers(20, 0, 0));
        DefineInteraction(Bribe,         Numbers(10, -30, -15), Numbers(10, -20, 0),   Numbers(10, -10, 0), Numbers(10, 0, 0),    Numbers(10, 5, 5),    Numbers(10, 10, 10));
        DefineInteraction(Therapy,       Numbers(10, -10, -20), Numbers(10, 0, -10),   Numbers(10, 10, 0),  Numbers(10, 15, 0),   Numbers(10, 20, 0),   Numbers(20, 30, 0));
        DefineInteraction(ElectricChair, Numbers(10, -50, -50), Numbers(15, -35, -35), Numbers(20, -20, -20), Numbers(20, -15, -10), Numbers(25, -10, -10), Numbers(35, -10, -10));
    }
}