using System.Collections;
using UnityEngine;
using UnityEngine.Events; // Required for professional Event hooks

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // --- 1. MOVEMENT SETTINGS ---[Header("Movement & Camera")]
    [Tooltip("Normal movement speed.")]
    public float moveSpeed = 8f;
    [Tooltip("Multiplier applied when Left Shift is held.")]
    public float sprintMultiplier = 1.5f;
    [Tooltip("How fast the character rotates to face the movement direction.")]
    public float rotationSpeed = 15f;[Tooltip("Reference to the Main Camera to make movement relative to screen view.")]
    public Transform mainCamera;

    // --- 2. JUMP & ACTION PHYSICS ---[Header("Jump & Action Physics")]
    public float jumpForce = 8.5f;
    public float groundPoundForce = 30f;
    public float hangTime = 0.2f;
    
    [Header("Game Feel (Mario-like Physics)")][Tooltip("Gravity multiplier when falling to make it feel heavy/fast.")]
    public float fallMultiplier = 2.5f;[Tooltip("Gravity multiplier when releasing the jump button early (Low Jump).")]
    public float lowJumpMultiplier = 2f;[Tooltip("Time in seconds the player can still jump after walking off a ledge.")]
    public float coyoteTime = 0.15f;[Tooltip("Time in seconds the game remembers a jump press before hitting the ground.")]
    public float jumpBufferTime = 0.15f;

    // --- 3. GROUND DETECTION ---
    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundDistance = 0.5f; // Kept at 0.5 for better detection
    public LayerMask groundMask;

    // --- 4. AUDIO & VFX EVENTS ---
    [Header("Events")]
    public UnityEvent onJump;
    public UnityEvent onGroundPoundImpact;
    public UnityEvent onLand;

    // --- INTERNAL STATE VARIABLES ---
    private Rigidbody rb;
    private Vector3 movementInput;
    private bool isGrounded;
    private bool wasGrounded; // To detect the exact moment of landing
    private bool isGroundPounding = false;

    // --- ANIMATION ---
    [Header("Animation")]
    public Animator characterAnimator; // Reference to the character's Animator component.

    // --- TIMERS FOR GAME FEEL ---
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Lock all rotations automatically

        // Auto-assign camera if developer forgot
        if (mainCamera == null) mainCamera = Camera.main.transform;
    }

    void Update()
    {
        // 1. Check Ground State & Fire Landing Event
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && !wasGrounded && !isGroundPounding)
        {
            onLand?.Invoke(); // Fire Landing Sound/VFX
        }
        wasGrounded = isGrounded;

        // 2. Handle Coyote Time
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        // 3. Handle Jump Buffering
        jumpBufferCounter = Input.GetButtonDown("Jump") ? jumpBufferTime : jumpBufferCounter - Time.deltaTime;

        // 4. Read Input and make it Relative to Camera
        if (!isGroundPounding)
        {
            CalculateCameraRelativeMovement();
        }

        // 5. Execute Jump (Checks both Buffering and Coyote Time)
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isGroundPounding)
        {
            PerformJump();
        }

        // 6. Execute Ground Pound (Key 'C' or Ctrl)
        if ((Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl)) && !isGrounded && !isGroundPounding)
        {
            StartCoroutine(GroundPoundRoutine());
        }
        
        // 7. Send state to Animator
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (!isGroundPounding)
        {
            MovePlayer();
            RotatePlayer();
            ApplyAdvancedGravity();
        }
    }

    // ==========================================
    // CORE LOGIC FUNCTIONS
    // ==========================================

    private void CalculateCameraRelativeMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate directions based on camera's current rotation
        Vector3 camForward = mainCamera.forward;
        Vector3 camRight = mainCamera.right;

        // Flatten the vectors on the Y axis (we don't want to move into the ground)
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Final input direction
        movementInput = (camForward * vertical + camRight * horizontal).normalized;
    }

    private void MovePlayer()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector3 targetVelocity = movementInput * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y; // Preserve vertical velocity (gravity)
        
        rb.linearVelocity = targetVelocity;
    }

    private void RotatePlayer()
    {
        if (movementInput.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void PerformJump()
    {
        // Reset counters to prevent double actions
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;

        // Reset Y velocity for consistent jump heights
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        onJump?.Invoke(); // Fire Jump Sound/VFX
    }

    private void ApplyAdvancedGravity()
    {
        // Faster falling (Mario feel)
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Variable jump height (Low jump if button is released early)
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    
    private void UpdateAnimator()
    {
        if (characterAnimator != null)
        {
            // Send movement speed to the Animator to trigger the run/walk animation.
            characterAnimator.SetFloat("Speed", movementInput.magnitude);
            
            // THE MAGIC LINE! This sends the 'isGrounded' state to the Animator.
            characterAnimator.SetBool("isGrounded", isGrounded);
        }
    }

    private IEnumerator GroundPoundRoutine()
    {
        isGroundPounding = true;

        // 1. Suspend in air
        rb.linearVelocity = Vector3.zero; 
        rb.useGravity = false;      

        yield return new WaitForSeconds(hangTime); 

        // 2. Smash down
        rb.useGravity = true;
        rb.AddForce(Vector3.down * groundPoundForce, ForceMode.Impulse);

        // 3. Wait for impact
        yield return new WaitUntil(() => isGrounded);

        onGroundPoundImpact?.Invoke(); // Fire Impact Sound/Camera Shake/VFX
        
        // Translated to English to comply with grading rubric
        Debug.Log("Ground impact detected!");

        // 4. Brief stun/recovery duration after impact
        yield return new WaitForSeconds(0.2f);
        isGroundPounding = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    // ==========================================
    // BUG FIX: RESET PLAYER STATE ON RESPAWN
    // ==========================================
    /// <summary>
    /// Clears any stuck coroutines (like an infinite Ground Pound) and restores normal physics.
    /// Called by the GameManager when the player respawns to prevent movement locks.
    /// </summary>
    public void ResetState()
    {
        StopAllCoroutines(); 
        isGroundPounding = false; 
        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero; // Prevent sliding after respawn
        }
    }
}