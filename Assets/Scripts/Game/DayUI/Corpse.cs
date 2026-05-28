using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;
using TMPro;
using TMPro;

public class Corpse : Interactable
{
    public PlayerStats corpseOwner; //the player whose corpse it is
    [SerializeField] TMP_Text nameplate;

    List<Joint2D> joints; //what the player uses to drag the corpse around

    public override void Awake()
    {

        base.Awake();
        if (corpseOwner)
        {
            nameplate.text = corpseOwner.playerName.Value + "'s corpse";
        }
        
        joints = new List<Joint2D>();
    }

    public override void Interact()
    {
        for (int i = 0; i < joints.Count; i++)
        {
            if (joints[i].connectedBody == player.GetComponent<Rigidbody2D>())
            {
                detach(player);
                return;
            }
        }

        attach(player);
    }

    [ObserversRpc] 
    public void attach(GameObject p) //attaches to the player object
    {
        DistanceJoint2D joint = gameObject.AddComponent<DistanceJoint2D>();
        joint.connectedBody = p.gameObject.GetComponent<Rigidbody2D>();
        joint.maxDistanceOnly = true;
        joint.distance = 2.5f;
        joint.autoConfigureDistance = false;
        joint.breakForce = 7000f;

        joint.breakAction = JointBreakAction2D.CallbackOnly;

        /*joint.dampingRatio = 0.7f;
        joint.frequency = 0.5f;
        joint.autoConfigureDistance = false;
        joint.distance = 2.5f;
        
        joint.breakForce
        joint.breakAction = JointBreakAction2D.CallbackOnly;*/

        joints.Add(joint);
        UIManager.Instance.currentInteraction = this;
    }

    [ObserversRpc]
    public void detach(GameObject p) //detaches from the player object
    {

        for (int i = 0; i < joints.Count; i++)
        {
            if (joints[i].connectedBody == p.GetComponent<Rigidbody2D>())
            {
                UIManager.Instance.currentInteraction = null;
                GameObject.Destroy(joints[i]);
                joints.Remove(joints[i]);
                break;
            }
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
