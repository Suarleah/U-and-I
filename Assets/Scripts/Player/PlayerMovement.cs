using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using FishNet;
using FishNet.Connection;


public class PlayerMovement : NetworkBehaviour
{
    //public float speed = 5f;
    [SerializeField] public PlayerStats stats;
    public InputActionAsset inputAsset;
    private InputAction moveAction;
    private Rigidbody2D playerRb;
    public GameObject visual;
    public GameObject ghostVisual;
    private TextMeshProUGUI nameText;
    private Animator animator;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private readonly SyncVar<string> playerName = new SyncVar<string>();

    public GameObject visionField; //object that allows player to see through fog of war

    public bool canMove; //added this since in some situations we might not want to allow player to move

    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        nameText = GetComponentInChildren<TextMeshProUGUI>();
        animator = GetComponentInChildren<Animator>();
        moveAction = inputAsset.FindAction("Move");
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
    }

    public override void OnStartClient()
    { // Called as cloient when join
        base.OnStartClient();

        playerName.OnChange += OnNameChanged;
        SetNameServerRpc(AuthenticationService.Instance.PlayerName);

        if (!IsOwner)
        {
            visionField.SetActive(false); //only the owner should have that player's vision
        }
        // Call OnNameChanged when playerName is changed

        cinemachineCamera.Priority = IsOwner ? 10 : 0;
        // The prioriity of the cinemachine is high if I am the owner of this gameobject
        // If I am not, then it is low because I don't want to use it
        // If you want every player to use one camera then just make the priority higher than 1

    }

    private void Update()
    {
        gameObject.layer = LayerMask.NameToLayer("Player");
        setGhostSpriteEnabled(false);
        setPlayerSpriteEnabled(true);
        if (stats.isDead.Value)
        {
            ghostUpdate();
        }
        if (!IsOwner) return;

        
        //gameObject.GetComponentInChildren<BoxCollider2D>().gameObject.layer = LayerMask.NameToLayer("Player");

        Vector2 dir = moveAction.ReadValue<Vector2>();
        if (canMove)
        {
            playerRb.linearVelocity = dir * stats.speed.Value;
            Animate();

            if (dir.x < 0) // If I pressing left
            {
                visual.transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
                // flip me visual... ARHHH!!!!!

            }
            if (dir.x > 0) // If I pressing right
            {
                visual.transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                // Yee Scallywags better be setting me rotation back to normal!! YARHHH!!!!
            }

            // Rigid body synced automatically by the NetworkTransform component :D
        }
    }

    private void ghostUpdate()
    {
        
        if (InstanceFinder.ClientManager.Connection.FirstObject.GetComponent<PlayerStats>().isDead.Value) //if youre the dead player, or if youre also dead, then you can see the ghost
        {
            setGhostSpriteEnabled(true);
        }
        setPlayerSpriteEnabled(false);

        if (!IsOwner) return;
        gameObject.layer = LayerMask.NameToLayer("Ghost");
        //gameObject.GetComponentInChildren<BoxCollider2D>().gameObject.layer = LayerMask.NameToLayer("Ghost");
    }

    

    private void Animate()
    {
        // No need to sync because of NetworkAnimator component on Visual :)
        animator.SetBool("moving", moveAction.ReadValue<Vector2>().magnitude != 0);
        // Playing run animation if the player is pressing the move button
    }

    private void ghostAnimate()
    {
        // No need to sync because of NetworkAnimator component on Visual :)
        //animator.SetBool("moving", moveAction.ReadValue<Vector2>().magnitude != 0);
        // Playing run animation if the player is pressing the move button
    }

    void OnNameChanged(string prev, string next, bool asServer)
    { // Sync vars send these 3 values
        nameText.text = next;
    }

    public void DisableMyInput() //i dont recommend using these because it will disable the entire input asset, ie: player wont be able to pause while this is disabled
    {
        inputAsset.Disable();
    }

    public void EnableMyInput()
    {
        inputAsset.Enable();
    }

    [ServerRpc]
    public void SetNameServerRpc(string name)
    {
        playerName.Value = name;
        stats.playerName.Value = name;
    }

    /* [ServerRpc] 
    public void MoveServerRpc(Vector2 dir)
    {
        playerRb.linearVelocity = dir * speed;
        MoveClientRpc(playerRb.position);
    }

    [ObserversRpc]
    public void MoveClientRpc(Vector2 pos)
    {
        playerRb.position = pos;
    }
    */

    private void setGhostSpriteEnabled(bool enabled)
    {
        ghostVisual.GetComponent<SpriteRenderer>().enabled = enabled; //set ghost visual and all related to invisible
        foreach (SpriteRenderer sprite in ghostVisual.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.enabled = enabled;
        }   
    }

    private void setPlayerSpriteEnabled (bool enabled)
    {
        visual.GetComponent<SpriteRenderer>().enabled = enabled; //set player visual and all related to invisible
        foreach (SpriteRenderer sprite in visual.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.enabled = enabled;
        }
    }
}