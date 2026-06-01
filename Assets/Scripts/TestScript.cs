using FishNet.Connection;
using FishNet.Object;
using UnityEngine;


//script I use to test certain network behaviours
public class TestScript : NetworkBehaviour
{

    void Update()
    {
        runCommandsOnClient();
        runCommandsOnServer();
    }

    public void runCommandsOnServer()
    {
        if (!IsServerStarted)
        {
            return;
        }

        ServerRpcTest("Server");
        ObserversRpcTest("Server");
    }

    public void runCommandsOnClient()
    {
        if (!IsClientStarted)
        {
            return;
        }

        ServerRpcTest("Client");
        ObserversRpcTest("Client");
    }






    [ServerRpc(RequireOwnership =false)]
    public void ServerRpcTest(string s){
        Debug.Log("ServerRpc ran from " + s);
    }

    [ObserversRpc]
    public void ObserversRpcTest(string s){
        Debug.Log("ObserversRpc ran from " + s);
    }

    [TargetRpc]
    public void TargetRpcTest(NetworkConnection conn, string s){
        Debug.Log("TargetRpc ran from " + s);
    }

}
