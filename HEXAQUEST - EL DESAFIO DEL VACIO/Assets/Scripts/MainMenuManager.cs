using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Master Controller for the Main Menu.
/// Handles scene navigation, UI state switching (Main Menu vs Options), 
/// and audio feedback for a professional User Experience.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The exact name of the gameplay scene to load.")]
    public string gameplaySceneName = "SampleScene";

    [Header("UI Containers")]
    [Tooltip("Drag the 'FondoMenu' object here.")]
    public GameObject mainMenuContainer; 
    [Tooltip("Drag the 'PanelOpciones' object here.")]
    public GameObject optionsPanel;

    [Header("Audio Feedback")]
    [Tooltip("AudioSource used for UI sound effects.")]
    public AudioSource sfxSource;
    [Tooltip("Sound triggered when hovering over a button.")]
    public AudioClip hoverSound;
    [Tooltip("Sound triggered when clicking a button.")]
    public AudioClip clickSound;

    void Start()
    {
        // INITIAL STATE: Main Menu active, Options hidden
        if (mainMenuContainer != null) mainMenuContainer.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    // ==========================================
    // BUTTON ACTIONS (NAVIGATION & STATE)
    // ==========================================

    /// <summary>
    /// Loads the main game scene.
    /// </summary>
    public void StartGame()
    {
        PlayClickSound();
        Debug.Log("Loading gameplay...");
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// Hides the Main Menu and shows the Options Panel.
    /// </summary>
    public void OpenOptions()
    {
        PlayClickSound();
        
        if (optionsPanel != null && mainMenuContainer != null) 
        {
            mainMenuContainer.SetActive(false); // Hide the main menu background and buttons
            optionsPanel.SetActive(true);       // Show the options/controls panel
        }
        else 
        {
            Debug.LogWarning("Missing UI references in the Inspector!");
        }
    }

    /// <summary>
    /// Hides the Options Panel and returns to the Main Menu.
    /// </summary>
    public void CloseOptions()
    {
        PlayClickSound();
        
        if (optionsPanel != null && mainMenuContainer != null) 
        {
            optionsPanel.SetActive(false);      // Hide options
            mainMenuContainer.SetActive(true);  // Show main menu again
        }
    }

    /// <summary>
    /// Closes the application.
    /// </summary>
    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Quitting Application...");
        Application.Quit();
    }

    // ==========================================
    // AUDIO LOGIC
    // ==========================================

    /// <summary>
    /// Plays the hover sound. Triggered by PointerEnter Event on buttons.
    /// </summary>
    public void PlayHoverSound()
    {
        if (sfxSource != null && hoverSound != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    /// <summary>
    /// Plays the click sound for button confirmation.
    /// </summary>
    private void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}