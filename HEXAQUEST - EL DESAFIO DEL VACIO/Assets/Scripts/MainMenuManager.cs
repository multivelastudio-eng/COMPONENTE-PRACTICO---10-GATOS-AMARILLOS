using System.Collections; // Required for Coroutines
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

    [Header("UI Containers")][Tooltip("Drag the 'FondoMenu' object here.")]
    public GameObject mainMenuContainer;[Tooltip("Drag the 'PanelOpciones' object here.")]
    public GameObject optionsPanel;

    [Header("Audio Feedback")][Tooltip("AudioSource used for UI sound effects.")]
    public AudioSource sfxSource;[Tooltip("Sound triggered when hovering over a button.")]
    public AudioClip hoverSound;
    [Tooltip("Sound triggered when clicking a button.")]
    public AudioClip clickSound;

    void Start()
    {
        // INITIAL STATE: Show Menu, Hide Options
        if (mainMenuContainer != null) mainMenuContainer.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    // ==========================================
    // BUTTON ACTIONS (NAVIGATION & STATE)
    // ==========================================

    /// <summary>
    /// Loads the main game scene with a slight delay to allow audio to finish.
    /// Linked to the "JUGAR" button.
    /// </summary>
    public void StartGame()
    {
        PlayClickSound();
        Debug.Log("Loading gameplay...");
        
        // BUG FIX: Wait a fraction of a second so the sound finishes before the scene is destroyed
        StartCoroutine(WaitAndLoadScene(gameplaySceneName));
    }

    /// <summary>
    /// Coroutine to handle the delay before loading a new scene.
    /// </summary>
    private IEnumerator WaitAndLoadScene(string sceneName)
    {
        // Wait for 0.3 seconds to let the click sound play fully
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Hides the Main Menu and shows the Options Panel.
    /// Linked to the "OPCIONES" button.
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
    /// Linked to the "VOLVER" button.
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
    /// Closes the application with a slight delay for the audio.
    /// Linked to the "SALIR" button.
    /// </summary>
    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Quitting Application...");
        StartCoroutine(WaitAndQuit());
    }

    private IEnumerator WaitAndQuit()
    {
        yield return new WaitForSeconds(0.3f);
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
            // PlayOneShot allows multiple sounds to overlap without cutting each other
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