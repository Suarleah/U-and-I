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
using System.Collections.Generic;
using System.Threading.Tasks;

public class VoteManager : NetworkBehaviour
{
    [Header("Voting Screen")]
    public GameObject playerName;
    public Transform playerList;
    public Color[] playerColors;
    [SerializeField] GameObject cursorPrefab;
    public int playersToLoad = 0;
    public List<NetworkObject> networkObjects = new List<NetworkObject>();
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
    public Transform[] spawnPoints;

    async void Start()
    {

        NetworkManager.SceneManager.OnClientPresenceChangeEnd += PlayerDoneLoading;
        playersToLoad = RelayManager.Instance.currentPlayersCount;

        sld = new SceneLoadData(sceneToLoad);
        sld.ReplaceScenes = ReplaceOption.All;

        shopC = shop.GetComponentInChildren<Canvas>();
    }

    async void Update()
    {
        ReadyToStart();
    }

    [Server]
    public void PlayerDoneLoading(ClientPresenceChangeEventArgs arrghs) // yarrrr!!!!
    {
        GameObject localPlayer = arrghs.Connection.FirstObject.gameObject;
        networkObjects.Add(localPlayer.GetComponent<NetworkObject>());
        playersToLoad--;
        Debug.Log(playersToLoad);

        if (playersToLoad == 0)
        {
            Debug.Log("I'm going to all players");
            AllPlayersDoneLoading();
        }

    }

    [Server]
    async Task AllPlayersDoneLoading()
    {
        for (int i = 0; i < networkObjects.Count; i++)
        {
            // For each player whom is connected
            NetworkObject no = networkObjects[i];
            // The object I am talking about is from my array of objects

            GameObject localPlayer = no.gameObject;
            // The local object is the local connections object

            GameObject name = Instantiate(playerName, playerList);
            Spawn(name, no.Owner);
            // make a name and then make that name spawn on the network
            
            String myName = localPlayer.GetComponentInChildren<TextMeshProUGUI>().text;
            // Create their name and then spawn in on the network, get the players local name above their head

            GameObject cursor = Instantiate(cursorPrefab, voteCanvas.transform);
            Spawn(cursor, no.Owner);
            // create a cursor and then spawn it over the network
            
            cursor.GetComponent<NetworkCursor>().SetColor(playerColors[i]);
            

            UpdateValuesOnClient(localPlayer.GetComponent<NetworkObject>(), cursor.GetComponent<NetworkObject>(),
                name.GetComponent<NetworkObject>(), myName, i);
            // Apperently you cannot pass GameObjects into RPC.....
            // Actually fuck this stupid bullshit

        }
    }

    [ObserversRpc]
    void UpdateValuesOnClient(NetworkObject localPlayer, NetworkObject cursor, NetworkObject nameText, String name, int i)
    {
        localPlayer.gameObject.SetActive(false);
        localPlayer.transform.position = spawnPoints[i].position;
        // Move the player again because IDGAF

        cursor.transform.SetParent(voteCanvas.transform, false);
        // The cursors color fene;jibr;g

        nameText.GetComponentInChildren<TextMeshProUGUI>().color = playerColors[i];
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