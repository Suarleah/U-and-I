using UnityEngine;
using System.Collections;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;


//this is a class for the patient's actual ingame behavior, so movement, attacks, health, etc.
public class Patient : NetworkBehaviour
{
    public NavMeshAgent agent;

    public PatientInteractable interactable;

    public int room; //the room # they are staying at
    public Transform spawn; // where the patient spawns, usually located in their room, and it's where they return after containment
    public RectTransform roomBounds; //for patientAI while contained, they can't leave their room


    public bool escaped; //whether or not they're currently escaped

    public float maxPatience; //most patients will use this to determine how close they are to escape
    public readonly SyncVar<float> patience = new SyncVar<float>();
    public float localpatience;

    public int maxHealth; //for patients who can get damaged, usually dropping hp to zero will contain them
    public readonly SyncVar<int> health = new SyncVar<int>();
    public float localhealth;

    public float damage;
    public float speed;
    public float chaseSpeed;


    public float wanderCooldownMin; //how long in between wandering it waits,, it's a range so its a bit more random
    public float wanderCooldownMax;
    public bool wanderUp = true;

    public int floor; //the floor this patient is currently on

    //generally escaped behavior
    public float escapedWanderRange;
    public float escapedWanderCooldownMin; //how long in between wandering it waits,, it's a range so its a bit more random
    public float escapedWanderCooldownMax;

    public GameObject aggroedPlayer; //if patient is escaped and aggroed, the player will be stored here
    public FieldOfView sightfov; // the patient sees furhter in the direction theyre facing
    public FieldOfView radialfov; // tha patient has a small radius around them that they will always be able to see players
    public float aggroLength;//the patient will stay aggroed until they reach the target's lest seen location. Then, they will have basically the equivalent of cheating, chasing down the player for this amount of time before deaggroing
    public float aggrotimer; 
    public bool cheating;
    public float attackCD;
    public float attackTimer; //the patient will have to wait between attacks so they dont always just one shot.


    public override void OnStartServer()
    {
        base.OnStartServer();
        // Initialize the server variable
        patience.Value = maxPatience;
        health.Value = maxHealth;
    }

    public virtual void Awake()
    {
        if (spawn)
        {
            transform.position = spawn.position;
        }
        
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        if (agent.isOnNavMesh && IsServerStarted)
        {
            agent.SetDestination(transform.position);
        }
        
    }

    public virtual void Update()
    {
        localpatience = patience.Value;
        localhealth = health.Value;
        if (!IsServerStarted)
        {
            return;
        }
        if (aggroedPlayer)
        {
            agent.speed = chaseSpeed;
        } else
        {
            agent.speed = speed;
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
        
    }


    //method to be overwritten by subclass, what the patient does on update when they are contained
    public virtual void ContainedUpdate()
    {
        patience.Value -= Time.deltaTime;
        aggroedPlayer = null;
        if (patience.Value <= 0)
        {
            StartCoroutine(Escape());
        }
        else if (wanderUp)
        {
            StartCoroutine(roomWander());
        }
        
    }

    public virtual IEnumerator Contain() //recontain a patient  on server
    {
        

        if (!IsServerStarted)
        {
            yield break; ;
        }
        escaped = false;
        patience.Value = maxPatience;
        transform.position = spawn.position;
        attackTimer = attackCD;
        aggrotimer = aggroLength;
        wanderUp = true;
        
        ContainAllClients();
  
        
    }

    [ObserversRpc]
    public virtual void ContainAllClients() //functions that contain has to do for all clients
    {
        interactable.enabled = true; //locally re enable interactions
    }

    public virtual IEnumerator Escape()//patient escapes;
    {
        
        if (!IsServerStarted)
        {
           yield break;
        }
   
        escaped = true;
        patience.Value = 0;
        wanderUp = true; 


        cheating = false;
        attackTimer = attackCD;
        aggrotimer = aggroLength;
        //find the closest player on the whole map and chase them for a little (to give the escape a slightly explosive start ya know)
        PlayerMovement closestPlayer = null;
        List<PlayerMovement> players = GameManager.Instance.GetPlayers().ToList<PlayerMovement>(); 
        for (int i = 0; i < players.Count; i++)
        {
            if (!closestPlayer)
            {
                if (!players[i].stats.isDead.Value)
                {
                    closestPlayer = players[i];
                }
            } else
            {
                if (Vector3.Distance(closestPlayer.transform.position, transform.position) > Vector3.Distance(players[i].transform.position, transform.position))
                {
                    closestPlayer = players[i];
                }
            }
        }
        aggroedPlayer = closestPlayer.gameObject;
        agent.SetDestination(closestPlayer.transform.position);


        
        EscapeAllClients();
        
    }

    [ObserversRpc]
    public virtual void EscapeAllClients() //functions that contain has to do for all clients
    {
        interactable.enabled = false; //locally disable all interactions
        interactable.closeInteractionPrompt();
        interactable.Close();

    }


    [Server]
    public virtual void changePatience(float amt)
    {
        patience.Value+=amt;
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
        
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * escapedWanderRange;
        NavMeshHit hit;

        // Project that point onto the NavMesh within the specified range
        if (NavMesh.SamplePosition(randomPoint, out hit, escapedWanderRange, NavMesh.AllAreas))
        {
            NavMeshPath path = new NavMeshPath();

            //check if path is possible
            agent.CalculatePath(hit.position, path);
            if (path.status == NavMeshPathStatus.PathInvalid || path.status == NavMeshPathStatus.PathPartial)
            {
                // Cancel the path and stop the agent
                agent.ResetPath();
            }
            else
            {
                // Path is possible, set the destination
                agent.SetDestination(hit.position);
                wanderUp = false;
                yield return new WaitForSeconds(Random.Range(escapedWanderCooldownMin, escapedWanderCooldownMax));
                wanderUp = true;
            }
        }
    }

    public virtual Transform FindClosestPlayer(){
        Transform closestPlayer = null;
        radialfov.FindVisibleTargets();
        if (radialfov.visibleTargets.Count > 0)
        {
            closestPlayer = radialfov.visibleTargets[0];
            for (int i = 1; i < radialfov.visibleTargets.Count; i++)
            {
                if (Vector3.Distance(radialfov.visibleTargets[i].position, transform.position) < Vector3.Distance(closestPlayer.position, transform.position))
                {
                    closestPlayer = radialfov.visibleTargets[i];
                }
            }
        }
        sightfov.FindVisibleTargets();
        if (sightfov.visibleTargets.Count > 0)
        {
            closestPlayer = sightfov.visibleTargets[0];
            for (int i = 1; i < sightfov.visibleTargets.Count; i++)
            {
                if (Vector3.Distance(sightfov.visibleTargets[i].position, transform.position) < Vector3.Distance(closestPlayer.position, transform.position))
                {
                    closestPlayer = sightfov.visibleTargets[i];
                    
                }
            }
        }
        if (closestPlayer)
        {
            if (!closestPlayer.GetComponent<PlayerStats>())
            {
                closestPlayer = closestPlayer.transform.parent;
            }
        }
        
        return closestPlayer;
    }

}
