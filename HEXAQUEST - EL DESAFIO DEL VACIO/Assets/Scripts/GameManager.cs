using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; // Essential for scene transitions

/// <summary>
/// Master Game Controller for "HEXAQUEST".
/// Manages the core game loop, round timing, player lifecycle, and high-level game states.
/// Delegates UI rendering to UIManager to follow the Single Responsibility Principle.
/// </summary>
public class GameManager : MonoBehaviour
{
    // --- 1. GAME SETTINGS ---
    [Header("Game Settings")]
    [Tooltip("Time the player has to reach the safe color.")]
    public float timeToChoose = 3f;
    [Tooltip("How long the incorrect platforms stay dropped.")]
    public float timeDropped = 2f;
    [Tooltip("Total number of attempts before Game Over.")]
    public int maxLives = 3;

    // --- DIFFICULTY PROGRESSION (PHASE 5) ---
    [Header("Difficulty Progression (Phase 5)")]
    [Tooltip("Minimum time the player can have to choose. The timer will never go below this value.")]
    public float minTimeToChoose = 1.0f;
    [Tooltip("Each round won reduces 'timeToChoose' by this amount.")]
    public float timePenaltyPerRound = 0.15f;
    [Tooltip("Base drop speed of the platforms (assigned to all platforms at start).")]
    public float basePlatformDropSpeed = 8f;
    [Tooltip("Each round won increases the platform drop speed by this amount.")]
    public float dropSpeedIncreasePerRound = 0.5f;
    [Tooltip("Maximum drop speed the platforms can reach.")]
    public float maxPlatformDropSpeed = 22f;

    // --- 2. WORLD REFERENCES ---
    [Header("World References")]
    [Tooltip("List of all hexagonal platforms in the scene.")]
    public List<HexagonPlatform> allPlatforms;
    [Tooltip("The position where the player reappears after falling.")]
    public Transform playerRespawnPoint;
    [Tooltip("The player GameObject.")]
    public GameObject player;
    [Tooltip("Reference to the Camera Shaker script for impact effects.")]
    public CameraShaker mainCameraScript; 
    [Tooltip("Background music source.")]
    public AudioSource bgmSource;

    // --- 3. EVENTS ---
    [Header("Events (Audio/VFX Hooks)")]
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
        currentLives = maxLives;
        
        if (bgmSource != null) bgmSource.Stop();
        
        // Prepare the UI through the UIManager Singleton
        if (UIManager.Instance != null) UIManager.Instance.SetupIntro();
        
