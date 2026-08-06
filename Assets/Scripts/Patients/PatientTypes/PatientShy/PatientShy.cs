using System.Collections.Generic;
using System.Collections;
using FishNet.Demo.AdditiveScenes;
using UnityEngine;
using UnityEngine.AI;

public class PatientShy : Patient
{
    
    public float shyRange;
    [Range(-1f, 1f)] public float viewThreshold; 
    public float maxViewDist; 
    public LayerMask viewObstructingLayer;
    List<GameObject> lookingPlayers = new List<GameObject>(); //list of all players current looking at patientshy (patientshy can only aggro onto players that look at him)


    public override void Update()
    {
        
        base.Update();
        lookingPlayers.Clear();
        //code that determines whether player has this patient in view
        foreach (PlayerMovement p in GameManager.Instance.GetPlayers())
        {
            //first check if player is facing this patient
            Vector2 directionToEntity = ((Vector2)transform.position - (Vector2)(p.transform.position)).normalized;
            Vector2 playerFacingDirection = p.transform.right;
            float dotProduct = Vector2.Dot(playerFacingDirection, directionToEntity);
            float dist = Vector2.Distance(p.transform.position, transform.position);

            if (dotProduct * p.visual.transform.localScale.x >= viewThreshold && dist <= maxViewDist) {
                // Next check if there is anything obstructing the view using Physics2D
                Vector2 directionToTarget = (Vector2)transform.position - (Vector2)p.transform.position;
                float distanceToTarget = directionToTarget.magnitude;

                RaycastHit2D hit = Physics2D.Raycast(p.transform.position, directionToTarget.normalized, distanceToTarget, viewObstructingLayer);

                if (hit.collider == null) {
                    lookingPlayers.Add(p.gameObject); // Add player to the list
                }
            }
            
        }
        if (patience.Value > maxPatience)
        {
            patience.Value = maxPatience;
        }
    }
    public override void ContainedUpdate()
    {
        foreach(PlayerMovement p in GameManager.Instance.GetPlayers())
        {
            float dist = Vector3.Distance(p.transform.position, transform.position);
            if (dist <= shyRange)
            {
                patience.Value -= Time.deltaTime * patienceDecay; //lose tons of patience for each nearby player
            }
        }
        foreach(Patient p in PatientManager.Instance.GetAllSpawnedPatients())
        {
            if (p == this) //obviously shouldnt count themself
            {
                continue;
            }
            float dist = Vector3.Distance(p.transform.position, transform.position);
            if (dist <= shyRange)
            {
                patience.Value -= Time.deltaTime * patienceDecay; //lose tons of patience for each nearby entity as well
            }
        }


        aggroedPlayer = null;
        if (patience.Value <= 0)
        {
            StartCoroutine(Escape());
        }
        else if (wanderUp)
        {
            if (currentWander!= null)
            {
                StopCoroutine(currentWander);
            }
            currentWander = StartCoroutine(roomWander());
        }
    }

    public override void FollowingUpdate()
    {
        foreach(PlayerMovement p in GameManager.Instance.GetPlayers())
        {
            float dist = Vector3.Distance(p.transform.position, transform.position);
            if (dist <= shyRange)
            {
                patience.Value -= Time.deltaTime * patienceDecay/10; //lose tons of patience for each nearby player
            }
        }
        foreach(Patient p in PatientManager.Instance.GetAllSpawnedPatients())
        {
            if (p == this) //obviously shouldnt count themself
            {
                continue;
            }
            float dist = Vector3.Distance(p.transform.position, transform.position);
            if (dist <= shyRange)
            {
                patience.Value -= Time.deltaTime * patienceDecay/10; //lose tons of patience for each nearby entity as well
            }
        }

        if (Vector3.Distance(followingPlayer.transform.position, transform.position) > 5)
        {
            agent.SetDestination(followingPlayer.transform.position);
        } else
        {
            agent.ResetPath();
        }
    }

    public override void EscapedUpdate()
    {
        /*if (!IsServerStarted)
        {
            return;
        }*/
        patience.Value += Time.deltaTime * maxPatience/escapeTime;
        if (patience.Value >= maxPatience)
        {
            StartCoroutine(Contain());
            return;
        }

        if (attackTimer > 0) //if on cd, cant move (so they dont stick to the player and one shot all the time)
        {
            attackTimer -=Time.deltaTime;
            return;
        }
        //find a player thats been looking
        if (lookingPlayers.Count > 0)
        {
            aggroedPlayer = lookingPlayers[0];
        }

        if (aggroedPlayer)
        {
            if (aggroedPlayer.GetComponent<PlayerStats>().isDead.Value) //if the aggroed player is dead, and theres no other target, reset aggro
            {
                aggroedPlayer = null;
                agent.SetDestination(transform.position);
            }
                
            // Chase, shy guy doesnt deaggro normally, and has unlimited vision
            agent.SetDestination(aggroedPlayer.transform.position);
            if (Vector3.Distance(aggroedPlayer.transform.position, transform.position) < 1f) //while in range, attack
            {
                aggroedPlayer.GetComponent<PlayerStats>().TakeDamage((int)damage, new DamageDetails());
                agent.SetDestination(transform.position);
                attackTimer = attackCD;
            }

        }
        else if (!aggroedPlayer)
        {
            attackTimer = attackCD;
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

    public override IEnumerator Escape()//patient escapes, overriden so because other patients by default aggro to a player, this one only aggroes if seen
    {
        
        if (!IsServerStarted)
        {
           yield break;
        }
        followingPlayer = null;
        escaped = true;
        patience.Value = 0;
        wanderUp = true; 
        EscapeAllClients();
        
    }
}
