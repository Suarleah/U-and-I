using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

// Wraps all Steam lobby logic so other scripts don't talk to Steamworks directly
public class SteamLobbyManager : MonoBehaviour
{
    public static SteamLobbyManager Instance { get; private set; }

    // Tags so we only find lobbies belonging to our game
    public const string GameKey = "game";
    public const string GameValue = "uandi";
    public const string LobbyNameKey = "lobbyName";
    public const int MaxPlayers = 10;

    // Current lobby state
    public Lobby CurrentLobby { get; private set; }
    public SteamId HostSteamId => CurrentLobby.Owner.Id;
    public bool IsLobbyValid => CurrentLobby.Id.IsValid;

    // Events the UI subscribes to
    public event Action<Lobby> OnLobbyCreated;
    public event Action<Lobby> OnLobbyJoined;
    public event Action OnLobbyLeft;
    public event Action<Friend> OnMemberJoined;
    public event Action<Friend> OnMemberLeft;
    public event Action<List<Lobby>> OnLobbyListReceived;
    public event Action<string> OnError;
    public event Action OnGameStartRequested;

    void Awake()
    {
        // Singleton enforcement
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Hook into Steam's lobby and chat events
    void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated      += HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered      += HandleLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave  += HandleMemberLeft;
        SteamMatchmaking.OnChatMessage       += HandleChatMessage;
    }

    // Unhook to avoid duplicate subscriptions
    void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated      -= HandleLobbyCreated;
        SteamMatchmaking.OnLobbyEntered      -= HandleLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= HandleMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave  -= HandleMemberLeft;
        SteamMatchmaking.OnChatMessage       -= HandleChatMessage;
    }

    // Create a new lobby and tag it as ours
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

    // Join an existing lobby by id
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

    // Leave the current lobby if we're in one
    public void LeaveLobby()
    {
        if (!IsLobbyValid) return;
        CurrentLobby.Leave();
        CurrentLobby = default;
        OnLobbyLeft?.Invoke();
        Debug.Log("[Lobby] Left lobby");
    }

    // Fetch all lobbies tagged for our game
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

    // Helpers the UI uses to display info
    public IEnumerable<Friend> GetMembers() => CurrentLobby.Members;
    public string GetLobbyName() => CurrentLobby.GetData(LobbyNameKey);

    // Forward Steam's "lobby created" callback as our own event
    private void HandleLobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
            OnLobbyCreated?.Invoke(lobby);
        else
            OnError?.Invoke($"Lobby creation failed: {result}");
    }

    // Fires when we successfully enter a lobby
    private void HandleLobbyEntered(Lobby lobby)
    {
        OnLobbyJoined?.Invoke(lobby);
    }

    // Fires when another player joins our lobby
    private void HandleMemberJoined(Lobby lobby, Friend friend)
    {
        OnMemberJoined?.Invoke(friend);
    }

    // Fires when another player leaves our lobby
    private void HandleMemberLeft(Lobby lobby, Friend friend)
    {
        OnMemberLeft?.Invoke(friend);
    }

    // Listen for the host's "START" message to know when to load the game
    private void HandleChatMessage(Lobby lobby, Friend friend, string message)
    {
        if (message == "START")
            OnGameStartRequested?.Invoke();
    }
}