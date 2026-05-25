using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using FishNet;
using FishNet.Connection;
using UnityEngine.AI;


public class testentity : NetworkBehaviour
{

    private NavMeshAgent myBrain;
    private PlayerMovement target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        myBrain = GetComponent<NavMeshAgent>();
        myBrain.updateRotation = false; // No rotation
        myBrain.updateUpAxis = false; // Not 3D
        target = InstanceFinder.ClientManager.Connection.FirstObject.gameObject.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServerStarted)
        {
            return;
        }

        myBrain.SetDestination(target.transform.position); // Calculate a new route every frame.... :(
    }

}
