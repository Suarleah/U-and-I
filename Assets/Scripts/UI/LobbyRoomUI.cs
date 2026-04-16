using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Lobby room screen, shows players and lets the host start the game
public class LobbyRoomUI : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text statusText;

    [Header("Player list")]
    [SerializeField] private Transform  playerListContent;
    [SerializeField] private GameObject playerListItemPrefab;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;

    // Subscribe to events and show the start button only for the host
    void OnEnable()
    {
        RefreshUI();

        SteamLobbyManager.Instance.OnMemberJoined       += HandleMemberJoined;
        SteamLobbyManager.Instance.OnMemberLeft         += HandleMemberLeft;
        SteamLobbyManager.Instance.OnError              += ShowError;
        SteamLobbyManager.Instance.OnGameStartRequested += HandleGameStartRequested;

        startButton.onClick.AddListener(OnStartClicked);
        leaveButton.onClick.AddListener(OnLeaveClicked);

        bool isOwner = SteamLobbyManager.Instance.HostSteamId == SteamClient.SteamId;
        startButton.gameObject.SetActive(isOwner);
    }

    // Always undo what OnEnable did
    void OnDisable()
    {
        SteamLobbyManager.Instance.OnMemberJoined       -= HandleMemberJoined;
        SteamLobbyManager.Instance.OnMemberLeft         -= HandleMemberLeft;
        SteamLobbyManager.Instance.OnError              -= ShowError;
        SteamLobbyManager.Instance.OnGameStartRequested -= HandleGameStartRequested;

        startButton.onClick.RemoveListener(OnStartClicked);
        leaveButton.onClick.RemoveListener(OnLeaveClicked);
    }

    // Update the player list when someone joins
    void HandleMemberJoined(Friend friend) => RefreshUI();

    // Update the player list when someone leaves
    void HandleMemberLeft(Friend friend) => RefreshUI();

    // Non-host clients receive this and load into the game
    void HandleGameStartRequested()
    {
        GameManager.Instance.JoinGameAsClient();
    }

    // Host clicks Start: launch server and tell clients via lobby chat
    void OnStartClicked()
    {
        bool isOwner = SteamLobbyManager.Instance.HostSteamId == SteamClient.SteamId;
        if (!isOwner) return;

        GameManager.Instance.HostStartGame();
        SteamLobbyManager.Instance.CurrentLobby.SendChatString("START");
    }

    // Player clicks Leave: tear down and go back to main menu
    void OnLeaveClicked()
    {
        GameManager.Instance.GoToMainMenu();
    }

    // Refresh lobby name, player list, and counter
    void RefreshUI()
    {
        if (!SteamLobbyManager.Instance.IsLobbyValid) return;

        lobbyNameText.text = SteamLobbyManager.Instance.GetLobbyName();
        RebuildPlayerList();

        int count = SteamLobbyManager.Instance.CurrentLobby.MemberCount;
        statusText.text = $"{count} / {SteamLobbyManager.MaxPlayers} players";
    }

    // Wipe and rebuild the list of player name labels
    void RebuildPlayerList()
    {
        foreach (Transform child in playerListContent)
            Destroy(child.gameObject);

        foreach (var member in SteamLobbyManager.Instance.GetMembers())
        {
            bool isOwner = member.Id == SteamLobbyManager.Instance.HostSteamId;
            var item  = Instantiate(playerListItemPrefab, playerListContent);
            var label = item.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = isOwner ? $"{member.Name}  [HOST]" : member.Name;
        }
    }

    // Display an error message in the status text
    void ShowError(string err)
    {
        if (statusText != null) statusText.text = $"Error: {err}";
    }
}