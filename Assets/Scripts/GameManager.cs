using Unity.Netcode;
using UnityEngine;

// Controls the high-level game state and which UI panel is visible
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject lobbyRoomPanel;
    [SerializeField] GameObject gamePanel;

    [Header("Game")]
    [SerializeField] GameObject gameWorld;

    // The screen the player is currently on
    public enum State { MainMenu, LobbyRoom, InGame }
    public State CurrentState { get; private set; } = State.MainMenu;

    void Awake()
    {
        // Singleton enforcement
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Tear down any active networking and lobby, then show the main menu
    public void GoToMainMenu()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        if (SteamLobbyManager.Instance != null && SteamLobbyManager.Instance.IsLobbyValid)
            SteamLobbyManager.Instance.LeaveLobby();

        CurrentState = State.MainMenu;
        mainMenuPanel?.SetActive(true);
        lobbyRoomPanel?.SetActive(false);
        gamePanel?.SetActive(false);
        if (gameWorld != null) gameWorld.SetActive(false);
    }

    // Switch to the lobby room view after creating or joining a lobby
    public void GoToLobbyRoom()
    {
        CurrentState = State.LobbyRoom;
        mainMenuPanel?.SetActive(false);
        lobbyRoomPanel?.SetActive(true);
        gamePanel?.SetActive(false);
        if (gameWorld != null) gameWorld.SetActive(false);
    }

    // Host starts the NGO server and enters the game world
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

    // Client connects to the host's server
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

    // Triggered by the host's Start Game button
    public void HostStartGame()
    {
        StartGameAsHost();
    }
}