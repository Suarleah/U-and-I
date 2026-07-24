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
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Threading.Tasks;

public class VoteManager : NetworkBehaviour
{

    public static VoteManager Instance;

    [Header("Voting Screen")]
    private PatientManager patientManager;
    public GameObject playerName;
    public Transform playerList;
    public Color[] playerColors;
    [SerializeField] GameObject cursorPrefab;
    public int playersToLoad = 0;
    public int votesCast = 0;
    public List<NetworkObject> networkObjects = new List<NetworkObject>();
    public List<GameObject> cursors = new List<GameObject>();
    public GameObject[] patientChoices;
    public Canvas voteCanvas;

    [Header("Fake Voting Visual")]
    public int spinCycles = 3; // how many full laps around all patients to do before landing on the winner
    public float spinInterval = 1f; // how long to wait between each pulse during the spin

    [Header("Shop Screen")]
    private ShopManager shopManager;
    public Transform[] spawnPoints;
    


    async void Start()
    {

        NetworkManager.SceneManager.OnClientPresenceChangeEnd += PlayerDoneLoading;
        playersToLoad = RelayManager.Instance.currentPlayersCount;
        Instance = this;
        patientManager = PatientManager.Instance;
        shopManager = ShopManager.Instance;

        List<PatientSO> ethanPoop = patientManager.getRandomUnusedPatients(patientChoices.Length);
        for (int i = ethanPoop.Count - 1; i >= 0; i--)
        {
            Voter v = patientChoices[i].GetComponent<Voter>();
            v.me = ethanPoop[i];
            
            patientChoices[i].GetComponent<Image>().sprite = ethanPoop[i].myPhoto;
            v.infoText.text = ethanPoop[i].desc;
        }


    }

    [Server]
    public void PlayerDoneLoading(ClientPresenceChangeEventArgs arrghs) // yarrrr!!!!
    {
        GameObject localPlayer = arrghs.Connection.FirstObject.gameObject;
        networkObjects.Add(localPlayer.GetComponent<NetworkObject>());
        playersToLoad--;
        //Debug.Log(playersToLoad);

        if (playersToLoad == 0)
        {
            Debug.Log("I'm going to all players");
            AllPlayersDoneLoading();
        }

    }

    [Server]
    public void DidAllPlayersVote()
    {
        if (votesCast == networkObjects.Count)
        {
            AllPlayersVoted();
        }
    }



    [Server]
    public void AllPlayersVoted()
    {
        //disabled voting once everyone has voted
        for (int i = patientChoices.Length - 1; i >= 0; i--)
        {
            Voter v = patientChoices[i].GetComponent<Voter>();
            v.votable = false;
        }


        List<int> votes = new List<int>();
        for (int i = 0; i < patientChoices.Length; i++)
        {
            for (int x = 0; x < patientChoices[i].GetComponent<Voter>().votesForMe.Value; x++)
            {
                votes.Add(i);
            }
        }

        if (votes.Count == 0)
        {
            // Nobody voted, pick a random patient
            int fallback = UnityEngine.Random.Range(0, patientChoices.Length);
            StartCoroutine(RevealWinner(fallback)); // play the spin animation, then make "fallback" the winner once it's done
            return;
        }

        int winner = UnityEngine.Random.Range(0, votes.Count);
        Debug.Log("winner is:" + winner);
        StartCoroutine(RevealWinner(votes[winner])); // play the spin animation, then make this patient the winner once it's done
    }

    [Server]
    private IEnumerator RevealWinner(int winnerIndex) // server-side coroutine that plays the spin and picks the winner once it's done
    {
        VotingVisual(winnerIndex); // tell every client to start playing the spinning reveal animation too

        for (int cycle = 0; cycle < spinCycles; cycle++) // do the same full laps the clients are doing
        {
            for (int i = 0; i < patientChoices.Length; i++)
            {
                yield return new WaitForSeconds(spinInterval); // wait the same amount of time per step as the clients do
            }
        }

        for (int i = 0; i <= winnerIndex; i++) // do the same final lap that stops on the winner
        {
            yield return new WaitForSeconds(spinInterval); // wait the same amount of time per step as the clients do
        }



        patientManager.selectPatient(patientChoices[winnerIndex].GetComponent<Voter>().me);
        foreach (GameObject o in cursors)
        {
            Despawn(o);
        }

        foreach (NetworkObject n in networkObjects)
        {
            n.gameObject.SetActive(true);
        }
        SetActiveObservers(networkObjects.ToArray());

        shopManager.BeginShopping();
    }

