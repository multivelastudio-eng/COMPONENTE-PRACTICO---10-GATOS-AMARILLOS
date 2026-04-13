using UnityEngine;
using System.Collections;

/// <summary>
/// Static Arena Camera: Stays in a fixed position but allows for cinematic screen shakes.
/// Compliant with high-quality game development standards.
/// </summary>
public class CameraShaker : MonoBehaviour
{
    // --- INTERNAL STATE VARIABLES ---
    private Vector3 originalPosition;
    private Vector3 currentShakeOffset = Vector3.zero;

    void Start()
    {
        // Memorize the exact position you set in the Unity Editor
        originalPosition = transform.position;
    }

    void Update()
    {
        // Keep the camera at its original position plus any active impact offset
        transform.position = originalPosition + currentShakeOffset;
    }

    // ==========================================
    // CINEMATIC IMPACT SYSTEM
    // ==========================================

    /// <summary>
    /// Triggers a screen shake effect without moving the camera from its base position.
    /// </summary>
    /// <param name="duration">Duration of the impact in seconds.</param>
    /// <param name="magnitude">Intensity of the vibration.</param>
    public void TriggerShake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Generate a random shake point within a sphere based on magnitude
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            currentShakeOffset = new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            
            // Wait until the next frame before continuing
            yield return null; 
        }

        // Return to perfect stillness after the duration ends
        currentShakeOffset = Vector3.zero;
    }
}