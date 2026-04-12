using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the behavior of each hexagonal platform tile.
/// Stable platforms remain fixed. Unstable platforms blink and disappear
/// after the player steps on them, simulating falling into the void.
/// 
/// Assigned to: Caren Vargas Vela
/// Team: 3 Halcones Estrategicos - HexaQuest: El Desafio del Vacio
/// Course: Programacion para Videojuegos 213027A - UNAD
/// </summary>
public class HexPlatformBehavior : MonoBehaviour
{
    [Header("Platform Type")]
    public bool isUnstable = false;     // If true, this tile falls after being stepped on
    public float delayBeforeFall = 2f;  // Seconds between step and disappearance
    public float blinkSpeed = 0.2f;     // Duration of each blink cycle (seconds)

    [Header("Visual Feedback")]
    public Color warningColor = Color.red;  // Color shown during the blink warning phase
    private Color originalColor;            // Stores the tile's original color
    private MeshRenderer meshRenderer;

    // State flag to prevent the fall sequence from triggering multiple times
    private bool isFalling = false;

    void Start()
    {
        // Cache the MeshRenderer and store original material color
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            originalColor = meshRenderer.material.color;
    }

    /// <summary>
    /// Triggered when the player physically lands on this tile.
    /// Only starts the fall sequence if the tile is unstable and not already falling.
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        // Verify the object that landed is tagged as Player
        if (collision.gameObject.CompareTag("Player") && isUnstable && !isFalling)
        {
            StartCoroutine(FallSequence());
        }
    }

    /// <summary>
    /// Coroutine that alternates the tile color as a warning, then disables the tile.
    /// The player has 'delayBeforeFall' seconds to react and jump off.
    /// </summary>
    IEnumerator FallSequence()
    {
        isFalling = true;

        float elapsed = 0f;

        // Blink loop: alternate between warning color and original color
        while (elapsed < delayBeforeFall)
        {
            if (meshRenderer != null)
            {
                // Switch to warning (red) color
                meshRenderer.material.color = warningColor;
                yield return new WaitForSeconds(blinkSpeed);

                // Restore original color
                meshRenderer.material.color = originalColor;
                yield return new WaitForSeconds(blinkSpeed);
            }
            else
            {
                // If no MeshRenderer, just wait
                yield return new WaitForSeconds(blinkSpeed * 2f);
            }

            elapsed += blinkSpeed * 2f;
        }

        // Deactivate the tile — it has "fallen into the void"
        gameObject.SetActive(false);
    }
}
