using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Main Menu panel.
///
/// Required UI hierarchy (wire up in Inspector):
///   Canvas/MainMenuPanel
///     ├─ TMP_Text       playerNameText
///     ├─ TMP_InputField lobbyNameInput
///     ├─ Button         createLobbyButton
///     ├─ Button         refreshButton
///     ├─ ScrollRect     lobbyScrollRect
///     │    └─ Content   lobbyListContent   (Vertical Layout Group)
///     └─ TMP_Text       statusText
///
/// LobbyListItem prefab: Button with a TMP_Text child.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Player info")]
    [SerializeField] TMP_Text playerNameText;

    [Header("Create")]
    [SerializeField] TMP_InputField lobbyNameInput;
    [SerializeField] Button         createLobbyButton;

    [Header("Lobby list")]
    [SerializeField] Button    refreshButton;
    [SerializeField] Transform lobbyListContent;   // Vertical Layout Group parent
    [SerializeField] GameObject lobbyItemPrefab;   // Prefab: Button + TMP_Text child

    [Header("Feedback")]
    [SerializeField] TMP_Text statusText;

    void OnEnable()
    {
        if (SteamClient.IsValid)
            playerNameText.text = SteamClient.Name;

        SteamLobbyManager.Instance.OnLobbyCreated      += HandleLobbyCreated;
        SteamLobbyManager.Instance.OnLobbyJoined       += HandleLobbyJoined;
        SteamLobbyManager.Instance.OnLobbyListReceived += HandleLobbyList;
        SteamLobbyManager.Instance.OnError             += ShowError;

        createLobbyButton.onClick.AddListener(OnCreateClicked);
        refreshButton.onClick.AddListener(OnRefreshClicked);

        // Auto-refresh on open
        OnRefreshClicked();
    }

    void OnDisable()
    {
        SteamLobbyManager.Instance.OnLobbyCreated      -= HandleLobbyCreated;
        SteamLobbyManager.Instance.OnLobbyJoined       -= HandleLobbyJoined;
        SteamLobbyManager.Instance.OnLobbyListReceived -= HandleLobbyList;
        SteamLobbyManager.Instance.OnError             -= ShowError;

        createLobbyButton.onClick.RemoveListener(OnCreateClicked);
        refreshButton.onClick.RemoveListener(OnRefreshClicked);
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    async void OnCreateClicked()
    {
        string name = lobbyNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) name = $"{SteamClient.Name}'s Game";
        SetStatus("Creating lobby…");
        SetInteractable(false);
        await SteamLobbyManager.Instance.CreateLobby(name);
        SetInteractable(true);
    }

    async void OnRefreshClicked()
    {
        SetStatus("Searching…");
        ClearLobbyList();
        await SteamLobbyManager.Instance.RefreshLobbyList();
    }

    // ── Lobby callbacks ───────────────────────────────────────────────────────

    private void HandleLobbyCreated(Lobby lobby)
    {
        SetStatus($"Lobby '{lobby.GetData(SteamLobbyManager.LobbyNameKey)}' created!");
        GameManager.Instance.GoToLobbyRoom();
    }

    private void HandleLobbyJoined(Lobby lobby)
    {
        SetStatus("Joined lobby!");
        GameManager.Instance.GoToLobbyRoom();
    }

    private void HandleLobbyList(List<Lobby> lobbies)
    {
        ClearLobbyList();

        if (lobbies.Count == 0)
        {
            SetStatus("No lobbies found. Create one!");
            return;
        }

        SetStatus($"{lobbies.Count} lobby(s) found");

        foreach (var lobby in lobbies)
        {
            string lobbyName = lobby.GetData(SteamLobbyManager.LobbyNameKey);
            int    memberCount = lobby.MemberCount;
            ulong  lobbyId    = lobby.Id;

            var item = Instantiate(lobbyItemPrefab, lobbyListContent);
            var label = item.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = $"{lobbyName}  [{memberCount}/{SteamLobbyManager.MaxPlayers}]";

            var btn = item.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnJoinLobbyClicked(lobbyId));
        }
    }

    async void OnJoinLobbyClicked(ulong lobbyId)
    {
        SetStatus("Joining…");
        SetInteractable(false);
        await SteamLobbyManager.Instance.JoinLobby(lobbyId);
        SetInteractable(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void ClearLobbyList()
    {
        foreach (Transform child in lobbyListContent)
            Destroy(child.gameObject);
    }

    void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"[MainMenu] {msg}");
    }

    void ShowError(string err)
    {
        SetStatus($"Error: {err}");
        SetInteractable(true);
    }

    void SetInteractable(bool value)
    {
        createLobbyButton.interactable = value;
        refreshButton.interactable     = value;
    }
}
