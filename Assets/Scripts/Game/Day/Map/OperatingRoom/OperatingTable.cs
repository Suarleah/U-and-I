using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class OperatingTable : Interactable
{
    InputActionAsset MinigameInputs;
    public ConnectWires wiresminigame;
    public TherapyMinigame therapyminigame;
    public ObserveMinigame observeMinigame;
    public LobotomyMinigame lobotomyMinigame;

    public enum Operation
    {
        Observe = 0,
        Therapy = 1,
        Lobotomy = 2,
        Electric_Chair = 3
    }

    public TextMeshProUGUI operationTooltipText;

    public Operation mode;

    // true while any player is mid-minigame on this table
    public readonly SyncVar<bool> inUse = new SyncVar<bool>();

    // local (client side) info about the operation this client is running
    private MinigameBase activeMinigame;
    private Patient operatingPatient;
    private string operatingInteraction;
    public float operatingRange = 20f; // how close the patient must be to the table to operate on

    void Update()
    {
        localOnCD = onCD.Value;
        if (closest)
        {
            c.gameObject.SetActive(true);
            if (Vector2.Distance(gameObject.transform.position, player.transform.position) > 5f)
            {
                closest = false;
            }
        }
        else
        {
            c.gameObject.SetActive(false);
        }

        if (Vector3.Distance(player.transform.position, transform.position) > 10f)
        {
            Close();
        }
        operationTooltipText.text = "press \"E\" to perform " + mode.ToString() + ".";
    }

    public override void Interact()
    {
        if (onCD.Value)
        {
            GiveFeedback("On Cooldown!");
            return;
        }
        if (inUse.Value)
        {
            GiveFeedback("Table in use!");
            return;
        }

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        Patient patient = pm.followingPatient;
        if (!patient)
        {
            GiveFeedback("No patient following!!");
            return;
        }

        if (Vector2.Distance(patient.transform.position, transform.position) > operatingRange)
        {
            GiveFeedback("Patient too far away!");
            return;
        }

        MinigameBase game = GetMinigame(mode);
        if (!game)
        {
            return;
        }

        operatingPatient = patient;
        operatingInteraction = GetInteractionName(mode);
        activeMinigame = game;

        UIManager.Instance.currentInteraction = this;
        interacting = true;
        pm.canMove = false;

        BeginOperation(patient);

        game.onFinished = OnMinigameFinished;
        game.Open();
    }

    // Called by the minigame (through MinigameBase.Finish) with a roll from 1 to 6.
    private void OnMinigameFinished(int roll)
    {
        SubmitOperationResult(operatingPatient, operatingInteraction, roll, player);
        GiveFeedback("Rolled a " + roll);

        activeMinigame = null; // cleared before Close() so Close doesn't treat this as a cancel
        Close();
    }

    public override void Close()
    {
        // Closing while a minigame is still open (pressing E, walking away) cancels the operation.
        if (activeMinigame)
        {
            activeMinigame.Cancel();
            activeMinigame = null;
            CancelOperation(operatingPatient);
        }

        operatingPatient = null;
        operatingInteraction = null;
        player.GetComponent<PlayerMovement>().canMove = true;

        closest = false;
        interacting = false;

        if (UIManager.Instance.currentInteraction == this)
        {
            UIManager.Instance.currentInteraction = null;
        }
    }

    // ─────────────────────── Server side ───────────────────────

    [ServerRpc(RequireOwnership = false)]
    private void BeginOperation(Patient patient)
    {
        if (!patient)
        {
            return;
        }
        if (Vector2.Distance(patient.transform.position, transform.position) > operatingRange)
        {
            return;
        }
        if (!patient.StartOperation())
        {
            return;
        }
        inUse.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitOperationResult(Patient patient, string interactionName, int roll, GameObject p)
    {
        inUse.Value = false;
        StartCoroutine(goOnCooldown(cooldown));

        if (!patient || !patient.IsBeingOperated)
        {
            return;
        }

        // End the operation BEFORE resolving, so an outcome that calls Escape() or
        // ChangeState(...) isn't immediately overwritten by EndOperation.
        patient.EndOperation();
        patient.ResolveInteraction(interactionName, roll, p);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CancelOperation(Patient patient)
    {
        inUse.Value = false;
        if (patient)
        {
            patient.EndOperation();
        }
    }

    // ─────────────────────── Helpers ───────────────────────

    private MinigameBase GetMinigame(Operation op)
    {
        switch (op)
        {
            case Operation.Observe:        return observeMinigame;
            case Operation.Therapy:        return therapyminigame;
            case Operation.Lobotomy:       return lobotomyMinigame;
            case Operation.Electric_Chair: return wiresminigame;
        }
        return null;
    }

    private string GetInteractionName(Operation op)
    {
        switch (op)
        {
            case Operation.Observe:        return PatientInteractions.Observe;
            case Operation.Therapy:        return PatientInteractions.Therapy;
            case Operation.Lobotomy:       return PatientInteractions.Lobotomy;
            case Operation.Electric_Chair: return PatientInteractions.ElectricChair;
        }
        return null;
    }
}