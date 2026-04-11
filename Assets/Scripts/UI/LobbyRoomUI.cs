using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Lobby Room panel shown after creating or joining a lobby.
///
/// Required UI hierarchy (wire up in Inspector):
///   Canvas/LobbyRoomPanel
///     ├─ TMP_Text  lobbyNameText
///     ├─ Transform playerListContent   (Vertical Layout Group parent)
///     ├─ Button    startButton         (host only — disable for clients)
///     ├─ Button    leaveButton
///     └─ TMP_Text  statusText
///
/// PlayerListItem prefab: a TMP_Text (optionally a small colored icon too).
/// </summary>
public class LobbyRoomUI : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] TMP_Text lobbyNameText;
    [SerializeField] TMP_Text statusText;

    [Header("Player list")]
    [SerializeField] Transform  playerListContent;
    [SerializeField] GameObject playerListItemPrefab; // Prefab with TMP_Text

    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button leaveButton;

    void OnEnable()
    {
        RefreshUI();

        SteamLobbyManager.Instance.OnMemberJoined += _ => RefreshUI();
        SteamLobbyManager.Instance.OnMemberLeft   += _ => RefreshUI();
        SteamLobbyManager.Instance.OnError        += ShowError;

        startButton.onClick.AddListener(OnStartClicked);
        leaveButton.onClick.AddListener(OnLeaveClicked);

        // Only the lobby owner can start
        bool isOwner = SteamLobbyManager.Instance.HostSteamId == SteamClient.SteamId;
        startButton.gameObject.SetActive(isOwner);
    }

    void OnDisable()
    {
        SteamLobbyManager.Instance.OnMemberJoined -= _ => RefreshUI();
        SteamLobbyManager.Instance.OnMemberLeft   -= _ => RefreshUI();
        SteamLobbyManager.Instance.OnError        -= ShowError;

        startButton.onClick.RemoveListener(OnStartClicked);
        leaveButton.onClick.RemoveListener(OnLeaveClicked);
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    void OnStartClicked()
    {
        // Host starts the NGO server and enters the game world.
        // Clients will be told to start via the GameStartRpc on the GameManager.
        GameManager.Instance.HostStartGame();

        // Notify clients via lobby chat message (simplest cross-platform signal)
        SteamLobbyManager.Instance.CurrentLobby.SendChatString("START");
    }

    void OnLeaveClicked()
    {
        GameManager.Instance.GoToMainMenu();
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    void RefreshUI()
    {
        if (!SteamLobbyManager.Instance.IsLobbyValid) return;

        lobbyNameText.text = SteamLobbyManager.Instance.GetLobbyName();
        RebuildPlayerList();

        int count = SteamLobbyManager.Instance.CurrentLobby.MemberCount;
        statusText.text = $"{count} / {SteamLobbyManager.MaxPlayers} players";
    }

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

    void ShowError(string err)
    {
        if (statusText != null) statusText.text = $"Error: {err}";
    }
}
