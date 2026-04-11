using UnityEngine;
using Steamworks;
public class SteamTest : MonoBehaviour {
    void Start() {
        try {
            SteamClient.Init(480, true);
            Debug.Log("Steam OK — " + SteamClient.Name);
        } catch (System.Exception e) { Debug.LogError(e); }
    }
    void OnDisable() { try { SteamClient.Shutdown(); } catch {} }
}