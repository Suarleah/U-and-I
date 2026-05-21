using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using FishNet;
using FishNet.Connection;


public class testentity : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServerStarted)
        {
            return;
        }

        float x = Random.Range(0, 10) - 4.5f;
        gameObject.transform.position += new Vector3(x/100, x/100, 0);
    }
}
