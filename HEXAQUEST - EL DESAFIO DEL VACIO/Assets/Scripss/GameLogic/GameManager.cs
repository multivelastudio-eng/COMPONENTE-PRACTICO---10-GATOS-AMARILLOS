using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Central manager for the HexaQuest game.
/// Controls the life system, win/loss conditions, and HUD updates.
/// 
/// Team: 3 Halcones Estrategicos - HexaQuest: El Desafio del Vacio
/// Course: Programacion para Videojuegos 213027A - UNAD
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Life System")]
    public int maxLives = 3;            // Total lives the player starts with
    private int currentLives;           // Current lives remaining during gameplay

    [Header("UI References")]
    public Text livesText;              // UI Text element showing the remaining lives
    public GameObject gameOverPanel;    // Panel displayed when all lives are lost
    public GameObject winPanel;         // Panel displayed when the player wins the level

    void Start()
    {
        // Set lives to maximum at the beginning of the game
        currentLives = maxLives;

        // Update the HUD display
        UpdateHUD();

        // Ensure end-game panels are hidden at the start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        // Make sure game is not paused from a previous run
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Called by KillZone when the player falls into the void.
    /// Subtracts a life and checks for game over condition.
    /// </summary>
    public void PlayerDied()
    {
        currentLives--;
        UpdateHUD();

        Debug.Log($"[GameManager] Lives remaining: {currentLives}");

        // Trigger game over if no more lives remain
        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    /// <summary>
    /// Called by GoalTrigger when the player completes the challenge.
    /// Shows the win panel and pauses the game.
    /// </summary>
    public void PlayerWon()
    {
        Debug.Log("[GameManager] Player won! Challenge complete.");

        if (winPanel != null)
            winPanel.SetActive(true);

        // Pause gameplay (time stops)
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Triggers the Game Over state.
    /// Shows the game-over panel and pauses the game.
    /// </summary>
    void GameOver()
    {
        Debug.Log("[GameManager] Game Over - No lives remaining.");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Pause gameplay (time stops)
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Restarts the current scene when the player clicks "Retry".
    /// Connected to the Retry button's OnClick() event.
    /// </summary>
    public void RestartGame()
    {
        // Resume time before reloading the scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Updates the HUD Text element with the current life count.
    /// </summary>
    void UpdateHUD()
    {
        if (livesText != null)
            livesText.text = $"Lives: {currentLives} / {maxLives}";
    }
}
