using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class P2PWrapper : MonoBehaviour
{
    [Header("Settings")]
    public ushort port = 7777;
    public string defaultIP = "127.0.0.1";

    UnityTransport transport;

    void Awake()
    {
        transport = GetComponent<UnityTransport>();
    }

    public void Host()
    {
        transport.SetConnectionData("0.0.0.0", port);
        NetworkManager.Singleton.StartHost();
    }

    public void Join(string ip)
    {
        transport.SetConnectionData(string.IsNullOrEmpty(ip) ? defaultIP : ip, port);
        NetworkManager.Singleton.StartClient();
    }

    public void Leave()
    {
        NetworkManager.Singleton.Shutdown();
    }
}