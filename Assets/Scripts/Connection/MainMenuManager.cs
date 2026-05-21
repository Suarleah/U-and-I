using UnityEngine;
using TMPro;
using System;
using FishNet.Managing.Scened;
using FishNet;
using UnityEngine.SceneManagement;
using FishNet.Object;
using Unity.Services.Core;
using FishNet.Connection;
using System.Collections;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Screen")]
    public TMP_Text joinCodeDisplay; // Code for others to use
    public TMP_InputField joinCodeInput; // Where you enter the join code
    public GameObject menu; // Holds Join and Host buttons
    public Button hostButton;
    public Button joinButton;

    [Header("Lobby Screen")]
    public TMP_InputField changeName; // Where player changes name

    public int playersReady = 0; // Used for starting game
    private bool startingGame; // Used for starting game
    public Image progressBox; // Loading progress bar for when all players are ready
    public TextMeshProUGUI playersReadyText;
    private ReadyManager readyManager;
    private Coroutine countdownCoroutine;
    public GameObject lobby; private Canvas lobbyC; // Lobby and its canvas

    public GameObject loading; // loading screen
    public String sceneToLoad; // Next scene
    private SceneLoadData sld;



    async void Start()
    {
        hostButton.interactable = false;
        joinButton.interactable = false;

        await RelayManager.Instance.InitializeAsync();
        // You can't click the buttons until they will work
        hostButton.interactable = true;
        joinButton.interactable = true;

        InstanceFinder.NetworkManager.SceneManager.OnLoadStart += OnLoadStart;
        InstanceFinder.NetworkManager.SceneManager.OnLoadEnd += OnLoadEnd;
        InstanceFinder.NetworkManager.SceneManager.OnClientLoadedStartScenes += ClientJoined;
        // When a scene starts loading and ends loading, not actually switching the scene just loading it asynchronously
        
        sld = new SceneLoadData(sceneToLoad);
        sld.ReplaceScenes = ReplaceOption.All;

        lobbyC = lobby.GetComponentInChildren<Canvas>();
    }

    async void Update()
    {
        ReadyToStart();
    }

    void OnLoadStart(SceneLoadStartEventArgs loadEventArgs)
    {
        loading.SetActive(true);
    }

    void OnLoadEnd(SceneLoadEndEventArgs loadEventArgs)
    {
        // sld.ReplaceScenes = ReplaceOption.All;
        // InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(sld);
    }

    void ClientJoined(NetworkConnection connection, bool asHost)
    {
        if (ReadyManager.Instance != null)
        {
            ReadyManager.Instance.UpdatePlayerReadyText(playersReady, InstanceFinder.NetworkManager.ClientManager.Clients.Count);
        }

    }

    public void StartGame()
    {
        ReadyManager.Instance.StartGame(sld);
    }

    public void ReadyToStart()
    {
        if (playersReady != InstanceFinder.NetworkManager.ClientManager.Clients.Count && playersReady != 0)
        {
            // If the number of players who are ready is not the same as the number of players in the game
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
            progressBox.fillAmount = 0;
            startingGame = false;
        }

        if (playersReady == InstanceFinder.NetworkManager.ClientManager.Clients.Count && !startingGame && playersReady != 0)
        {
            // If the number of players who are ready is the same as the number of players in the game
            countdownCoroutine = StartCoroutine(startCountdown());
        }
    }

    private IEnumerator startCountdown()
    {
        startingGame = true;
        for (float i = 0; i < 1; i += 0.05f)
        {
            if (ReadyManager.Instance != null)
            {
                ReadyManager.Instance.UpdateBoxClients(i); // Have to do this through a client method so that everyone sees it update
                yield return new WaitForSeconds(0.05f);
            }

        }
        // If we made it through the whole timer, start the game!!
        StartGame();
    }

    public async void OnHostClicked()
    {
        loading.SetActive(true);
        string code = await RelayManager.Instance.CreateRelayAsync(RelayManager.Instance.maxPlayers);
        // get code from ro4;ieiirmnpv;ivb
        joinCodeDisplay.text = "Secret code: " + code;
        menu.SetActive(false);
        lobby.SetActive(true);
        loading.SetActive(false);
    }

    public async void OnJoinClicked()
    {
        loading.SetActive(true);
        await RelayManager.Instance.JoinRelayAsync(joinCodeInput.text.Trim().ToUpper());
        menu.SetActive(false);
        lobby.SetActive(true);
        // GET CODE FROM THE RELAY MANAGER
        loading.SetActive(false);
    }

    PlayerMovement FindLocalPlayer()
    {
        return FishNet.InstanceFinder.ClientManager.Connection.FirstObject.gameObject.GetComponent<PlayerMovement>();
        /*
        foreach (PlayerMovement p in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            if (p.IsOwner) return p;
        }
        return null;*/
    }

    public void ChangeName(String name)
    {
        PlayerMovement localPlayer = FindLocalPlayer();
        localPlayer.SetNameServerRpc(name);
    }

    public void WhileTyping(String nothing)
    {
        PlayerMovement localPlayer = FindLocalPlayer();
        localPlayer.DisableMyInput();
    }

    public void DoneTyping(String nothing)
    {
        PlayerMovement localPlayer = FindLocalPlayer();
        localPlayer.EnableMyInput();
    }
    void OnDisable() // Yarrr I don't be listening to events in other scenes
    {
        InstanceFinder.NetworkManager.SceneManager.OnLoadStart -= OnLoadStart;
        InstanceFinder.NetworkManager.SceneManager.OnLoadEnd -= OnLoadEnd;
        InstanceFinder.NetworkManager.SceneManager.OnClientLoadedStartScenes -= ClientJoined;
    }
}