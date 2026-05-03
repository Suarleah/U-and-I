using FishNet.Object;
using UnityEngine;

public class ReadyManager : NetworkBehaviour
{
    public static ReadyManager Instance;
    private MainMenuManager mainMenuManager;

    void Awake()
    {
        Instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient(); // This is like awake but for when I actually connect
        mainMenuManager = FindFirstObjectByType<MainMenuManager>();
    }

    [ServerRpc(RequireOwnership = false)]
    // Anyone can tell the server to update this
    public void PlayerEnter()
    {
        mainMenuManager.playersReady += 1;
        UpdatePlayerReadyText(mainMenuManager.playersReady, NetworkManager.ClientManager.Clients.Count);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerExit()
    {
        mainMenuManager.playersReady -= 1;
        UpdatePlayerReadyText(mainMenuManager.playersReady, NetworkManager.ClientManager.Clients.Count);
    }

    [ObserversRpc]
    public void UpdateBoxClients(float size)
    {
        mainMenuManager.progressBox.fillAmount = size;
    }

    [ObserversRpc]
    public void UpdatePlayerReadyText(int playersReady, int playersConnected)
    {
        mainMenuManager.playersReadyText.text = (playersReady + "/" + playersConnected);
    }
}