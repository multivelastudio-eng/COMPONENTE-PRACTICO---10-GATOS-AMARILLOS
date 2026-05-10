using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Handles all User Interface elements, animations, and visual polish.
/// Uses a Singleton pattern to be easily commanded by the GameManager.
/// </summary>
public class UIManager : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
    public static UIManager Instance { get; private set; }[Header("Intro Sequence UI")]
    public Animator introAnimator; 
    public Sprite readySprite;
    public Sprite reallySprite;
    public Sprite startSprite;
    private Image introImageDisplay; 

    [Header("Main HUD References")]
    public GameObject hudContainer; 
    public TextMeshProUGUI instructionText; 
    public TextMeshProUGUI scoreText; 
    public GameObject[] heartIcons; 
    public GameObject gameOverPanel; 
    public Image colorIndicator;

    [Header("Visual Polish & Impact")]
    public Image screenFlashImage;
    public GameObject textShineOverlay;

    void Awake()
    {
        // Singleton Setup: Ensures only one UIManager exists
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (introAnimator != null) introImageDisplay = introAnimator.GetComponent<Image>();
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

    public void ShowGameOverPanel()
    {
        if (hudContainer != null) hudContainer.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    // ==========================================
    // VISUAL POLISH COROUTINES
    // ==========================================

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