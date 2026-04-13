using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 
using UnityEngine.Events;

/// <summary>
/// Master Game Controller for "HEXAQUEST".
/// Handles cinematic sequences, gameplay loops, stabilized respawn logic, and UI score updates.
/// </summary>
public class GameManager : MonoBehaviour
{
    // --- 1. GAME SETTINGS ---
    [Header("Game Settings")]
    public float timeToChoose = 3f;
    public float timeDropped = 2f;
    public int maxLives = 3;

    // --- 2. WORLD REFERENCES ---
    [Header("World References")]
    public List<HexagonPlatform> allPlatforms;
    public Transform playerRespawnPoint;
    public GameObject player;

    // --- 3. INTRO SEQUENCE UI ---
    [Header("Intro Sequence UI")]
    public Animator introAnimator; 
    public Sprite readySprite;
    public Sprite reallySprite;
    public Sprite startSprite;
    private Image introImageDisplay; 

    // --- 4. MAIN HUD REFERENCES ---
    [Header("Main HUD References")]
    public GameObject hudContainer; 
    public TextMeshProUGUI instructionText; 
    public TextMeshProUGUI scoreText; // Only the NUMBER goes here
    public GameObject[] heartIcons; 
    public GameObject gameOverPanel; 
    public Image colorIndicator;

    // --- 5. VISUAL POLISH & IMPACT ---
    [Header("Visual Polish & Impact")]
    public Image screenFlashImage;
    public GameObject textShineOverlay;
    public CameraShaker mainCameraScript; 
    public AudioSource bgmSource;

    // --- 6. AUDIO & VFX EVENTS ---
    [Header("Events (Audio/VFX)")]
    public UnityEvent onIntroStart; 
    public UnityEvent onRoundStart;
    public UnityEvent onPlatformsDrop; 
    public UnityEvent onPlayerHitVoid; 

    // --- INTERNAL STATE VARIABLES ---
    private int currentScore = 0;
    private int currentLives;
    private bool playerFellThisRound = false; 

    void Start()
    {
        if (introAnimator != null) introImageDisplay = introAnimator.GetComponent<Image>();
        currentLives = maxLives;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (screenFlashImage != null) screenFlashImage.gameObject.SetActive(false);
        if (textShineOverlay != null) textShineOverlay.SetActive(false);
        if (bgmSource != null) bgmSource.Stop();
        
        StartCoroutine(GameSequenceRoutine());
    }

    private IEnumerator GameSequenceRoutine()
    {
        // ==========================================
        // STAGE 1: CINEMATIC INTRO
        // ==========================================
        if (hudContainer != null) hudContainer.SetActive(false);
        if (introAnimator != null) introAnimator.gameObject.SetActive(true);

        onIntroStart?.Invoke();

        if (introImageDisplay != null) introImageDisplay.sprite = readySprite;
        ExecuteTextImpact(false); 
        yield return new WaitForSeconds(1.2f);

        if (introImageDisplay != null) introImageDisplay.sprite = reallySprite;
        ExecuteTextImpact(false);
        yield return new WaitForSeconds(1.2f);

        if (introImageDisplay != null) introImageDisplay.sprite = startSprite;
        ExecuteTextImpact(true); 

        if (bgmSource != null) bgmSource.Play();
        yield return new WaitForSeconds(1.0f);

        if (introAnimator != null) introAnimator.gameObject.SetActive(false);
        if (hudContainer != null) hudContainer.SetActive(true);
        UpdateUI();

        // ==========================================
        // STAGE 2: MAIN GAME LOOP
        // ==========================================
        while (currentLives > 0)
        {
            // --- CLEAN START ---
            playerFellThisRound = false; 

            // EMERGENCY CHECK: If for some weird reason the player is disabled, bring them back.
            if (!player.activeInHierarchy)
            {
                RespawnPlayer();
                yield return new WaitForSeconds(0.5f);
            }

            // Pick a random safe color
            int randomColorIndex = Random.Range(0, System.Enum.GetValues(typeof(PlatformColor)).Length);
            PlatformColor safeColor = (PlatformColor)randomColorIndex;

            // UI Instruction
            if (instructionText != null) instructionText.text = "GO TO THIS COLOR!";
            if (colorIndicator != null) 
            {
                colorIndicator.color = GetRealColor(safeColor); 
                StartCoroutine(ColorIndicatorPopRoutine());
            }
            onRoundStart?.Invoke();

            // Wait Phase (Interruptible if player falls)
            float chooseTimer = timeToChoose;
            while (chooseTimer > 0 && !playerFellThisRound)
            {
                chooseTimer -= Time.deltaTime;
                yield return null;
            }

            // Drop Phase (Only if player didn't fall during the choice time)
            if (!playerFellThisRound)
            {
                if (instructionText != null) instructionText.text = "WATCH OUT!";
                if (colorIndicator != null) colorIndicator.color = Color.black; 
                onPlatformsDrop?.Invoke();
                
                foreach (HexagonPlatform platform in allPlatforms)
                {
                    if (platform.platformColor != safeColor) platform.Drop();
                }

                float dropTimer = timeDropped;
                while (dropTimer > 0 && !playerFellThisRound)
                {
                    dropTimer -= Time.deltaTime;
                    yield return null;
                }
            }

            // Recovery Phase (All platforms back up)
            foreach (HexagonPlatform platform in allPlatforms) platform.ResetPlatform();

            // --- FINAL RESULTS EVALUATION ---
            if (playerFellThisRound)
            {
                currentLives--; 
                UpdateUI();
                
                if (instructionText != null) instructionText.text = "LIFE LOST!";
                
                // Wait for the death fade animation to finish
                yield return new WaitForSeconds(1.5f); 

                if (currentLives > 0)
                {
                    RespawnPlayer();
                    if (instructionText != null) instructionText.text = "GET READY...";
                    yield return new WaitForSeconds(1.5f); // Preparation time
                }
            }
            else
            {
                currentScore++;
                UpdateUI();
                if (instructionText != null) instructionText.text = "ROUND PASSED!";
                yield return new WaitForSeconds(1.0f);
            }
        }

        GameOver();
    }

