using UnityEngine.AI;

// One thing a patient can be doing. States only run on the server.
//   Enter: called once when the patient switches into this state
//   Tick:  called every server frame while in this state; transitions go here (patient.ChangeState)
//   Exit:  called once when leaving
public abstract class PatientState
{
    protected readonly Patient patient;
    protected NavMeshAgent Agent => patient.agent;

    protected PatientState(Patient patient)
    {
        this.patient = patient;
    }

    public virtual string Name => GetType().Name;
    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void Exit() { }
}

// Base for every escaped state. It runs the escape timer first and recontains the patient
// when the timer is done, so escaped states only need to write EscapedTick.
public abstract class EscapedState : PatientState
{
    protected EscapedState(Patient patient) : base(patient) { }

    public sealed override void Tick()
    {
        if (patient.TickEscapeTimer()) return; // escape over, patient is now Contained
        EscapedTick();
    }

    protected abstract void EscapedTick();
}