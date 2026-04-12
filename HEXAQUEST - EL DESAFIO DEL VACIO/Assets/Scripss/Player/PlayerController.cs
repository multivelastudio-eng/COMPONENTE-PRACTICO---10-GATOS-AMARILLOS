using UnityEngine;

/// <summary>
/// Controls the main player character movement and jumping
/// on the hexagonal floating platform.
/// Assigned to: Caren Vargas Vela
/// Team: 3 Halcones Estrategicos - HexaQuest: El Desafio del Vacio
/// Course: Programacion para Videojuegos 213027A - UNAD
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;           // Walking/running speed
    public float jumpForce = 7f;           // Force applied on jump
    public float groundCheckRadius = 0.3f; // Radius to detect ground surface

    [Header("References")]
    public Transform groundCheck;          // Empty child object below player feet
    public LayerMask groundLayer;          // Layer assigned to hexagon platforms

    // Private variables
    private Rigidbody rb;
    private bool isGrounded;
    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        // Get the Rigidbody component attached to this player
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Read player input axes every frame
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Check if the player is standing on a platform (grounded)
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Handle jump input: only allowed when the player is on the ground
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // Apply movement using physics in FixedUpdate for smooth results
        MovePlayer();
    }

    /// <summary>
    /// Moves the player based on horizontal and vertical input axes.
    /// Maintains current vertical velocity to preserve gravity effect.
    /// </summary>
    void MovePlayer()
    {
        // Build the movement direction from input
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);

        // Normalize to prevent faster diagonal movement
        if (movement.magnitude > 1f)
            movement.Normalize();

        // Apply velocity while keeping the existing vertical (gravity) velocity
        rb.linearVelocity = new Vector3(
            movement.x * moveSpeed,
            rb.linearVelocity.y,
            movement.z * moveSpeed
        );
    }

    /// <summary>
    /// Applies a vertical impulse force to make the player jump.
    /// Resets vertical velocity first for a consistent jump height.
    /// </summary>
    void Jump()
    {
        // Reset existing vertical velocity before jumping for predictable height
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Apply upward impulse force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // --- DEBUG: Visualize the ground check sphere in the Unity Editor ---
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