    [ObserversRpc]
    private void SetActiveObservers(NetworkObject[] n)
    {
        foreach (NetworkObject o in n)
        {
            o.gameObject.SetActive(true);
        }
    }

    [ObserversRpc]
    void VotingVisual(int windex) // tells every client to play the reveal animation, landing on windex
    {
        StartCoroutine(PlayVotingVisual(windex)); // kick off the actual animation coroutine on this client
    }

    private IEnumerator PlayVotingVisual(int windex) // the coroutine that actually plays the spinning animation
    {
        Animator childAnim = new Animator();

        //Cycle through a few times for bullshit
        for (int cycle = 0; cycle < spinCycles; cycle++) // do a few full laps through every patient first
        {
            for (int i = 0; i < patientChoices.Length; i++)
            {
                //StartCoroutine(PulseVoter(i)); // grow this patient's vote icons
                Voter v = patientChoices[i].GetComponent<Voter>(); // grab the Voter script for this patient

                for (int j = 0; j < v.voteHolder.childCount; j++) // go through every vote icon currently parented under this patient
                {
                    childAnim = v.voteHolder.GetChild(j).GetComponent<Animator>(); // try to get that vote icon's animator
                    if (childAnim != null) // only trigger it if it actually has one
                    {
                        childAnim.SetTrigger("myTurn"); // play the animation on that vote icon
                        yield return new WaitForSeconds(spinInterval);
                    }
                }
            }
        }

        childAnim.SetTrigger("win");
        yield return null;
    }

    [Server]
    async Task AllPlayersDoneLoading()
    {
        for (int i = 0; i < networkObjects.Count; i++)
        {
            // For each player whom is connected
            NetworkObject no = networkObjects[i];
            // The object I am talking about is from my array of objects

            GameObject localPlayer = no.gameObject;
            // The local object is the local connections object

            GameObject name = Instantiate(playerName, playerList);
            Spawn(name, no.Owner);
            // make a name and then make that name spawn on the network

            String myName = localPlayer.GetComponentInChildren<TextMeshProUGUI>().text;
            // Create their name and then spawn in on the network, get the players local name above their head

            GameObject cursor = Instantiate(cursorPrefab, voteCanvas.transform);
            Spawn(cursor, no.Owner);
            // create a cursor and then spawn it over the network

            cursor.GetComponent<NetworkCursor>().SetColor(playerColors[i]);
            cursors.Add(cursor);


            UpdateValuesOnClient(localPlayer.GetComponent<NetworkObject>(), cursor.GetComponent<NetworkObject>(),
                name.GetComponent<NetworkObject>(), myName, i);
            // Apperently you cannot pass GameObjects into RPC.....
            // Actually fuck this stupid bullshit
        }
        
    }

    [ObserversRpc]
    void UpdateValuesOnClient(NetworkObject localPlayer, NetworkObject cursor, NetworkObject nameText, String name, int i)
    {
        localPlayer.gameObject.SetActive(false);
        localPlayer.transform.position = spawnPoints[i].position;
        // Move the player again because IDGAF

        cursor.transform.SetParent(voteCanvas.transform, false);
        // The cursors color fene;jibr;g

        nameText.GetComponentInChildren<TextMeshProUGUI>().color = playerColors[i];
        nameText.GetComponentInChildren<TextMeshProUGUI>().text = name;
        nameText.transform.SetParent(playerList, false);

    }

    PlayerMovement FindLocalPlayer()
    {
        return FishNet.InstanceFinder.ClientManager.Connection.FirstObject.gameObject.GetComponent<PlayerMovement>();
    }
}