using UnityEngine;

/// <summary>
/// Placed at the end of the hexagonal path.
/// When the player reaches this trigger, the game registers a victory.
/// 
/// Assigned to: Caren Vargas Vela
/// Team: 3 Halcones Estrategicos - HexaQuest: El Desafio del Vacio
/// Course: Programacion para Videojuegos 213027A - UNAD
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;     // Reference to the central game manager

    /// <summary>
    /// Called when the player's collider enters the goal zone.
    /// Notifies the GameManager to trigger the win state.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[GoalTrigger] Player completed the hexagon challenge!");

            if (gameManager != null)
                gameManager.PlayerWon();
        }
    }
}
