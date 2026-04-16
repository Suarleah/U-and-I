using Steamworks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

// Networked top-down player: movement, name label, camera follow
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;

    [Header("Visual")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] TMPro.TMP_Text nameLabel;

    // Synced data: only the owning client writes, everyone reads
    private NetworkVariable<Vector2> netPosition = new(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<FixedString64Bytes> netName = new(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Rigidbody2D rb;

    // Configure rigidbody for top-down movement
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // Set initial values, hook up sync, freeze physics on remote copies
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            netName.Value = SteamClient.IsValid
                ? new FixedString64Bytes(SteamClient.Name)
                : new FixedString64Bytes("Player");
            netPosition.Value = rb.position;
        }

        netName.OnValueChanged += HandleNameChanged;
        netPosition.OnValueChanged += HandleRemotePositionChanged;

        ApplyName(netName.Value.ToString());

        if (!IsOwner)
            rb.bodyType = RigidbodyType2D.Kinematic;
    }

    // Clean up sync subscriptions when this player despawns
    public override void OnNetworkDespawn()
    {
        netName.OnValueChanged -= HandleNameChanged;
        netPosition.OnValueChanged -= HandleRemotePositionChanged;
    }

    // Only the owning client runs movement
    void Update()
    {
        if (!IsOwner || !IsSpawned) return;
        HandleMovement();
    }

    // Read keyboard, move the rigidbody, sync position and sprite flip
    void HandleMovement()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        Vector2 dir = Vector2.zero;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    dir.y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  dir.y -= 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  dir.x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x += 1f;

        rb.linearVelocity = dir.normalized * speed;
        netPosition.Value = rb.position;

        if (spriteRenderer != null && dir.x != 0f)
            spriteRenderer.flipX = dir.x < 0f;
    }

    // Smoothly move other players' visuals toward their synced position
    private void HandleRemotePositionChanged(Vector2 prev, Vector2 next)
    {
        rb.MovePosition(Vector2.Lerp(rb.position, next, 0.5f));
    }

    // Update the visual nameplate when the synced name changes
    private void HandleNameChanged(FixedString64Bytes prev, FixedString64Bytes next)
    {
        ApplyName(next.ToString());
    }

    // Set the nameplate text and the GameObject name
    void ApplyName(string playerName)
    {
        if (nameLabel != null) nameLabel.text = playerName;
        gameObject.name = $"Player_{playerName}";
    }

    // Camera follows only the local player
    void LateUpdate()
    {
        if (!IsOwner) return;
        var cam = Camera.main;
        if (cam == null) return;
        cam.transform.position = new Vector3(transform.position.x, transform.position.y, cam.transform.position.z);
    }
}