using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.EventSystems;

public class Voter : NetworkBehaviour, IPointerClickHandler
{
    // We should make them pay taxes since we give them representation
    public GameObject textDesc;
    public Transform voteHolder;
    public readonly SyncVar<int> votesForMe = new SyncVar<int>(0);
    public PatientSO me;
    public PatientManager patientManager;
    public VoteManager voteManager;


    async void Start()
    {
        patientManager = PatientManager.Instance;

    }

    [Server]
    public void TheWinner()
    {
        patientManager.selectPatient(me);
        // Load next scene
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        WhoClickedMe();
    }
    [ObserversRpc]
    public void ClickedMeClient(NetworkObject cursorObject)
    {
        NetworkCursor c = cursorObject.GetComponent<NetworkCursor>();
        c.myVote.transform.SetParent(voteHolder);
        c.myVote.gameObject.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void WhoClickedMe(NetworkConnection connection = null)
    {
        NetworkCursor clickedMe = null;

        foreach (NetworkObject o in connection.Objects)
        {
            if (o.GetComponent<NetworkCursor>())
            {
                //For each object that the person who clicked this button owns
                clickedMe = o.GetComponent<NetworkCursor>();
                // If the object is their cursor, then set that as the cursor who clicked this
            }
        }
        // Get their vote icon, which is their color, and add it as a child of me
        clickedMe.myVote.transform.SetParent(voteHolder);
        clickedMe.myVote.gameObject.SetActive(true);

        votesForMe.Value++;
        ClickedMeClient(clickedMe.NetworkObject);
        // Visually parent the vote for Clients

        Voter prev = clickedMe.myPrevVote;
        // reference the last patient they voted for
        if (prev != null)
        {
            // if they have voted before
            prev.votesForMe.Value--;
            // minus one from their last vote because they are voting for me now
            VoteManager.Instance.votesCast--;
            // undo their previous vote count so we don't double count
        }

        // I am now their last vote
        clickedMe.myPrevVote = this;

        VoteManager.Instance.votesCast++;
        VoteManager.Instance.DidAllPlayersVote();
    }

}