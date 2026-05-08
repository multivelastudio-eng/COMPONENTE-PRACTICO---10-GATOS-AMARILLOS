using System.Collections;
using UnityEngine;

/// <summary>
/// Defines the colors available for the platforms.
/// </summary>
public enum PlatformColor 
{ 
    Yellow, Blue, Cyan, Orange, Red, Pink, Green 
}

/// <summary>
/// Controls the behavior of individual hexagonal platforms, including dropping and resetting.
/// Manages dynamic layer switching to prevent mid-air collision glitches.
/// </summary>
public class HexagonPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    [Tooltip("The designated color for this platform.")]
    public PlatformColor platformColor;
    
    [Tooltip("How far the platform falls when incorrect.")]
    public float dropDistance = 15f;
    
    [Tooltip("The speed at which the platform drops and rises.")]
    public float dropSpeed = 8f;

    // Internal position tracking
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    
    // Internal layer management for physics bug fixing
    private int defaultLayer;
    private int groundLayer;

    void Start()
    {
        // Store the initial position of the platform when the game starts
        originalPosition = transform.position;
        targetPosition = originalPosition;

        // BUG FIX: Store layers so the platform stops acting as "Ground" when it drops.
        // The "Default" layer will be ignored by the player's GroundMask.
        defaultLayer = LayerMask.NameToLayer("Default");
        
        // We assume the platform is correctly assigned to the "Suelo" layer in the Editor at start.
        groundLayer = gameObject.layer; 
    }

    void Update()
    {
        // Smoothly interpolate the position to create a floating/falling effect
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dropSpeed);
    }

    /// <summary>
    /// Commands the platform to fall down into the void.
    /// Removes its "Ground" status to prevent the player from jumping on it mid-air.
    /// </summary>
    public void Drop()
    {
        targetPosition = originalPosition + (Vector3.down * dropDistance);
        
        // Remove the ground layer so the player's physics radar ignores it while falling
        gameObject.layer = defaultLayer; 
    }

    /// <summary>
    /// Commands the platform to return to its original safe position.
    /// Restores its "Ground" status so the player can land on it safely.
    /// </summary>
    public void ResetPlatform()
    {
        targetPosition = originalPosition;
        
        // Restore the original ground layer so the player can walk on it again
        gameObject.layer = groundLayer; 
    }
}