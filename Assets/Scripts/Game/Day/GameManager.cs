using UnityEngine;
using FishNet;
using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using FishNet.Managing.Scened;
using Unity.VectorGraphics;
using FishNet.Demo.AdditiveScenes;
using TMPro;
using System.Xml;

public class GameManager : NetworkBehaviour
{
    public readonly SyncVar<float> credits = new SyncVar<float>(); //company credits are shared across all players (basically just money but evil corporation credits since youre paid in credits which can only be used in the company)
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI quotaText;
    public static GameManager Instance;
    private PatientManager patientManager;
    
    public string winScene; private SceneLoadData win;
    public string loseScene; private SceneLoadData lose;

    public readonly SyncList<int> players = new SyncList<int>(); //list of all players in the game
    public readonly SyncVar<int> day = new SyncVar<int>(); //the day #
    public readonly SyncVar<float> daytime = new SyncVar<float>(); //the amount of time left in the day

    public readonly SyncVar<int> quota = new SyncVar<int>(); //the day #

    [SerializeField] private List<int> quotas; //list of quotas in order that players need to pass
    void Awake()
    {
        Instance = this;
        win = new SceneLoadData(winScene);
        win.ReplaceScenes = ReplaceOption.All;

        lose = new SceneLoadData(loseScene);
        lose.ReplaceScenes = ReplaceOption.All;

        patientManager = PatientManager.Instance;
        credits.OnChange += OnChangeCredits;
        day.OnChange += OnChangeDay;
        quota.OnChange += OnChangeQuota;

        creditsText.text = "Credits: " + credits.Value;
        dayText.text = "Day: " + day.Value;
        quotaText.text = "Quota: " + quota.Value;
        FishNet.InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoadEnd;
    }

    private void OnSceneLoadEnd(SceneLoadEndEventArgs args)
    {
        if (args.QueueData.AsServer)
        {
            foreach (UnityEngine.SceneManagement.Scene scene in args.LoadedScenes) //for some reason the default is the vector one which doesnt work, thats why i used the full name
            {
                if (scene.name == "Vote Screen")
                {
                    day.Value++;
                    if (quotas.Count > day.Value)
                    {
                        quota.Value = quotas[(day.Value-1)/3]; 
                    } else //if not a set value, just increase exponentially
                    {
                        quota.Value = (int)(quota.Value * 1.5f);
                    }
                    break;
                }
                if (scene.name == "entity test scene")
                {
                    daytime.Value = 300;
                    break;
                }

                
            }

            foreach (PlayerMovement p in GetPlayers())
            {
                p.stats.ResetPlayer();
            }
        }
    }


    private void OnChangeDay(int prev, int next, bool asServer)
    {
        dayText.text = "Day: " + next;
    }

    private void OnChangeCredits(float prev, float next, bool asServer)
    {
        creditsText.text = "Credits: " + next;
    }
    
    private void OnChangeQuota(int prev, int next, bool asServer)
    {
        quotaText.text = "Quota: " + next;
    }

    [ServerRpc(RequireOwnership = false)]
    public void playerDied(GameObject player) //players call this whenever they die
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!GetPlayers()[i].stats.isDead.Value)
            {
                return;
            }
        }


        //if every player is dead, end game here but i have no method to end the game so i have nothing here
        //StartCoroutine(EndGame());
        Debug.Log("Game over!");
        NetworkManager.SceneManager.LoadGlobalScenes(lose);
    }

/*    public void ReloadScene()
    {
        patientManager.SpawnAllPatients();
        foreach (PlayerMovement p in GetPlayers())
        {
            PlayerStats s = p.GetComponent<PlayerStats>();
            s.isDead.Value = false;
        }
        NetworkManager.SceneManager.LoadGlobalScenes(me);
    }
*/

    [ServerRpc(RequireOwnership = false)]
    public void PlayerClockedOut(GameObject player) //players call this whenever they try to clock out, if all players have clocked out, the day ends
    {
        
        for (int i = 0; i < players.Count; i++)
        {
            if (!GetPlayers()[i].stats.isClockedOut.Value)
            {
                return;
            }
        }
        Debug.Log("All players have clocked out! Day ending!");
        AllPlayersClocked();
        //After clock out, anything that happens will not count (so for the short animation time they cant die or make money or something)
    }
    public void AllPlayersClocked()
    {
        foreach (PlayerMovement p in GetPlayers())
        {
            p.stats.ResetPlayer();
        }
        if (day.Value%3 == 0) //if its a quota day
        {
            if (credits.Value <= quota.Value) //check if quota was reached
            {
                NetworkManager.SceneManager.LoadGlobalScenes(lose);
            } else
            {
                NetworkManager.SceneManager.LoadGlobalScenes(win);
            }
        } else
        {
            NetworkManager.SceneManager.LoadGlobalScenes(win);
        }
        
        
    }

    public List<PlayerMovement> GetPlayers()
    {
        List<PlayerMovement> result = new List<PlayerMovement>();
        foreach (int id in players)
        {
            NetworkObject netObj;
            FishNet.InstanceFinder.NetworkManager.ClientManager.Objects.Spawned.TryGetValue(id, out netObj);
            if (netObj != null)
            {
                result.Add(netObj.GetComponent<PlayerMovement>());
            }
        }
        return result;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddCredits(int amt)
    {
        credits.Value += amt;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubtractCredits(int amt)
    {
        credits.Value -= amt;
    }

    void Update()
    {
        if (!IsServerStarted)
        {
            return;
        }
        if (daytime.Value >= 0)
        {
            daytime.Value -= Time.deltaTime;
        }
        
    }

}
