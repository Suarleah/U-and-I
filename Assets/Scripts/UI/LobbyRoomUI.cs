using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LobbyRoomUI : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text statusText;

    [Header("Player list")]
    [SerializeField] private Transform  playerListContent;
    [SerializeField] private GameObject playerListItemPrefab; // Prefab with TMP_Text

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;

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

    void OnStartClicked()
    {
        // Host starts the NGO server and enters the game world.
        // Clients will be told to start via the GameStartRpc on GameManager.
        GameManager.Instance.HostStartGame();

        // Notify clients viachat
        SteamLobbyManager.Instance.CurrentLobby.SendChatString("START");
    }

    void OnLeaveClicked()
    {
        GameManager.Instance.GoToMainMenu();
    }

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
