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
    public int currentPlayersCount = 0;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InstanceFinder.NetworkManager.ClientManager.OnClientTimeOut += PlayerLeft;
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

        Debug.Log("Join code: " + joinCode);

        return joinCode;
    }

    public async Task<bool> JoinRelayAsync(string joinCode)
    {
        if (string.IsNullOrWhiteSpace(joinCode))
        {
            return false;
            // If they didn't type anything in then they didn't join
        }
            

        JoinAllocation allocation;

        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            // try to do this
        }
        catch (RelayServiceException e)
        {
            // If they fail to join then they didn't join
            return false;

        }

        UnityTransport transport = InstanceFinder.NetworkManager.TransportManager.GetTransport<UnityTransport>();
        transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

        InstanceFinder.ClientManager.StartConnection();
        return true;
        // If they joined then they joined
    }

    public void PlayerLeft()
    {
        currentPlayersCount--;
    }
}
