using FishNet.Demo.AdditiveScenes;
using UnityEngine;
using UnityEngine.AI;

public class PatientZero : Patient
{
    


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
            
                if (!aggroedPlayer.GetComponent<PlayerStats>().isDead.Value)
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
                        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
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
                if (Vector3.Distance(aggroedPlayer.transform.position, transform.position) < 1f)
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
                StartCoroutine(escapedWander());
            }
        }
        
        
    }


    /*private void OnCollisionEnter2D(Collision2D other) {
        PlayerStats stats = other.gameObject.GetComponent<PlayerStats>();
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
