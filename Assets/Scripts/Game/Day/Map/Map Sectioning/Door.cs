using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Door : NetworkBehaviour
{
    SyncVar<bool> closed = new SyncVar<bool>();

    [ServerRpc]
    void CloseDoor()
    {
        
    }

    [ServerRpc]
    void OpenDoor()
    {
        
    }
}
