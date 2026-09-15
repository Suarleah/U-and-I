using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ShopEntry
{
    public ItemSO item;
    public int cost;
}

public class ShopManager : NetworkBehaviour
{
    public static ShopManager Instance;
    public GameObject shopScreen;
    public GameObject shopCanvas;
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

    [Header("Shop")]
    public List<ShopEntry> catalog; //the items you can buy
    public Transform dropZone; // where purchased items land
    public float dropScatterRadius = 0.5f; // spreads simultaneous purchases apart instead of stacking them exactly

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
        Debug.Log("shopping start");
        playersReadyText.text = (playersReady.Value + " / " + InstanceFinder.NetworkManager.ClientManager.Clients.Count);
        voteScreen.SetActive(false);
        shopScreen.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartGame()
    {
        NetworkManager.SceneManager.LoadGlobalScenes(sld);
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
    private void UpdateReadyText(int i)
    {
        playersReadyText.text = (i + " / " + InstanceFinder.NetworkManager.ClientManager.Clients.Count);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerLeftReadyZone()
    {
        playersReady.Value--;
        UpdateReadyText(playersReady.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerEnterReadyZone()
    {
        playersReady.Value++; 
        UpdateReadyText(playersReady.Value);
    }


     [ServerRpc(RequireOwnership = false)]
    public void TryPurchaseItem(int catalogIndex, NetworkConnection conn = null)
    {
        if (catalogIndex < 0 || catalogIndex >= catalog.Count) return;
 
        ShopEntry entry = catalog[catalogIndex];
 
        if (!GameManager.Instance.TrySpendCredits(entry.cost))
        {
            PurchaseFailed(conn, "Not enough credits!");
            return;
        }
 
        SpawnPurchasedItem(entry.item);
    }
 
    [Server]
    private void SpawnPurchasedItem(ItemSO item)
    {
        Vector2 offset = UnityEngine.Random.insideUnitCircle * dropScatterRadius;
        Vector3 pos = dropZone.position + new Vector3(offset.x, offset.y, 0f);
 
        GameObject go = Instantiate(item.itemInteractablePrefab, pos, Quaternion.identity);
        ServerManager.Spawn(go);
    }
 
    [TargetRpc]
    private void PurchaseFailed(NetworkConnection conn, string reason)
    {
        if (ShopMenuUI.Instance) ShopMenuUI.Instance.ShowMessage(reason);
    }

}