    // VISUAL HELPERS
    private void ExecuteTextImpact(bool isMega)
    {
        if (introAnimator != null) introAnimator.SetTrigger(isMega ? "DoMegaBoing" : "DoBoing");
        if (textShineOverlay != null) StartCoroutine(TextShineRoutine());
        if (isMega)
        {
            if (screenFlashImage != null) StartCoroutine(FlashRoutine());
            if (mainCameraScript != null) mainCameraScript.TriggerShake(0.5f, 0.7f);
        }
    }

    private IEnumerator ColorIndicatorPopRoutine()
    {
        if (colorIndicator == null) yield break;
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

    private Color GetRealColor(PlatformColor pc) { switch (pc) { case PlatformColor.Red: return Color.red; case PlatformColor.Blue: return Color.blue; case PlatformColor.Cyan: return Color.cyan; case PlatformColor.Yellow: return Color.yellow; case PlatformColor.Orange: return new Color(1f, 0.5f, 0f); case PlatformColor.Pink: return new Color(1f, 0.4f, 0.7f); case PlatformColor.Green: return Color.green; default: return Color.white; } }

    public void PlayerFell()
    {
        // VITAL FIX: Check if player is already marked as "fallen" to avoid multiple triggers
        if (!playerFellThisRound && currentLives > 0)
        {
            playerFellThisRound = true;
            onPlayerHitVoid?.Invoke();
            
            PlayerEffects effects = player.GetComponent<PlayerEffects>();
            if (effects != null) effects.StartFadeOutAndFall();
            else player.SetActive(false); 
        }
    }

    private void RespawnPlayer()
    {
        // 1. Set position
        player.transform.position = playerRespawnPoint.position;
        
        // 2. UNITY 6 FIX: Reset visuals and kinematic state BEFORE velocity
        PlayerEffects effects = player.GetComponent<PlayerEffects>();
        if (effects != null) effects.ResetVisuals(); 
        
        // 3. Reset Momentum now that the body is dynamic again
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null) playerRb.linearVelocity = Vector3.zero; 
        
        // 4. Reset internal controller state (fixes Ground Pound lock)
        PlayerController pController = player.GetComponent<PlayerController>();
        if (pController != null) pController.ResetState();
        
        // 5. Reactivate character
        player.SetActive(true);
    }

    private void UpdateUI() 
    { 
        // --- SCORE UPDATE FIX ---
        // Sends ONLY the number as text, allowing you to use a static "Puntos" label in the UI.
        if (scoreText != null) 
        {
            scoreText.text = currentScore.ToString(); 
        }

        // Heart lives update
        for (int i = 0; i < heartIcons.Length; i++) 
        {
            if (heartIcons[i] != null) heartIcons[i].SetActive(i < currentLives); 
        }
    }

    private void GameOver() 
    { 
        if (bgmSource != null) bgmSource.Stop(); 
        player.SetActive(false); 
        if (hudContainer != null) hudContainer.SetActive(false); 
        if (gameOverPanel != null) gameOverPanel.SetActive(true); 
    }
}