using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;
    public InputActionAsset inputAsset;
    private InputAction moveAction;
    private Rigidbody2D playerRb;
    public GameObject visual;
    private TextMeshProUGUI nameText;

    private readonly SyncVar<string> playerName = new SyncVar<string>();

    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        nameText = GetComponentInChildren<TextMeshProUGUI>();
        moveAction = inputAsset.FindAction("Move");
    }

    public override void OnStartClient()
    { // Called as cloient when join
        base.OnStartClient();
        
        playerName.OnChange += OnNameChanged;
        SetNameServerRpc(AuthenticationService.Instance.PlayerName);
        // Call OnNameChanged when playerName is changed
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 dir = moveAction.ReadValue<Vector2>();
        playerRb.linearVelocity = dir * speed;

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

    void OnNameChanged(string prev, string next, bool asServer)
    { // Sync vars send these 3 values
        nameText.text = next;
    }

    public void DisableMyInput()
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
}