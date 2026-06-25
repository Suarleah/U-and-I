using UnityEngine;
using UnityEngine.AI;

public class FieldOfViewFollower : MonoBehaviour
{
    public GameObject followedGO;

    public NavMeshAgent agent;

    public FieldOfView fov;

    // Update is called once per frame
    void Update()
    {
        
        if (agent)
        {
            if (agent.desiredVelocity.sqrMagnitude > 0)
            {
                float angleRad = Mathf.Atan2(agent.desiredVelocity.x, agent.desiredVelocity.y);
            
                float angleDeg = angleRad * Mathf.Rad2Deg;
                
                angleDeg = (angleDeg + 360f) % 360f; 
                
                fov.fovRotation = angleDeg;
            }
            
        }
        else
        { //follow gameobject
         //   fov.fovRotation = followedGO.transform.rotation.z;
        }
        
    }
}
