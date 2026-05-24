using System.Collections;
using UnityEngine;
using UnityEngine.Events; 

/// <summary>
/// Handles player movement, jumping, and ground pounding mechanics.
/// Includes advanced physics checks to prevent double-jumping, mid-air glitches,
/// and micro-landing audio/animation bugs on collider seams.
/// Optimized for Unity 6.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // --- 1. MOVEMENT SETTINGS ---
    [Header("Movement & Camera")]
    [Tooltip("Normal movement speed.")]
    public float moveSpeed = 8f;
    [Tooltip("Multiplier applied when Left Shift is held.")]
    public float sprintMultiplier = 1.5f;
    [Tooltip("How fast the character rotates to face the movement direction.")]
    public float rotationSpeed = 15f;
    [Tooltip("Reference to the Main Camera to make movement relative to screen view.")]
    public Transform mainCamera;

    // --- 2. JUMP & ACTION PHYSICS ---
    [Header("Jump & Action Physics")]
    [Tooltip("Exact vertical velocity applied when jumping.")]
    public float jumpForce = 8.5f;
    [Tooltip("Downward force applied during a Ground Pound.")]
    public float groundPoundForce = 30f;
    [Tooltip("Seconds the character hangs in the air before pounding down.")]
    public float hangTime = 0.2f;
    
    [Header("Game Feel (Mario-like Physics)")]
    [Tooltip("Gravity multiplier when falling to make it feel heavy/fast.")]
    public float fallMultiplier = 2.5f;
    [Tooltip("Gravity multiplier when releasing the jump button early (Low Jump).")]
    public float lowJumpMultiplier = 2f;
    [Tooltip("Time in seconds the player can still jump after walking off a ledge.")]
    public float coyoteTime = 0.15f;
    [Tooltip("Time in seconds the game remembers a jump press before hitting the ground.")]
    public float jumpBufferTime = 0.15f;

    // --- 3. GROUND DETECTION ---
    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundDistance = 0.5f; 
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
    private bool wasGrounded; 
    private bool isGroundPounding = false;

    // --- ANIMATION ---
    [Header("Animation")]
    public Animator characterAnimator; 

    // --- TIMERS FOR GAME FEEL & BUG FIXES ---
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpCooldownTimer; 
    
    // NEW FIX: Tracks how long the player has actually been off the ground
    private float airTimer = 0f; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; 
        
        if (mainCamera == null) mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;

        // 1. Strict Ground Detection
        if (rb.linearVelocity.y <= 0.1f)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            isGrounded = false; 
        }

        // 2. Air Timer Logic (BUG FIX: Seam Glitches)
        if (!isGrounded)
        {
            airTimer += Time.deltaTime;
        }

        // 3. Fire Landing Event
        // Only fire if we actually spent a meaningful amount of time in the air (>0.15s).
        // This entirely ignores the tiny 1-frame bumps when walking over hexagon seams.
        if (isGrounded && !wasGrounded && !isGroundPounding)
        {
            if (airTimer > 0.15f)
            {
                onLand?.Invoke(); 
            }
        }
        
        // Reset air timer when firmly on the ground
        if (isGrounded)
        {
            airTimer = 0f;
        }

        wasGrounded = isGrounded;

        // 4. Handle Coyote Time 
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        // 5. Handle Jump Buffering 
        jumpBufferCounter = Input.GetButtonDown("Jump") ? jumpBufferTime : jumpBufferCounter - Time.deltaTime;

        // 6. Read Movement Input
        if (!isGroundPounding)
        {
            CalculateCameraRelativeMovement();
        }

        // 7. Execute Jump
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isGroundPounding && jumpCooldownTimer <= 0f)
        {
            PerformJump();
        }

        // 8. Execute Ground Pound 
        if ((Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl)) && !isGrounded && !isGroundPounding)
        {
            StartCoroutine(GroundPoundRoutine());
        }
        
        // 9. Send physical state to the Animator
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

        Vector3 camForward = mainCamera.forward;
        Vector3 camRight = mainCamera.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        movementInput = (camForward * vertical + camRight * horizontal).normalized;
    }

    private void MovePlayer()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector3 targetVelocity = movementInput * currentSpeed;
        
        targetVelocity.y = rb.linearVelocity.y; 
        
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
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        jumpCooldownTimer = 0.2f;
        
        // BUG FIX 1: Force the air timer so the jump animation triggers instantly!
        airTimer = 0.2f; 

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        onJump?.Invoke(); 
    }

    private void ApplyAdvancedGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }
    
    private void UpdateAnimator()
    {
        if (characterAnimator != null)
        {
            characterAnimator.SetFloat("Speed", movementInput.magnitude);

            // ==========================================
            // BUG FIX 2: ANIMATION FLICKER/STUTTER RESOLUTION
            // We only tell the animator we are airborne if we have been airborne for more than 0.1 seconds,
            // OR if we explicitly triggered a jump/ground pound.
            // This filters out the 1-frame gaps when walking over hexagon seams.
            // ==========================================
            bool animatorGrounded = isGrounded || (airTimer < 0.1f);
            characterAnimator.SetBool("isGrounded", animatorGrounded);
        }
    }

    private IEnumerator GroundPoundRoutine()
    {
        isGroundPounding = true;

        rb.linearVelocity = Vector3.zero; 
        rb.useGravity = false;      

        yield return new WaitForSeconds(hangTime); 

        rb.useGravity = true;
        rb.linearVelocity = new Vector3(0f, -groundPoundForce, 0f);

        yield return new WaitUntil(() => isGrounded);

        onGroundPoundImpact?.Invoke(); 
        
        Debug.Log("Ground impact detected!");

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

    public void ResetState()
    {
        StopAllCoroutines(); 
        isGroundPounding = false; 
        jumpCooldownTimer = 0f; 
        airTimer = 0f; // Clean up timer on respawn
        
        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero; 
        }
    }
}