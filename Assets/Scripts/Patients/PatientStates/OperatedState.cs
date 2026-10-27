// when the patient is being operated on. Stands still and doesn't lose patience.
// The OperatingTable moves the patient in and out of this state through
// Patient.StartOperation / Patient.EndOperation.
public class OperatedState : PatientState
{
    public OperatedState(Patient patient) : base(patient) { }

    public override void Enter()
    {
        Agent.ResetPath();
    }

    public override void Tick()
    {
        // intentionally empty: no patience drain, no movement, maybe can have like a cisual effect
    }
}