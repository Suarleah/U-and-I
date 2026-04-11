using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
public class SteamLobbyManager : MonoBehaviour
{
    public static SteamLobbyManager Instance { get; private set; }

    public const string GameKey = "game";
    public const string GameValue = "uandi";
    public const string LobbyNameKey = "lobbyName";
    public const int MaxPlayers = 10;

    public Lobby CurrentLobby { get; private set; }
    public SteamId HostSteamId => CurrentLobby.Owner.Id;
    public bool IsLobbyValid => CurrentLobby.Id.IsValid;
    public event Action<Lobby> OnLobbyCreated;
    public event Action<Lobby> OnLobbyJoined;
    public event Action OnLobbyLeft;
    public event Action<Friend> OnMemberJoined;
    public event Action<Friend> OnMemberLeft;
    public event Action<List<Lobby>> OnLobbyListReceived;
    public event Action<string> OnError;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += HandleLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += HandleMemberLeft;
    }

    void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= HandleLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= HandleMemberLeft;
    }
    public async Task CreateLobby(string lobbyName)
    {
        try
        {
            var result = await SteamMatchmaking.CreateLobbyAsync(MaxPlayers);
            if (!result.HasValue)
            {
                OnError?.Invoke("Failed to create lobby.");
                return;
            }

            CurrentLobby = result.Value;
            CurrentLobby.SetData(GameKey, GameValue);
            CurrentLobby.SetData(LobbyNameKey, lobbyName);
            CurrentLobby.SetJoinable(true);

            Debug.Log($"[Lobby] Created '{lobbyName}' ({CurrentLobby.Id})");
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Error creating lobby: {e.Message}");
        }
    }

    public async Task JoinLobby(ulong lobbyId)
    {
        try
        {
            var result = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
            if (!result.HasValue)
            {
                OnError?.Invoke("Failed to join lobby.");
                return;
            }

            CurrentLobby = result.Value;
            Debug.Log($"[Lobby] Joined {CurrentLobby.Id}");
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Error joining lobby: {e.Message}");
        }
    }

    public void LeaveLobby()
    {
        if (!IsLobbyValid) return;
        CurrentLobby.Leave();
        CurrentLobby = default;
        OnLobbyLeft?.Invoke();
        Debug.Log("[Lobby] Left lobby");
    }

    public async Task RefreshLobbyList()
    {
        try
        {
            var lobbies = await SteamMatchmaking.LobbyList
                .WithKeyValue(GameKey, GameValue)
                .RequestAsync();

            var list = new List<Lobby>();
            if (lobbies != null)
                list.AddRange(lobbies);

            OnLobbyListReceived?.Invoke(list);
        }
        catch (Exception e)
        {
            OnError?.Invoke($"Error fetching lobbies: {e.Message}");
        }
    }

    public IEnumerable<Friend> GetMembers() => CurrentLobby.Members;

    public string GetLobbyName() => CurrentLobby.GetData(LobbyNameKey);

    private void HandleLobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
            OnLobbyCreated?.Invoke(lobby);
        else
            OnError?.Invoke($"Lobby creation failed: {result}");
    }

    private void HandleLobbyEntered(Lobby lobby)
    {
        OnLobbyJoined?.Invoke(lobby);
    }

    private void HandleMemberJoined(Lobby lobby, Friend friend)
    {
        OnMemberJoined?.Invoke(friend);
    }

    private void HandleMemberLeft(Lobby lobby, Friend friend)
    {
        OnMemberLeft?.Invoke(friend);
    }
}
