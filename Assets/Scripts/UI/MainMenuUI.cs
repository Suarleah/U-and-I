using System.Collections.Generic;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Main menu screen, lets the player create or join a lobby
public class MainMenuUI : MonoBehaviour
{
    [Header("Player info")]
    [SerializeField] TMP_Text playerNameText;

    [Header("Create")]
    [SerializeField] TMP_InputField lobbyNameInput;
    [SerializeField] Button         createLobbyButton;

    [Header("Lobby list")]
    [SerializeField] Button     refreshButton;
    [SerializeField] Transform  lobbyListContent;
    [SerializeField] GameObject lobbyItemPrefab;

    [Header("Feedback")]
    [SerializeField] TMP_Text statusText;

    // Wire up Steam events and button clicks
    void OnEnable()
    {
        if (SteamClient.IsValid)
            playerNameText.text = SteamClient.Name;

        if (SteamLobbyManager.Instance == null)
        {
            SetStatus("Steam lobby system not ready.");
            return;
        }

        SteamLobbyManager.Instance.OnLobbyCreated      += HandleLobbyCreated;
        SteamLobbyManager.Instance.OnLobbyJoined       += HandleLobbyJoined;
        SteamLobbyManager.Instance.OnLobbyListReceived += HandleLobbyList;
        SteamLobbyManager.Instance.OnError             += ShowError;

        createLobbyButton.onClick.AddListener(OnCreateClicked);
        refreshButton.onClick.AddListener(OnRefreshClicked);

        OnRefreshClicked();
    }

    // Always undo what OnEnable hooked up
    void OnDisable()
    {
        if (SteamLobbyManager.Instance != null)
        {
            SteamLobbyManager.Instance.OnLobbyCreated      -= HandleLobbyCreated;
            SteamLobbyManager.Instance.OnLobbyJoined       -= HandleLobbyJoined;
            SteamLobbyManager.Instance.OnLobbyListReceived -= HandleLobbyList;
            SteamLobbyManager.Instance.OnError             -= ShowError;
        }

        createLobbyButton.onClick.RemoveListener(OnCreateClicked);
        refreshButton.onClick.RemoveListener(OnRefreshClicked);
    }

    // Make a new lobby with the typed name
    async void OnCreateClicked()
    {
        string name = lobbyNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) name = $"{SteamClient.Name}'s Game";
        SetStatus("Creating lobby...");
        SetInteractable(false);
        await SteamLobbyManager.Instance.CreateLobby(name);
        SetInteractable(true);
    }

    // Re-fetch the visible lobby list
    async void OnRefreshClicked()
    {
        SetStatus("Searching...");
        ClearLobbyList();
        await SteamLobbyManager.Instance.RefreshLobbyList();
    }

    // Move to the lobby room when our lobby gets created
    private void HandleLobbyCreated(Lobby lobby)
    {
        SetStatus($"Lobby '{lobby.GetData(SteamLobbyManager.LobbyNameKey)}' created!");
        GameManager.Instance.GoToLobbyRoom();
    }

    // Move to the lobby room after joining someone else's lobby
    private void HandleLobbyJoined(Lobby lobby)
    {
        SetStatus("Joined lobby!");
        GameManager.Instance.GoToLobbyRoom();
    }

    // Display the fetched lobbies as buttons
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
            string lobbyName   = lobby.GetData(SteamLobbyManager.LobbyNameKey);
            int    memberCount = lobby.MemberCount;
            ulong  lobbyId     = lobby.Id;

            var item  = Instantiate(lobbyItemPrefab, lobbyListContent);
            var label = item.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = $"{lobbyName}  [{memberCount}/{SteamLobbyManager.MaxPlayers}]";

            var btn = item.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnJoinLobbyClicked(lobbyId));
        }
    }

    // Try to join the lobby the player clicked
    async void OnJoinLobbyClicked(ulong lobbyId)
    {
        SetStatus("Joining...");
        SetInteractable(false);
        await SteamLobbyManager.Instance.JoinLobby(lobbyId);
        SetInteractable(true);
    }

    // Remove all lobby buttons from the list
    void ClearLobbyList()
    {
        foreach (Transform child in lobbyListContent)
            Destroy(child.gameObject);
    }

    // Update the status message at the bottom
    void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"[MainMenu] {msg}");
    }

    // Show error and re-enable buttons
    void ShowError(string err)
    {
        SetStatus($"Error: {err}");
        SetInteractable(true);
    }

    // Enable or disable the create/refresh buttons during async work
    void SetInteractable(bool value)
    {
        createLobbyButton.interactable = value;
        refreshButton.interactable     = value;
    }
}