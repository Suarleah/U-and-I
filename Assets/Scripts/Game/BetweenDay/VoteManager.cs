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
using FishNet.Object.Synchronizing;

public class VoteManager : NetworkBehaviour
{
    [Header("Voting Screen")]
    public GameObject playerName;
    public Transform playerList;
    public readonly SyncVar<int> colorIndex= new SyncVar<int>(0);
    public Color[] playerColors;
    [SerializeField] GameObject cursorPrefab;
    public Canvas voteCanvas;

    [Header("Shop Screen")]
    public int playersReady = 0; // Used for starting game
    private bool startingGame; // Used for starting game
    public Image progressBox; // Loading progress bar for when all players are ready
    public TextMeshProUGUI playersReadyText;
    private ReadyManager readyManager;
    private Coroutine countdownCoroutine;
    public GameObject shop; private Canvas shopC; // shop and its canvas
    public String sceneToLoad; // Next scene
    private SceneLoadData sld;
    public readonly SyncVar<int> posIndex= new SyncVar<int>(0);
    public Transform[] spawnPoints;

    async void Start()
    {
        NetworkManager.SceneManager.OnClientPresenceChangeEnd += PlayerDoneLoading;


        sld = new SceneLoadData(sceneToLoad);
        sld.ReplaceScenes = ReplaceOption.All;

        shopC = shop.GetComponentInChildren<Canvas>();
    }

    async void Update()
    {
        ReadyToStart();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateIndexes()
    {
        colorIndex.Value++;
        posIndex.Value++;
    }

    [Server]
    public void PlayerDoneLoading(ClientPresenceChangeEventArgs arrghs)
    {
        GameObject localPlayer = arrghs.Connection.FirstObject.gameObject;
        
        localPlayer.SetActive(false);
        localPlayer.transform.position = spawnPoints[posIndex.Value].position;
        // Get local player, disable, move to desired location for shopping

        GameObject name = Instantiate(playerName, playerList);
        Spawn(name, arrghs.Connection);
        name.transform.SetParent(playerList, false);
        String myName = localPlayer.GetComponentInChildren<TextMeshProUGUI>().text;
        // Create their name and then spawn in on the network, get the players local name above their head

        GameObject cursor = Instantiate(cursorPrefab, voteCanvas.transform);
        Spawn(cursor, arrghs.Connection);
        cursor.transform.SetParent(voteCanvas.transform, false);
        // create a cursor and then spawn it over the network

        UpdateForClients(localPlayer.GetComponent<NetworkObject>(), cursor.GetComponent<NetworkObject>(),
            name.GetComponent<NetworkObject>(), myName);
            // Apperently you cannot pass GameObjects into RPC.....
            // Actually fuck this stupid bullshit

        UpdateIndexes(); // Only gets called on the server dont call it or ill fucking kill you
    }

    [ObserversRpc]
    void UpdateForClients(NetworkObject localPlayer, NetworkObject cursor, NetworkObject nameText, String name)
    {
        localPlayer.gameObject.SetActive(false);
        localPlayer.transform.position = spawnPoints[posIndex.Value].position;
        // Move the player again because IDGAF

        cursor.GetComponentInChildren<Image>().color = playerColors[colorIndex.Value];
        cursor.transform.SetParent(voteCanvas.transform, false);
        // The cursors color fene;jibr;g

        nameText.GetComponentInChildren<TextMeshProUGUI>().color = playerColors[colorIndex.Value];
        nameText.GetComponentInChildren<TextMeshProUGUI>().text = name;
        nameText.transform.SetParent(playerList, false);

        
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
        // If we made it through the whole timer, resume the game!!
        StartGame();
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
}