using Steamworks;
using UnityEngine;

// Initializes Steam once at game launch and keeps it alive
public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }
    public const uint AppId = 480; // Spacewar test app, replace with your real app id later

    void Awake()
    {
        // Singleton enforcement
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Connect to Steam
        try
        {
            SteamClient.Init(AppId, true);
            Debug.Log($"Steam initialized: {SteamClient.Name} ({SteamClient.SteamId})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Steam failed to initialize: {e}");
        }
    }

    // Steam needs this called every frame so its events fire
    void Update()
    {
        SteamClient.RunCallbacks();
    }

    // Clean shutdown when the game closes
    void OnApplicationQuit()
    {
        SteamClient.Shutdown();
    }
}