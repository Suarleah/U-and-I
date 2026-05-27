using UnityEngine;

public class PatientZero : Patient
{
    public GameObject aggroedPlayer; //if patientzero is escaped and aggroed, the player will be stored here
    public float aggroRange;
    public float deaggroRange;


    public override void EscapedUpdate()
    {
        /*if (!IsServerStarted)
        {
            return;
        }*/
        patience.Value += Time.deltaTime;
        if (patience.Value >= maxPatience)
        {
            StartCoroutine(Contain());
            return;
        }
        if (aggroedPlayer)
        {
            if (Vector3.Distance(aggroedPlayer.transform.position, transform.position) >= deaggroRange)
            {
                aggroedPlayer = null;
            } else
            {
                agent.SetDestination(aggroedPlayer.transform.position);
            }
        }
        else if (!aggroedPlayer)
        {
            if (Vector3.Distance(FishNet.InstanceFinder.ClientManager.Connection.FirstObject.transform.position, transform.position) <= aggroRange) //right now this just finds the host and chases thjem but soontm i will make it chase nearest
            {
                aggroedPlayer = FishNet.InstanceFinder.ClientManager.Connection.FirstObject.gameObject;
            } else if ( wanderUp)
            {
                escapedWander();
            }
        }
        
    }

}