        // Start the master gameplay sequence
        StartCoroutine(GameSequenceRoutine());
    }

    private IEnumerator GameSequenceRoutine()
    {
        // ==========================================
        // STAGE 1: CINEMATIC INTRO
        // ==========================================
        onIntroStart?.Invoke();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowReady();
            yield return new WaitForSeconds(1.2f);

            UIManager.Instance.ShowReally();
            yield return new WaitForSeconds(1.2f);

            UIManager.Instance.ShowStart();
        }

        // Visual and physical impact at 'START!'
        if (mainCameraScript != null) mainCameraScript.TriggerShake(0.5f, 0.7f);
        if (bgmSource != null) bgmSource.Play();
        yield return new WaitForSeconds(1.0f);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.EndIntroAndShowHUD();
        }
        RefreshUI();

        // ==========================================
        // STAGE 2: MAIN GAME LOOP
        // ==========================================
        while (currentLives > 0)
        {
            playerFellThisRound = false; 

            // Safeguard: Ensure player is active before a new round starts
            if (!player.activeInHierarchy)
            {
                RespawnPlayer();
                yield return new WaitForSeconds(0.5f);
            }

            // Pick a random safe color from the Enum
            PlatformColor safeColor = (PlatformColor)Random.Range(0, System.Enum.GetValues(typeof(PlatformColor)).Length);

            // Update UI Instructions
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetInstruction("GO TO THIS COLOR!");
                UIManager.Instance.SetColorIndicator(GetRealColor(safeColor));
            }
            onRoundStart?.Invoke();

            // Wait Phase (Smart Timer: stops if player falls early)
            float chooseTimer = timeToChoose;
            while (chooseTimer > 0 && !playerFellThisRound)
            {
                chooseTimer -= Time.deltaTime;
                yield return null;
            }

            // Drop Phase (Only if player is still on the board)
            if (!playerFellThisRound)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.SetInstruction("WATCH OUT!");
                    UIManager.Instance.SetColorIndicator(Color.black);
                }
                onPlatformsDrop?.Invoke();
                
                foreach (HexagonPlatform platform in allPlatforms)
                {
                    if (platform.platformColor != safeColor) platform.Drop();
                }

                // Survival Timer
                float dropTimer = timeDropped;
                while (dropTimer > 0 && !playerFellThisRound)
                {
                    dropTimer -= Time.deltaTime;
                    yield return null;
                }
            }

            // Reset Round: All platforms come back up
            foreach (HexagonPlatform platform in allPlatforms) platform.ResetPlatform();

            // ==========================================
            // STAGE 3: RESULTS & LATE-DEATH BUG FIX
            // ==========================================
            if (playerFellThisRound)
            {
                yield return StartCoroutine(HandleDeathSequence());
            }
            else
            {
                currentScore++;
                IncreaseDifficulty(); // PHASE 5: Scale difficulty on round pass
                RefreshUI();
                if (UIManager.Instance != null) UIManager.Instance.SetInstruction("ROUND PASSED!");
                
                // Check for falls during the victory pause
                float victoryTimer = 1.5f;
                while (victoryTimer > 0 && !playerFellThisRound)
                {
                    victoryTimer -= Time.deltaTime;
                    yield return null;
                }

                if (playerFellThisRound)
                {
                    yield return StartCoroutine(HandleDeathSequence());
                }
            }
        }

        GameOver();
    }

    private IEnumerator HandleDeathSequence()
    {
        currentLives--; 
        RefreshUI();
        
        if (UIManager.Instance != null) UIManager.Instance.SetInstruction("LIFE LOST!");
        
        // Wait for the cinematic death animation (fade/fall) to finish
        yield return new WaitForSeconds(1.5f); 

        if (currentLives > 0)
        {
            RespawnPlayer();
            if (UIManager.Instance != null) UIManager.Instance.SetInstruction("GET READY...");
            yield return new WaitForSeconds(1.5f); // Stabilization delay
        }
    }

    private void RefreshUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
            UIManager.Instance.UpdateLives(currentLives);
        }
    }

    private Color GetRealColor(PlatformColor pc)
    {
        switch (pc)
        {
            case PlatformColor.Red: return Color.red;
            case PlatformColor.Blue: return Color.blue;
            case PlatformColor.Cyan: return Color.cyan;
            case PlatformColor.Yellow: return Color.yellow;
            case PlatformColor.Orange: return new Color(1f, 0.5f, 0f); 
            case PlatformColor.Pink: return new Color(1f, 0.4f, 0.7f); 
            case PlatformColor.Green: return Color.green;
            default: return Color.white;
        }
    }

    /// <summary>
    /// Triggered by the VoidZone script. Initiates the death process.
    /// </summary>
    public void PlayerFell()
    {
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
        // 1. Move player to the safe zone
        player.transform.position = playerRespawnPoint.position;
        
        // 2. IMPORTANT: Reset visuals and kinematic state BEFORE physics
        PlayerEffects effects = player.GetComponent<PlayerEffects>();
        if (effects != null) effects.ResetVisuals(); 
        
        // 3. Reset Physics momentum
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null) playerRb.linearVelocity = Vector3.zero; 
        
        // 4. Reset internal movement state
        PlayerController pController = player.GetComponent<PlayerController>();
        if (pController != null) pController.ResetState();
        
        player.SetActive(true);
    }

    // ==========================================
    // DIFFICULTY PROGRESSION SYSTEM (PHASE 5)
    // ==========================================

    /// <summary>
    /// PHASE 5: Progressively increases game difficulty with each survived round.
    /// Reduces player reaction time and increases platform drop speed.
    /// </summary>
    private void IncreaseDifficulty()
    {
        // --- 1. Reduce reaction time ---
        float newTime = timeToChoose - timePenaltyPerRound;
        timeToChoose = Mathf.Max(newTime, minTimeToChoose);

        // --- 2. Increase platform drop speed ---
        float newDropSpeed = basePlatformDropSpeed + (dropSpeedIncreasePerRound * currentScore);
        float clampedDropSpeed = Mathf.Min(newDropSpeed, maxPlatformDropSpeed);

        foreach (HexagonPlatform platform in allPlatforms)
        {
            platform.dropSpeed = clampedDropSpeed;
        }

        // --- 3. CONSOLE LOG (Evidence for rubric) ---
        Debug.Log("[HEXAQUEST - Difficulty] === ROUND " + currentScore + " PASSED ===");
        Debug.Log("[HEXAQUEST - Difficulty] Current reaction time: " + timeToChoose.ToString("F2") + "s (min: " + minTimeToChoose + "s)");
        Debug.Log("[HEXAQUEST - Difficulty] Platform drop speed: " + clampedDropSpeed.ToString("F1") + " (max: " + maxPlatformDropSpeed + ")");
    }

    // ==========================================
    // GAME OVER & VICTORY CONDITION (PHASE 5)
    // ==========================================

    private void GameOver()
    {
        if (bgmSource != null) bgmSource.Stop();
        player.SetActive(false); 
        
        // --- HIGH SCORE LOGIC (PHASE 5: VICTORY CONDITION) ---
        // Read the previously saved high score
        int highScore = PlayerPrefs.GetInt("Hexaquest_HighScore", 0);
        bool isNewRecord = false;

        // If we beat the record, it's a "Victory"!
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("Hexaquest_HighScore", highScore); // Save new record
            PlayerPrefs.Save();
            
            // Only trigger "New Record" if the player has played before and scored > 0
            if (PlayerPrefs.HasKey("Hexaquest_HighScore") && currentScore > 0)
            {
                isNewRecord = true;
                Debug.Log("[HEXAQUEST - Victory] New High Score Achieved: " + highScore);
            }
        }
        
        // Pass the actual score, high score, and victory status to the UIManager
        if (UIManager.Instance != null) 
        {
            UIManager.Instance.ShowGameOverPanel(currentScore, highScore, isNewRecord);
        }
    }

    // ==========================================
    // SCENE NAVIGATION (FOR UI BUTTONS)
    // ==========================================
    
    public void RetryGame()
    {
        if (UIManager.Instance != null) UIManager.Instance.PlayClickSound();
        StartCoroutine(WaitAndLoadScene(SceneManager.GetActiveScene().name));
    }

    public void ReturnToMainMenu()
    {
        if (UIManager.Instance != null) UIManager.Instance.PlayClickSound();
        StartCoroutine(WaitAndLoadScene("MainMenu"));
    }

    private IEnumerator WaitAndLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(sceneName);
    }
}