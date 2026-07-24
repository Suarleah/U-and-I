using FishNet.Demo.AdditiveScenes;
using UnityEngine;
using UnityEngine.AI;

public class PatientBoo : Patient
{
    
    [Range(-1f, 1f)] public float viewThreshold; 
    public float maxViewDist; 
    public LayerMask viewObstructingLayer;

    public override void Update()
    {
        
        base.Update();
        //code that determines whether player has this patient in view
        foreach (PlayerMovement p in GameManager.Instance.GetPlayers())
        {
            //first check if player is facing this patient
            Transform ptf = p.transform;
            Vector2 directionToEntity = (transform.position - ptf.position).normalized;
            Vector2 playerFacingDirection = ptf.right; 
             float dotProduct = Vector2.Dot(playerFacingDirection, directionToEntity);
            float dist = Vector3.Distance(ptf.position, transform.position);
            if (dotProduct*p.visual.transform.localScale.x >= viewThreshold && dist <= maxViewDist) //use local scale for direction
            {
                //next check if theres anything obstructing the view
                Vector3 directionToTarget = transform.position - ptf.position;
                float distanceToTarget = directionToTarget.magnitude;
                if (!Physics.Raycast(ptf.position, directionToTarget.normalized, out RaycastHit hit, distanceToTarget, viewObstructingLayer))
                {
                    //patient boo freezes and patience doesnt decrease
                    agent.speed = 0f;
                    agent.angularSpeed = 0f;
                     patience.Value += Time.deltaTime; //patience decreases in the superclass so this evens it out as long as a player is looking at patient boo
                }
            }
        }

        
    }

    public override void EscapedUpdate()
    {
        /*if (!IsServerStarted)
        {
            return;
        }*/
        patience.Value += Time.deltaTime;
        if (patience.Value >= maxPatience)
        {
            aggrotimer = aggroLength;
            StartCoroutine(Contain());
            return;
        }

        if (attackTimer > 0) //if on cd, cant move (so they dont stick to the player and one shot all the time)
        {
            attackTimer -=Time.deltaTime;
            return;
        }
        //find closest player
        Transform closestPlayer = FindClosestPlayer();
        if (closestPlayer)
        {
            aggroedPlayer = closestPlayer.gameObject;
            
        }

        if (aggroedPlayer)
        {
            
            if (!closestPlayer) //the patient sees no one
            {
            
                if (aggroedPlayer.GetComponent<PlayerStats>().isDead.Value) //if the aggroed player is dead, and theres no other target, reset aggro
                {
                    cheating = false;
                    aggroedPlayer = null;
                    agent.SetDestination(transform.position);
                }
                //if it already reached player's last seen location
                if (!agent.pathPending)
                {
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.5f)
                        {
                            if (!cheating) //if not already cheating
                            {
                                cheating = true; //then cheat
                            }
                            
                        }
                    }
                }    
                if (cheating) //while cheating, the patient chases the last aggroed player even without line of sight
                {
                    
                    agent.SetDestination(aggroedPlayer.transform.position);
                    
                    aggrotimer -= Time.deltaTime; 
                } 
                

            } else
            { //if theres a closest player, that means the patient currently sees the target. Chase
                agent.SetDestination(aggroedPlayer.transform.position);
                cheating = false;
                aggrotimer = aggroLength;
                if (Vector3.Distance(aggroedPlayer.transform.position, transform.position) < 1f) //while in range, attack
                {
                    aggroedPlayer.GetComponent<PlayerStats>().TakeDamage((int)damage, new DamageDetails());
                    agent.SetDestination(transform.position);
                    attackTimer = attackCD;
                }

            }
            if (aggrotimer <= 0) //when aggro timer reacheds 0, patient gives up and deaggros
            {
                cheating = false;
                aggroedPlayer = null;
                agent.SetDestination(transform.position);
            }
        }
        else if (!aggroedPlayer)
        {
            cheating = false;
            attackTimer = attackCD;
            aggrotimer = aggroLength;
            if ( wanderUp)
            {
                if (currentWander!= null)
                {
                    StopCoroutine(currentWander);
                }
                currentWander = StartCoroutine(escapedWander());
            }
        }
        
        
    }


    /*private void OnTriggerEnter2D(Collider2D other) {
        PlayerStats stats = other.gameObject.transform.parent.GetComponentInChildren<PlayerStats>();
        if (stats && escaped)
        {
            //if (Vector3.Distance(aggroedPlayer.transform.position, transform.position) <= 2) //if player is close enough
            {
                //attack (this should always land, i dont really want it to be cheesed for a melee attack)
                stats.TakeDamage((int)damage, new DamageDetails());
                agent.SetDestination(transform.position);
                attackTimer = attackCD;
            }
        }
    }*/

}
