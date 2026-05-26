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

public class VoteManager : NetworkBehaviour
{
    [Header("Voting Screen")]
    public GameObject playerName;
    public GameObject cursor; // Should be white
    public Transform playerList;
    private static int colorIndex;
    public Color[] playerColors;

    [Header("Shop Screen")]
    public int playersReady = 0; // Used for starting game
    private bool startingGame; // Used for starting game
    public Image progressBox; // Loading progress bar for when all players are ready
    public TextMeshProUGUI playersReadyText;
    private ReadyManager readyManager;
    private Coroutine countdownCoroutine;
    public GameObject shop;
    [SerializeField]private Canvas shopC; // Lobby and its canvas
    public String sceneToLoad; // Next scene
    private SceneLoadData sld;
    private static int posIndex;
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

    [ServerRpc]
    public void UpdateIndexes()
    {
        colorIndex++;
        posIndex++;
    }

    [ObserversRpc]
    public void PlayerDoneLoading(ClientPresenceChangeEventArgs arrghs)
    {
        GameObject localPlayer = arrghs.Connection.FirstObject.gameObject;

        localPlayer.SetActive(false);
        localPlayer.transform.position = spawnPoints[posIndex].position;
        
        TextMeshProUGUI name = Instantiate(playerName, playerList).GetComponentInChildren<TextMeshProUGUI>();
        // name is the text of the playerName prefab that shows up in the list of players
        name.text = localPlayer.GetComponentInChildren<TextMeshProUGUI>().text;
        // the playerName text shows up as the same text that appears ervoe clients text above their head in game
        
        playerName.GetComponentInChildren<Image>().color = playerColors[colorIndex];
        UpdateIndexes();
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