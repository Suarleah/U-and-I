using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.AI;

// Base class for every patient's in-game behavior. Behavior runs as a state machine on the
// server (see Patients/States/). The patient is always in exactly one state.
//
// MAKING A NEW PATIENT: subclass this, then do one or more of the following:
//   - Override DefineInteractions() to say what each dice roll of each interaction does
//   - Override a hook: GetPatienceDrain, FindTarget, PickEscapeState, ServerPreUpdate, OnEscaped/OnContained
//   - Override BuildStates() to swap a default state for your own subclass of it
//   - Add new states and switch to them with ChangeState(...)
public class Patient : NetworkBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public PatientInteractable interactable;
    public FieldOfView sightfov;  // sees further in the direction it faces
    public FieldOfView radialfov; // small radius that always sees players
    [HideInInspector] public PatientSO patientSO; // set by PatientManager on spawn (server only)

    [Header("Room (set by PatientManager on spawn)")]
    public int room;
    public Transform spawn;
    public RectTransform roomBounds;
    public int floor;

    [Header("Patience")]
    public float maxPatience;
    public float patienceDecay;               // patience lost per second while contained
    public float followingDrainDivisor = 10f; // following drains this many times slower
    public float escapeTime;                  // how long an escape lasts before it recontains itself
    public readonly SyncVar<float> patience = new SyncVar<float>();

    [Header("Health")]
    public int maxHealth;
    public readonly SyncVar<int> health = new SyncVar<int>();

    [Header("Movement")]
    public float speed;
    public float followingSpeed;
    public float chaseSpeed;
    public float angularSpeed;
    public float followDistance = 5f;
    public float wanderCooldownMin;
    public float wanderCooldownMax;
    public float escapedWanderRange;
    public float escapedWanderCooldownMin;
    public float escapedWanderCooldownMax;

    [Header("Combat")]
    public float damage;
    public float attackRange = 1f;
    public float attackCD;    // pause after landing a hit so it can't hit every frame
    public float aggroLength; // how long Hunt tracks a target it can't see before giving up

    [Header("Being Looked At (for patients that use IsLookedAtBy)")]
    [Range(-1f, 1f)] public float viewThreshold;
    public float maxViewDist;
    public LayerMask viewObstructingLayer;

    [Header("Runtime (debug)")]
    public string currentStateName;
    public GameObject aggroedPlayer;
    public GameObject followingPlayer;
    public float localpatience; // Inspector mirrors of the SyncVars
    public float localhealth;

    // Synced so clients can drive animations and sounds from the current state.
    public readonly SyncVar<string> stateName = new SyncVar<string>();

    // ─────────────────────────── States ───────────────────────────
    // These only exist on the server.

    public PatientState Current { get; private set; }
    public PatientState Contained { get; protected set; }
    public PatientState Following { get; protected set; }
    public PatientState Wander { get; protected set; }
    public PatientState Chase { get; protected set; }
    public PatientState Hunt { get; protected set; }
    public PatientState AttackCooldown { get; protected set; }

    public bool IsEscaped => Current is EscapedState;

    // Override to swap a default state (call base first) or to create extra states of your own.
    protected virtual void BuildStates()
    {
        Contained = new ContainedState(this);
        Following = new FollowingState(this);
        Wander = new EscapedWanderState(this);
        Chase = new ChaseState(this);
        Hunt = new HuntState(this);
        AttackCooldown = new AttackCooldownState(this);
    }

    public void ChangeState(PatientState next)
    {
        Current?.Exit();
        Current = next;
        stateName.Value = next.Name;
        Current.Enter();
    }

    // ─────────────────────────── Lifecycle ───────────────────────────

    public override void OnStartServer()
    {
        base.OnStartServer();
        patience.Value = maxPatience;
        health.Value = maxHealth;

        BuildStates();
        ChangeState(Contained);
        DefineInteractions();

        if (patientSO) // null for patients spawned mid-day through SpawnPatientAt
        {
            AddNotebookInfo(patientSO.log1);
            AddNotebookInfo(patientSO.log2);
        }
    }

    public virtual void Awake()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.angularSpeed = angularSpeed;
        if (spawn)
        {
            agent.Warp(spawn.position);
        }
    }

    private void Update()
    {
        localpatience = patience.Value;
        localhealth = health.Value;
        currentStateName = stateName.Value;
        if (!IsServerStarted || Current == null)
        {
            return;
        }

        ServerPreUpdate();
        Current.Tick();

        patience.Value = Mathf.Min(patience.Value, maxPatience);
    }

    // ─────────────────────── Hooks (override these) ───────────────────────

    // Runs every server frame before the current state ticks. Use it for checks that apply
    // in every state, and for "from any state" transitions into your own custom states.
    protected virtual void ServerPreUpdate() { }

    // Patience lost per second while contained. Following loses this / followingDrainDivisor.
    public virtual float GetPatienceDrain() => patienceDecay;

    // Who this patient would go after right now, or null. Used by Wander, Chase, and Hunt.
    public virtual GameObject FindTarget()
    {
        Transform t = FindClosestVisiblePlayer();
        return t ? t.gameObject : null;
    }

    // The state the patient enters the moment it escapes.
    // Default: go after the nearest living player. Hunt walks to where they are, then tracks them for a bit.
    protected virtual PatientState PickEscapeState()
    {
        PlayerMovement nearest = FindNearestLivingPlayer();
        if (!nearest)
        {
            return Wander;
        }
        aggroedPlayer = nearest.gameObject;
        return Hunt;
    }

    protected virtual void OnEscaped() { }   // one-time trigger right after escaping
    protected virtual void OnContained() { } // one-time trigger right after being contained

    // ─────────────────────── Escape / Contain ───────────────────────

    [Server]
    public void Escape()
    {
        if (followingPlayer)
        {
            followingPlayer.GetComponent<PlayerMovement>().releaseFollower();
            followingPlayer = null;
        }
        patience.Value = 0;

        ChangeState(PickEscapeState());
        EscapeAllClients();
        OnEscaped();
    }

    [Server]
    public void Contain()
    {
        patience.Value = maxPatience;
        aggroedPlayer = null;
        agent.Warp(spawn.position);

        ChangeState(Contained);
        ContainAllClients();
        OnContained();
    }

    [ObserversRpc]
    public virtual void ContainAllClients()
    {
        interactable.enabled = true;
    }

    [ObserversRpc]
    public virtual void EscapeAllClients()
    {
        interactable.enabled = false;
        interactable.closeInteractionPrompt();
        interactable.Close();
    }

    [Server]
    public virtual void changePatience(float amt)
    {
        patience.Value += amt;
    }

    [ObserversRpc]
    public void AddNotebookInfo(string info)
    {
        if (PlayerMovement.LocalInstance != null)
        {
            PlayerMovement.LocalInstance.myNotebook.AddInfoToPatient(patientSO, info);
        }
    }

    // ─────────────────────── Interactions ───────────────────────
    // Each interaction is 6 outcomes, one per dice roll. An outcome is a function that receives
    // the player who used it and can do anything: change numbers, ChangeState(...), Escape(),
    // spawn things, and so on. Outcomes run on the server.

    public delegate void RollOutcome(GameObject player);

    private readonly Dictionary<string, RollOutcome[]> interactions = new Dictionary<string, RollOutcome[]>();

    // Override in each patient and call DefineInteraction once per interaction button.
    protected virtual void DefineInteractions() { }

    // Outcomes are listed in roll order: the first one is a roll of 1, the last one is a roll of 6.
    protected void DefineInteraction(string interactionName, params RollOutcome[] rolls)
    {
        if (rolls.Length != 6)
        {
            Debug.LogError($"{name}: '{interactionName}' has {rolls.Length} outcomes, expected 6");
        }
        interactions[interactionName] = rolls;
    }

    // Called by PatientInteractable on the server when a player uses an interaction button.
    [Server]
    public void ResolveInteraction(string interactionName, int roll, GameObject player)
    {
        if (interactions.TryGetValue(interactionName, out RollOutcome[] rolls) && roll >= 1 && roll <= rolls.Length)
        {
            rolls[roll - 1](player);
        }
        else
        {
            Debug.LogWarning($"{name}: no '{interactionName}' outcome for a roll of {roll}");
        }
    }

    // Shortcut for a roll that only changes numbers: Numbers(credits, patience, playerHealth).
    protected RollOutcome Numbers(int credits, float patienceChange, int playerHealth) =>
        player => ApplyNumbers(player, credits, patienceChange, playerHealth);

    // Use this inside a custom outcome when it should also change numbers.
    protected void ApplyNumbers(GameObject player, int credits, float patienceChange, int playerHealth)
    {
        if (credits > 0) GameManager.Instance.AddCredits(credits);
        else if (credits < 0) GameManager.Instance.SubtractCredits(-credits);

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (playerHealth > 0) stats.Heal(playerHealth, new DamageDetails());
        else if (playerHealth < 0) stats.TakeDamage(-playerHealth, new DamageDetails());

        changePatience(patienceChange);
    }

    // ─────────────────────── Helpers (used by states) ───────────────────────

    // Drains patience by GetPatienceDrain() / divisor. When it runs out, escapes and returns true.
    public bool DrainPatience(float divisor = 1f)
    {
        patience.Value -= GetPatienceDrain() / divisor * Time.deltaTime;
        if (patience.Value > 0)
        {
            return false;
        }
        Escape();
        return true;
    }

    // Escape timer: patience refills over escapeTime. When it's full, contains and returns true.
    public bool TickEscapeTimer()
    {
        patience.Value += maxPatience / escapeTime * Time.deltaTime;
        if (patience.Value < maxPatience)
        {
            return false;
        }
        Contain();
        return true;
    }

    // Hits the target if it's within attackRange. Returns true if it hit.
    public bool TryAttack(GameObject target)
    {
        if (Vector2.Distance(target.transform.position, transform.position) >= attackRange)
        {
            return false;
        }
        target.GetComponent<PlayerStats>().TakeDamage((int)damage, new DamageDetails());
        return true;
    }

    public bool ReachedDestination() =>
        !agent.pathPending
        && agent.remainingDistance <= agent.stoppingDistance
        && (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.5f);

    public static bool IsDead(GameObject player) => player.GetComponent<PlayerStats>().isDead.Value;

    public Vector3 RandomPointInRoom()
    {
        Vector3[] corners = new Vector3[4];
        roomBounds.GetWorldCorners(corners);
        return new Vector3(
            Random.Range(corners[0].x, corners[2].x),
            Random.Range(corners[0].y, corners[2].y),
            transform.position.z);
    }

    // A random reachable point within escapedWanderRange, or false if none was found this try.
    public bool TryGetEscapedWanderPoint(out Vector3 point)
    {
        point = default;
        Vector3 random = transform.position + (Vector3)(Random.insideUnitCircle * escapedWanderRange);
        if (!NavMesh.SamplePosition(random, out NavMeshHit hit, escapedWanderRange, NavMesh.AllAreas))
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        point = hit.position;
        return true;
    }

    // Closest living player seen by either field of view.
    public Transform FindClosestVisiblePlayer()
    {
        radialfov.FindVisibleTargets();
        sightfov.FindVisibleTargets();

        Transform closest = null;
        float best = float.MaxValue;
        foreach (Transform hit in radialfov.visibleTargets.Concat(sightfov.visibleTargets))
        {
            // The FOV can hit the client hitbox child instead of the player root.
            Transform player = hit.GetComponent<PlayerStats>() ? hit : hit.parent;
            if (IsDead(player.gameObject)) continue;

            float d = Vector2.Distance(player.position, transform.position);
            if (d < best)
            {
                best = d;
                closest = player;
            }
        }
        return closest;
    }

    public PlayerMovement FindNearestLivingPlayer()
    {
        PlayerMovement nearest = null;
        float best = float.MaxValue;
        foreach (PlayerMovement p in GameManager.Instance.GetPlayers())
        {
            if (p.stats.isDead.Value) continue;
            float d = Vector2.Distance(p.transform.position, transform.position);
            if (d < best)
            {
                best = d;
                nearest = p;
            }
        }
        return nearest;
    }

    // True if the player is alive, facing this patient, within maxViewDist, and has a clear view.
    public bool IsLookedAtBy(PlayerMovement p)
    {
        if (p.stats.isDead.Value)
        {
            return false;
        }

        Vector2 toMe = transform.position - p.transform.position;
        float dist = toMe.magnitude;
        if (dist > maxViewDist)
        {
            return false;
        }

        // visual.localScale.x is -1 when the player sprite is flipped to face left
        float facing = Vector2.Dot(p.transform.right, toMe.normalized) * p.visual.transform.localScale.x;
        if (facing < viewThreshold)
        {
            return false;
        }

        return Physics2D.Raycast(p.transform.position, toMe.normalized, dist, viewObstructingLayer).collider == null;
    }

    public List<PlayerMovement> GetPlayersLookingAtMe()
    {
        List<PlayerMovement> result = new List<PlayerMovement>();
        foreach (PlayerMovement p in GameManager.Instance.GetPlayers())
        {
            if (IsLookedAtBy(p)) result.Add(p);
        }
        return result;
    }

    // Counts players and other patients within range of this patient.
    public int CountEntitiesWithin(float range)
    {
        int count = 0;
        foreach (PlayerMovement p in GameManager.Instance.GetPlayers())
        {
            if (Vector2.Distance(p.transform.position, transform.position) <= range) count++;
        }
        foreach (Patient other in PatientManager.Instance.GetAllSpawnedPatients())
        {
            if (other != this && Vector2.Distance(other.transform.position, transform.position) <= range) count++;
        }
        return count;
    }
}