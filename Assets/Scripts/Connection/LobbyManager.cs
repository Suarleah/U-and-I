using UnityEngine;
using TMPro;
using System;
using FishNet.Managing.Scened;
using FishNet;
using UnityEngine.SceneManagement;
using FishNet.Object;
using Unity.Services.Core;
using FishNet.Connection;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text joinCodeDisplay;
    public TMP_InputField joinCodeInput;
    public GameObject menu;
    public GameObject lobby; private Canvas lobbyC;
    public GameObject loading;
    public String sceneToLoad;
    private SceneLoadData sld;
    public TMP_InputField changeName;

    async void Start()
    {
        await RelayManager.Instance.InitializeAsync();
        
        InstanceFinder.NetworkManager.SceneManager.OnLoadStart += OnLoadStart;  
        InstanceFinder.NetworkManager.SceneManager.OnLoadEnd += OnLoadEnd;
        InstanceFinder.NetworkManager.SceneManager.OnClientLoadedStartScenes += ClientJoined;
        // When a scene starts loading and ends loading, not actually switchibg the scene just loading it asyncornously
        sld = new SceneLoadData(sceneToLoad);
        sld.ReplaceScenes = ReplaceOption.All;

        lobbyC = lobby.GetComponent<Canvas>();
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
       // I am having trouble displaying the canvas because we need the cam to follow player
        lobbyC.renderMode = RenderMode.ScreenSpaceCamera;
        // Force it to show up
        lobbyC.renderMode = RenderMode.WorldSpace;
       // sld.MovedNetworkObjects = 
        
    }

    public void OnStartClicked()
    {
        InstanceFinder.NetworkManager.SceneManager.LoadGlobalScenes(sld);
    }

    public async void OnHostClicked()
    {
        string code = await RelayManager.Instance.CreateRelayAsync(RelayManager.Instance.maxPlayers);
        // get code from ro4;ieiirmnpv;ivb
        joinCodeDisplay.text = "Secret code: " + code;
        menu.SetActive(false);
        lobby.SetActive(true);
    }

    public async void OnJoinClicked()
    {
        await RelayManager.Instance.JoinRelayAsync(joinCodeInput.text.Trim().ToUpper());
        menu.SetActive(false);
        lobby.SetActive(true);
        // GET CODE FROM THE RELAY MANAGER
    }

    PlayerMovement FindLocalPlayer()
    {
        foreach (PlayerMovement p in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            if (p.IsOwner) return p;
        }
        return null;
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
}