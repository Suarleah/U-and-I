using Steamworks;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Top-level coordinator. Boots Steam, owns the NetworkManager + SteamTransport,
/// and drives the panel state machine (MainMenu → LobbyRoom → Game).
/// Attach to a persistent GameObject in the Lobby scene alongside
/// NetworkManager, SteamTransport, and SteamLobbyManager.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject lobbyRoomPanel;
    [SerializeField] GameObject gamePanel;

    [Header("Game")]
    [SerializeField] GameObject gameWorld;   // Parent of map, player spawn points, etc.

    public enum State { MainMenu, LobbyRoom, InGame }
    public State CurrentState { get; private set; } = State.MainMenu;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitSteam();
    }

    void OnDestroy()
    {
        if (SteamClient.IsValid)
            SteamClient.Shutdown();
    }

    // ── Steam init ────────────────────────────────────────────────────────────

    void InitSteam()
    {
        try
        {
            SteamClient.Init(480, true); // Replace 480 with your real App ID before shipping
            Debug.Log($"[Steam] Logged in as {SteamClient.Name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Steam] Init failed: {e.Message}");
        }
    }

    // ── State machine ─────────────────────────────────────────────────────────

    public void GoToMainMenu()
    {
        NetworkManager.Singleton.Shutdown();
        SteamLobbyManager.Instance.LeaveLobby();

        CurrentState = State.MainMenu;
        mainMenuPanel?.SetActive(true);
        lobbyRoomPanel?.SetActive(false);
        gamePanel?.SetActive(false);
        if (gameWorld != null) gameWorld.SetActive(false);
    }

    public void GoToLobbyRoom()
    {
        CurrentState = State.LobbyRoom;
        mainMenuPanel?.SetActive(false);
        lobbyRoomPanel?.SetActive(true);
        gamePanel?.SetActive(false);
        if (gameWorld != null) gameWorld.SetActive(false);
    }

    public void StartGameAsHost()
    {
        if (!SteamLobbyManager.Instance.IsLobbyValid)
        {
            Debug.LogError("[GameManager] No active lobby to start game from.");
            return;
        }

        CurrentState = State.InGame;
        mainMenuPanel?.SetActive(false);
        lobbyRoomPanel?.SetActive(false);
        gamePanel?.SetActive(true);
        if (gameWorld != null) gameWorld.SetActive(true);

        NetworkManager.Singleton.StartHost();
        Debug.Log("[GameManager] Game started as host");
    }

    public void JoinGameAsClient()
    {
        CurrentState = State.InGame;
        mainMenuPanel?.SetActive(false);
        lobbyRoomPanel?.SetActive(false);
        gamePanel?.SetActive(true);
        if (gameWorld != null) gameWorld.SetActive(true);

        NetworkManager.Singleton.StartClient();
        Debug.Log("[GameManager] Joining game as client");
    }

    // Called by host's Start Game button
    public void HostStartGame()
    {
        StartGameAsHost();

        // Tell all clients to start
        // Clients receive OnClientConnectedCallback and will be in-game already
        // via the NGO connection flow — no extra RPC needed for scene-less setup
    }

    void Update()
    {
        // Facepunch.Steamworks requires periodic callback dispatch
        if (SteamClient.IsValid)
            SteamClient.RunCallbacks();
    }
}
