using System;
using System.Collections;
using FishNet.Managing.Scened;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : NetworkBehaviour
{
    public static ShopManager Instance;
    public Canvas shopCanvas;
    public Canvas voteCanvas;
    public int playersReady = 0; // Used for starting game
    private bool startingGame; // Used for starting game
    public Image progressBox; // Loading progress bar for when all players are ready
    public TextMeshProUGUI playersReadyText;
    private ReadyManager readyManager;
    private Coroutine countdownCoroutine;
    public String sceneToLoad; // Next scene
    private SceneLoadData sld;

    async void Start()
    {
        Instance = this;

        sld = new SceneLoadData(sceneToLoad);
        sld.ReplaceScenes = ReplaceOption.All;
    }

    async void Update()
    {
        ReadyToStart();
    }


    [ObserversRpc]
    public void BeginShopping()
    {
        voteCanvas.enabled = false;
        shopCanvas.enabled = true;
    }

    public void StartGame()
    {
        ReadyManager.Instance.StartGame(sld);
    }

    public void ReadyToStart()
    {
        if (playersReady != RelayManager.Instance.currentPlayersCount && playersReady != 0)
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

        if (playersReady == RelayManager.Instance.currentPlayersCount && !startingGame && playersReady != 0)
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
}
