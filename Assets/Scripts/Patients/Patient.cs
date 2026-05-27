using UnityEngine;
using System.Collections;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.AI;


//this is a class for the patient's actual ingame behavior, so movement, attacks, health, etc.
public class Patient : NetworkBehaviour
{
    public NavMeshAgent agent;

    public Interactable interactable;

    public int room; //the room # they are staying at
    public Transform spawn; // where the patient spawns, usually located in their room, and it's where they return after containment
    public RectTransform roomBounds; //for patientAI while contained, they can't leave their room


    public bool escaped; //whether or not they're currently escaped

    public float maxPatience; //most patients will use this to determine how close they are to escape
    public readonly SyncVar<float> patience = new SyncVar<float>(); 

    public int maxHealth; //for patients who can get damaged, usually dropping hp to zero will contain them
    public readonly SyncVar<int> health = new SyncVar<int>();

    public float wanderCooldownMin; //how long in between wandering it waits,, it's a range so its a bit more random
    public float wanderCooldownMax;
    public bool wanderUp = true;

    public float escapedWanderRange;
    public float escapedWanderCooldownMin; //how long in between wandering it waits,, it's a range so its a bit more random
    public float escapedWanderCooldownMax;

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Initialize the server variable
        patience.Value = maxPatience;
        health.Value = maxHealth;
    }

    public virtual void Awake()
    {

        transform.position = spawn.position;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.SetDestination(transform.position);
    }

    public virtual void Update()
    {
        if (!IsServerStarted)
        {
            return;
        }
        if (escaped)
        {
            EscapedUpdate();
        } else
        {
            ContainedUpdate();
        }
    }

    //method to be overwritten by subclass, what the patient does on update when they are escaped
    public virtual void EscapedUpdate()
    {
        interactable.enabled = false; 
        interactable.interacting = false;
    }


    //method to be overwritten by subclass, what the patient does on update when they are contained
    public virtual void ContainedUpdate()
    {
        patience.Value -= Time.deltaTime;
        if (wanderUp)
        {
            StartCoroutine(roomWander());
        }
        if(patience.Value <= 0)
        {
            Escape();
        }
    }

    public virtual IEnumerator Contain() //recontain a patient 
    {
        escaped = false;
        patience.Value = maxPatience;
        transform.position = spawn.position;
        wanderUp = true;
        yield return null;
    }

    public virtual IEnumerator Escape()//patient escapes;
    {
        escaped = true;
        patience.Value = 0;
        wanderUp = true;
        yield return null;
    }

    public virtual IEnumerator roomWander() //just ambient movement for the patient to do while contained
    {
        wanderUp = false;
        Vector3[] corners = new Vector3[4];
        roomBounds.GetWorldCorners(corners);
        float randX = Random.Range(corners[0].x, corners[2].x);
        float randY = Random.Range(corners[0].y, corners[2].y);

        agent.SetDestination(new Vector3(randX, randY, transform.position.z));

        
        yield return new WaitForSeconds(Random.Range(wanderCooldownMin, wanderCooldownMax));
        wanderUp = true;
    }

    public virtual IEnumerator escapedWander() //just ambient movement for the patient to do while contained
    {
        wanderUp = false;
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * escapedWanderRange;
        NavMeshHit hit;

        // Project that point onto the NavMesh within the specified range
        if (NavMesh.SamplePosition(randomPoint, out hit, escapedWanderRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        yield return new WaitForSeconds(Random.Range(escapedWanderCooldownMin, escapedWanderCooldownMax));
        wanderUp = true;
    }

}
