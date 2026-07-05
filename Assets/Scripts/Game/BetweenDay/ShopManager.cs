using System;
using System.Collections;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : NetworkBehaviour
{
    public static ShopManager Instance;
    public GameObject shopScreen;
    public GameObject voteScreen;
    readonly SyncVar<int> playersReady = new SyncVar<int>(0); // Used for starting game
    private bool startingGame; // Used for starting game
    public Image progressBox; // Loading progress bar for when all players are ready
    public TextMeshProUGUI playersReadyText;
    private ReadyManager readyManager;
    private VoteManager voteManager;
    private Coroutine countdownCoroutine;
    public String sceneToLoad; // Next scene
    private SceneLoadData sld;

    async void Start()
    {
        Instance = this;

        sld = new SceneLoadData(sceneToLoad);
        sld.ReplaceScenes = ReplaceOption.All;

        voteManager = VoteManager.Instance;
    }

    async void Update()
    {
        ReadyToStart();
    }


    [ObserversRpc]
    public void BeginShopping()
    {
        playersReadyText.text = (playersReady.Value + " / " + RelayManager.Instance.currentPlayersCount);
        voteScreen.SetActive(false);
        shopScreen.SetActive(true);
    }

    public void StartGame()
    {
        ReadyManager.Instance.StartGame(sld);
    }

    public void ReadyToStart()
    {
        if (playersReady.Value != RelayManager.Instance.currentPlayersCount && playersReady.Value != 0)
        {
            // If the number of players who are ready is not the same as the number of players in the game
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }
            UpdateReadyBox(0);
            startingGame = false;
        }

        if (playersReady.Value == RelayManager.Instance.currentPlayersCount && !startingGame && playersReady.Value != 0)
        {
            // If the number of players who are ready is the same as the number of players in the game
            countdownCoroutine = StartCoroutine(startCountdown(progressBox));
        }
    }
    public IEnumerator startCountdown(Image i)
    {
        startingGame = true;
        for (float f = 0; f < 1; f += 0.05f)
        {
            UpdateReadyBox(f);
            yield return new WaitForSeconds(0.05f);

        }
        // If we made it through the whole timer, resume the game!!
        StartGame();
    }

    [ObserversRpc]
    private void UpdateReadyBox(float f)
    {
        progressBox.fillAmount = f;
    }

    [ObserversRpc]
    public void PlayerLeftReadyZone()
    {
        playersReady.Value--;
        playersReadyText.text = playersReady.Value + " / " + (RelayManager.Instance.currentPlayersCount);
    }
    [ObserversRpc]
    public void PlayerEnterReadyZone()
    {
        playersReady.Value++;
        playersReadyText.text = playersReady.Value + " / " + (RelayManager.Instance.currentPlayersCount);
    }


}
