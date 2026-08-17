using UnityEngine;
using TMPro;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;

public class UIFeedbackText : NetworkBehaviour
{
    public readonly SyncVar<float> alpha = new SyncVar<float>();

    void Awake()
    {
        alpha.OnChange+= OnChange;
    }

    //when you interact with a Patient, they might give feedback on your interaction, this is the visual element of that
    [Server]
    public void Begin()
    {
        gameObject.GetComponentInChildren<TextMeshProUGUI>().alpha = 1; //just temp, its to set opacity to 0
    }



    // Update is called once per frame
    void Update()
    {
        if (IsServerStarted)
        {
            ServerUpdate();
        }
        
    }

    [Server]
    void ServerUpdate(){
        transform.position += Time.deltaTime * (new Vector3(0, 1, 0));
        if (gameObject.GetComponentInChildren<TextMeshProUGUI>().alpha <= 0)
        {
            Despawn(base.NetworkObject);
            return;
        }
        gameObject.GetComponentInChildren<TextMeshProUGUI>().alpha -= Time.deltaTime;
    }

    public void OnChange(float prev, float next, bool asServer)
    {
        gameObject.GetComponentInChildren<TextMeshProUGUI>().alpha = next;
    }
}
