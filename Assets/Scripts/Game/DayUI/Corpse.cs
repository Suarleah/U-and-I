using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using System.Collections.Generic;
using TMPro;
using TMPro;
using FishNet.Transporting;
using UnityEditor.MemoryProfiler;

public class Corpse : Interactable
{
    public PlayerStats corpseOwner; //the player whose corpse it is
    [SerializeField] TMP_Text nameplate;

    [SerializeField] DistanceJoint2D joint; //what the player uses to drag the corpse around

    public override void Awake()
    {

        base.Awake();
        if (corpseOwner)
        {
            nameplate.text = corpseOwner.playerName.Value + "'s corpse";
        }
        
    }

    public override void Interact()
    {
        if (joint.connectedBody == player.GetComponent<Rigidbody2D>())
        {
            UIManager.Instance.currentInteraction = null;
            detach(player);
            return;
        }
        UIManager.Instance.currentInteraction = this;
        attach(player);
    }

    [ServerRpc(RequireOwnership = false)]
    public void attach(GameObject p) //attaches to the player object
    {
        if (joint.connectedBody)
        {
            CloseClient(joint.connectedBody.GetComponent<NetworkBehaviour>().Owner);
        }
        GiveOwnership(p.GetComponent<NetworkBehaviour>().Owner);
        //connectBody(p);
        attachAllClients(p);

    }

    [TargetRpc]
    public void CloseClient(NetworkConnection conn)
    {
        Close();
    }

    [ObserversRpc]
    public void attachAllClients(GameObject p)
    {
        connectBody(p);
    }
    public void connectBody(GameObject p) // has to be done on client because client authoritative
    {
        joint.enabled = true;
        joint.connectedBody = p.GetComponent<Rigidbody2D>();
        //connectBody(joint, p);
        /*joint.maxDistanceOnly = true;
        joint.distance = 2.5f;
        joint.autoConfigureDistance = false;
        joint.breakForce = 7000f;

        joint.breakAction = JointBreakAction2D.CallbackOnly;*/
    }


    [ServerRpc(RequireOwnership = false)]
    public void detach(GameObject p) //detaches from the player object
    {

        detachAllClients(p);
    }


    [ObserversRpc]
    public void detachAllClients(GameObject p)
    {
        detachBody(p);
    }
    public void detachBody(GameObject p)
    {
        if (joint.connectedBody == p.GetComponent<Rigidbody2D>())
        {
            joint.enabled = false;
            UIManager.Instance.currentInteraction = null;
            joint.connectedBody = null;
        }
    }

    public void OnJointBreak2D()
    {
        Close();
    }

    public override void Close()
    {
        base.Close();
        detach(player);
        /*
        for (int i = 0; i < joints.Count; i++)
        {
            if (joints[i].connectedBody == player.GetComponent<Rigidbody2D>())
            {
                detach(player);
                return;
            }
        }*/
    }
}
