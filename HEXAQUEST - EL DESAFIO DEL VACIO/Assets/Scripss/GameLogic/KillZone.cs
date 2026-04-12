using UnityEngine;

/// <summary>
/// Invisible trigger zone positioned below all platforms.
/// Detects when the player falls into the void and notifies the GameManager.
/// The player is then respawned at the starting position if lives remain.
/// 
/// Assigned to: Caren Vargas Vela
/// Team: 3 Halcones Estrategicos - HexaQuest: El Desafio del Vacio
/// Course: Programacion para Videojuegos 213027A - UNAD
/// </summary>
public class KillZone : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;      // The point where the player reappears after dying
    public GameManager gameManager;     // Reference to the central game manager

    /// <summary>
    /// Triggered when any collider enters this zone.
    /// Processes death logic only for the player object.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // Only react to the Player object
        if (other.CompareTag("Player"))
        {
            Debug.Log("[KillZone] Player fell into the void.");

            // Tell the GameManager the player died (reduces lives)
            if (gameManager != null)
                gameManager.PlayerDied();

            // Move the player back to the respawn position
            if (respawnPoint != null)
                other.transform.position = respawnPoint.position;

            // Reset vertical velocity to prevent residual fall speed after respawn
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = Vector3.zero;
        }
    }
}
