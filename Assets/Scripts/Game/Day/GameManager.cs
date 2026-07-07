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

public class GameManager : NetworkBehaviour
{
    public readonly SyncVar<float> credits = new SyncVar<float>(); //company credits are shared across all players (basically just money but evil corporation credits since youre paid in credits which can only be used in the company)
    [SerializeField] private TextMeshProUGUI creditsText;
    public static GameManager Instance;
    private PatientManager patientManager;
    public int day; //the day #
    public string winScene; private SceneLoadData win;
    public string loseScene; private SceneLoadData lose;

    public readonly SyncList<int> players = new SyncList<int>(); //list of all players in the game



    void Awake()
    {
        Instance = this;
        win = new SceneLoadData(winScene);
        win.ReplaceScenes = ReplaceOption.All;

        lose = new SceneLoadData(loseScene);
        lose.ReplaceScenes = ReplaceOption.All;

        patientManager = PatientManager.Instance;
        credits.OnChange += OnChangeCredits;

        creditsText.text = "credits: "+ credits.Value;
    }

    private void OnChangeCredits(float prev, float next, bool asServer)
    {
        creditsText.text = "credits: "+ next;
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
        NetworkManager.SceneManager.LoadGlobalScenes(win);
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

}
