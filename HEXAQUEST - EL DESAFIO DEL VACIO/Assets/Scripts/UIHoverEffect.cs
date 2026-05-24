using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// PHASE 5 - UI Polish: Animated scaling effect when hovering over a menu button.
/// Implements Unity's IPointerEnterHandler and IPointerExitHandler to detect hover events.
/// Applies a smooth scaling animation ("Pop" effect) to provide clear user feedback.
/// </summary>
public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Effect Configuration (Phase 5)")]
    [Tooltip("Scale factor reached when the cursor hovers over the button. 1.15 = 15% larger.")]
    public float hoverScale = 1.15f;

    [Tooltip("Speed of the scaling animation. Higher values = faster animation.")]
    public float animationSpeed = 10f;

    // Internal State
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;

    void Awake()
    {
        // Store the original button size on start
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    // ==========================================
    // BUG FIX: GHOST HOVER RESOLUTION
    // ==========================================
    /// <summary>
    /// Executed automatically by Unity when the GameObject is disabled (e.g., hiding a menu).
    /// Resets the scale instantly to prevent the button from being stuck in a "hovered" state.
    /// </summary>
    void OnDisable()
    {
        // 1. Instantly snap back to original scale
        transform.localScale = originalScale;
        targetScale = originalScale;
        
        // 2. Kill any running animations so it doesn't try to grow while invisible
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
    }

    // ==========================================
    // HOVER LOGIC
    // ==========================================

    /// <summary>
    /// PHASE 5: Executed when the cursor ENTERS the button area.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
        RestartScaleAnimation();

        // Optional log for rubric evidence
        // Debug.Log("[HEXAQUEST] Hover Enter on: " + gameObject.name);
    }

    /// <summary>
    /// PHASE 5: Executed when the cursor EXITS the button area.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        RestartScaleAnimation();
    }

    private void RestartScaleAnimation()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        
        // Safety check: Only start coroutines if the object is actually active
        if (gameObject.activeInHierarchy)
        {
            scaleCoroutine = StartCoroutine(AnimateScale());
        }
    }

    private IEnumerator AnimateScale()
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
            yield return null;
        }

        // Ensure it reaches the exact final value
        transform.localScale = targetScale;
    }
}