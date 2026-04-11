using Steamworks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
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
            // 
            netPosition.Value = rb.position;
        }

        // Subscribe to remote changes
        netName.OnValueChanged += (_, n) => ApplyName(n.ToString());
        netPosition.OnValueChanged += OnRemotePositionChanged;

        // Apply current values
        ApplyName(netName.Value.ToString());

        // Disable physics for non-owner eleport them instead
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
        var kb = Keyboard.current;
        if (kb == null) return;

        Vector2 dir = Vector2.zero;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) dir.y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) dir.y -= 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) dir.x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x += 1f;

        rb.linearVelocity = dir.normalized * speed;

        // Sync position every physics step
        netPosition.Value = rb.position;

        if (spriteRenderer != null && dir.x != 0f)
            spriteRenderer.flipX = dir.x < 0f;
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
