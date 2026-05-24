using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Handles all User Interface elements, final score reporting, and audio feedback.
/// Uses a Singleton pattern to be easily commanded by the GameManager.
/// Includes an advanced, juicy "Heartbeat & Wobble" animation for New Records.
/// </summary>
public class UIManager : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
    public static UIManager Instance { get; private set; }

    // --- INTRO UI ---
    [Header("Intro Sequence UI")]
    public Animator introAnimator; 
    public Sprite readySprite;
    public Sprite reallySprite;
    public Sprite startSprite;
    private Image introImageDisplay; 

    // --- MAIN HUD ---
    [Header("Main HUD References")]
    public GameObject hudContainer; 
    public TextMeshProUGUI instructionText; 
    public TextMeshProUGUI scoreText; 
    public GameObject[] heartIcons; 
    public GameObject gameOverPanel; 
    
    [Header("Game Over UI (Phase 5 - Victory Condition)")]
    [Tooltip("Text to display the actual final score instead of X")]
    public TextMeshProUGUI finalScoreText; 
    [Tooltip("Text to display the all-time High Score")]
    public TextMeshProUGUI highScoreText; 
    [Tooltip("UI Element that activates ONLY when a new record is broken")]
    public GameObject newRecordAlert; 

    [Header("Main HUD Polish")]
    public Image colorIndicator;

    // --- POLISH & AUDIO ---
    [Header("Visual Polish & Audio")]
    public Image screenFlashImage;
    public GameObject textShineOverlay;
    
    [Tooltip("AudioSource used exclusively for UI sounds (hovers, clicks, game over)")]
    public AudioSource uiAudioSource; 
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip gameOverSound;

    void Awake()
    {
        // Singleton Setup: Ensures only one UIManager exists
        if (Instance == null) 
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }

        if (introAnimator != null) 
        {
            introImageDisplay = introAnimator.GetComponent<Image>();
        }
    }

    // ==========================================
    // INTRO SEQUENCE METHODS
    // ==========================================

    public void SetupIntro()
    {
        if (hudContainer != null) hudContainer.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (screenFlashImage != null) screenFlashImage.gameObject.SetActive(false);
        if (textShineOverlay != null) textShineOverlay.SetActive(false);
        
        if (introAnimator != null) introAnimator.gameObject.SetActive(true);
    }

    public void ShowReady() 
    { 
        if (introImageDisplay != null) introImageDisplay.sprite = readySprite; 
        ExecuteTextImpact(false); 
    }

    public void ShowReally() 
    { 
        if (introImageDisplay != null) introImageDisplay.sprite = reallySprite; 
        ExecuteTextImpact(false); 
    }

    public void ShowStart() 
    { 
        if (introImageDisplay != null) introImageDisplay.sprite = startSprite; 
        ExecuteTextImpact(true); 
    }

    public void EndIntroAndShowHUD()
    {
        if (introAnimator != null) introAnimator.gameObject.SetActive(false);
        if (hudContainer != null) hudContainer.SetActive(true);
    }

    // ==========================================
    // GAMEPLAY UI METHODS
    // ==========================================

    public void UpdateScore(int score) 
    { 
        if (scoreText != null) scoreText.text = score.ToString(); 
    }

    public void UpdateLives(int lives) 
    { 
        for (int i = 0; i < heartIcons.Length; i++) 
        {
            if (heartIcons[i] != null) heartIcons[i].SetActive(i < lives);
        }
    }

    public void SetInstruction(string message) 
    { 
        if (instructionText != null) instructionText.text = message; 
    }

    public void SetColorIndicator(Color safeColor) 
    { 
        if (colorIndicator != null) 
        { 
            colorIndicator.color = safeColor; 
            StartCoroutine(ColorIndicatorPopRoutine()); 
        } 
    }

    /// <summary>
    /// Shows the Game Over screen, plays defeat sound, and evaluates Win/Loss condition (Phase 5).
    /// </summary>
    public void ShowGameOverPanel(int finalScore, int highScore, bool isNewRecord)
    {
        if (hudContainer != null) hudContainer.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        
        // Update the final score text
        if (finalScoreText != null) 
        {
            finalScoreText.text = "Puntaje Final: " + finalScore.ToString();
        }

        // Update the High Score text
        if (highScoreText != null)
        {
            highScoreText.text = "Mejor Récord: " + highScore.ToString();
        }

        // Activate the "New Record!" alert with advanced animation if the player achieved a Victory
        if (newRecordAlert != null)
        {
            if (isNewRecord)
            {
                newRecordAlert.SetActive(true);
                StartCoroutine(AnimateNewRecordPop()); // Start the juicy animation
            }
            else
            {
                newRecordAlert.SetActive(false);
            }
        }

        // Play Game Over Sound
        if (uiAudioSource != null && gameOverSound != null) 
        {
            uiAudioSource.PlayOneShot(gameOverSound);
        }
    }

    // ==========================================
    // AUDIO METHODS
    // ==========================================

    public void PlayHoverSound() 
    { 
        if (uiAudioSource != null && hoverSound != null) 
        {
            uiAudioSource.PlayOneShot(hoverSound); 
        }
    }

    public void PlayClickSound() 
    { 
        if (uiAudioSource != null && clickSound != null) 
        {
            uiAudioSource.PlayOneShot(clickSound); 
        }
    }

    // ==========================================
    // VISUAL POLISH COROUTINES
    // ==========================================

    /// <summary>
    /// Creates a delayed bouncy pop-in, wobble, and heartbeat effect for the New Record text.
    /// </summary>
    private IEnumerator AnimateNewRecordPop()
    {
        // 0. Keep it completely invisible and rotated initially
        newRecordAlert.transform.localScale = Vector3.zero;
        newRecordAlert.transform.localRotation = Quaternion.Euler(0, 0, 15f); // Start slightly tilted

        // 1. THE ANTICIPATION DELAY: Let the player read the "Game Over" and score first
        yield return new WaitForSeconds(0.6f);

        // 2. EXPLOSIVE POP & WOBBLE: Pop up from zero to 120% while rotating
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f; // Speed of the pop
            
            // Wobble calculation
            float wobble = Mathf.Lerp(15f, -5f, t);
            newRecordAlert.transform.localRotation = Quaternion.Euler(0, 0, wobble);
            
            // Scale calculation
            newRecordAlert.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.2f, t);
            yield return null;
        }

        // 3. SETTLING: Smoothly return to 100% scale and straight rotation (0 degrees)
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            newRecordAlert.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(0, 0, -5f), Quaternion.identity, t);
            newRecordAlert.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, t);
            yield return null;
        }

        // Ensure perfect alignment at the end of the settling phase
        newRecordAlert.transform.localRotation = Quaternion.identity;

        // 4. INFINITE HEARTBEAT: Pulse slowly to keep drawing attention
        while (true)
        {
            float pulse = 1f + Mathf.PingPong(Time.time * 0.5f, 0.1f);
            newRecordAlert.transform.localScale = Vector3.one * pulse;
            yield return null;
        }
    }

    private void ExecuteTextImpact(bool isMega) 
    { 
        if (introAnimator != null) introAnimator.SetTrigger(isMega ? "DoMegaBoing" : "DoBoing"); 
        if (textShineOverlay != null) StartCoroutine(TextShineRoutine()); 
        if (isMega && screenFlashImage != null) StartCoroutine(FlashRoutine()); 
    }

    private IEnumerator ColorIndicatorPopRoutine() 
    { 
        colorIndicator.transform.localScale = Vector3.one * 1.3f; 
        yield return new WaitForSeconds(0.15f); 
        colorIndicator.transform.localScale = Vector3.one; 
    }

    private IEnumerator TextShineRoutine() 
    { 
        textShineOverlay.SetActive(true); 
        yield return new WaitForSeconds(0.15f); 
        textShineOverlay.SetActive(false); 
    }

    private IEnumerator FlashRoutine() 
    { 
        screenFlashImage.gameObject.SetActive(true); 
        float elapsed = 0f; 
        float duration = 0.4f; 
        
        while (elapsed < duration) 
        { 
            float alpha = Mathf.Lerp(0.8f, 0f, elapsed / duration); 
            screenFlashImage.color = new Color(1f, 1f, 1f, alpha); 
            elapsed += Time.deltaTime; 
            yield return null; 
        } 
        
        screenFlashImage.gameObject.SetActive(false); 
    }
}