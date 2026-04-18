using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using FishNet;
using FishNet.Transporting.UTP;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;
    // There should only be one of these
    public int maxPlayers = 4;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task InitializeAsync()
    {
        await UnityServices.InitializeAsync();
        // Try to sign in

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            // if we can't sign in then we go anonyumus

        Debug.Log("My player ID is: super cool, it is: " + AuthenticationService.Instance.PlayerId);
    }

    public async Task<string> CreateRelayAsync(int maxPlayers)
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        // Make a room through unity
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        // join code from the room

        UnityTransport transport = InstanceFinder.NetworkManager.TransportManager.GetTransport<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));
        // connection stuff

        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
        // Find the static server and connection managers and start AS THE HOST

        Debug.Log("Join code: "+ joinCode);

        return joinCode;
    }

    public async Task JoinRelayAsync(string joinCode)
    {
        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

        UnityTransport transport = InstanceFinder.NetworkManager.TransportManager.GetTransport<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

        InstanceFinder.ClientManager.StartConnection();
    }
}
