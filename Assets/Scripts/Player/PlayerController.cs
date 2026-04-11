using Steamworks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;

    [Header("Visual")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TMPro.TMP_Text nameLabel;

    private NetworkVariable<Vector2> netPosition = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<FixedString64Bytes> netName = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // top down
        rb.freezeRotation = true; // topdown
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public override void OnNetworkSpawn()
    {
        // 's values once on spawn
        if (IsOwner) // If I am the owner of this gameobject
        {
            netName.Value = SteamClient.IsValid ? new FixedString64Bytes(SteamClient.Name) : new FixedString64Bytes("Player");
            // the name is equal to their steam name if it is valid, if not then just "player"
            netPosition.Value = rb.position;
        }

        // remote changes
        netName.OnValueChanged += (_, n) => ApplyName(n.ToString());
        netPosition.OnValueChanged += OnRemotePositionChanged;

        // Apply current values
        ApplyName(netName.Value.ToString());

        // Disable physics for non-owner teleport them instead
        if (!IsOwner)
            rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (!IsOwner || !IsSpawned) return;
        HandleMovement();
    }

    void HandleMovement()
    {
        // Sync position every physics step
        netPosition.Value = rb.position;
    }

    private void OnRemotePositionChanged(Vector2 prev, Vector2 next)
    {
        rb.MovePosition(Vector2.Lerp(rb.position, next, 0.5f));
    }


    void ApplyName(string playerName)
    {
        if (nameLabel != null) nameLabel.text = playerName;
        gameObject.name = $"Player_{playerName}";
    }

  
    void LateUpdate()
    {
          // Camera follow local only
        if (!IsOwner) return;
        var cam = Camera.main;
        if (cam == null) return;
        cam.transform.position = new Vector3(transform.position.x, transform.position.y, cam.transform.position.z);
    }
}
