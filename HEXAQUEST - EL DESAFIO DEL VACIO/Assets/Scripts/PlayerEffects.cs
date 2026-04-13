using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles cinematic visual effects for the player, like fading out upon death.
/// Manages kinematic state transitions to prevent physics warnings.
/// </summary>
public class PlayerEffects : MonoBehaviour
{
    private Renderer[] characterRenderers;
    private List<Color> originalColors = new List<Color>();
    
    private Rigidbody rb;
    private Collider col;
    private PlayerController pController;

    void Awake()
    {
        characterRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in characterRenderers)
        {
            if(rend.material.HasProperty("_Color"))
            {
                originalColors.Add(rend.material.color);
            }
        }
        
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        pController = GetComponent<PlayerController>();
    }

    public void StartFadeOutAndFall()
    {
        if (pController != null) pController.enabled = false; 
        
        if (rb != null) 
        {
            // First stop, then make kinematic
            rb.linearVelocity = Vector3.zero; 
            rb.isKinematic = true; 
            rb.useGravity = false;
        }
        
        if (col != null) col.enabled = false;

        StartCoroutine(FadeOutAndFallRoutine());
    }

    /// <summary>
    /// Restores the player to its physical state. 
    /// Must be called BEFORE attempting to set physics velocity.
    /// </summary>
    public void ResetVisuals()
    {
        StopAllCoroutines(); 
        
        // 1. Restore Physics State FIRST to avoid Unity warnings
        if (rb != null) 
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        
        if (col != null) col.enabled = true;
        if (pController != null) pController.enabled = true;

        // 2. Restore Visuals
        for (int i = 0; i < characterRenderers.Length; i++)
        {
            if(characterRenderers[i].material.HasProperty("_Color") && i < originalColors.Count)
            {
                characterRenderers[i].material.color = originalColors[i];
            }
        }
    }

    private IEnumerator FadeOutAndFallRoutine()
    {
        float duration = 1.0f; 
        float elapsed = 0f;
        Vector3 startFallPosition = transform.position;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            
            for (int i = 0; i < characterRenderers.Length; i++)
            {
                if(characterRenderers[i].material.HasProperty("_Color") && i < originalColors.Count)
                {
                    Color baseColor = originalColors[i];
                    characterRenderers[i].material.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                }
            }

            transform.position = Vector3.Lerp(startFallPosition, startFallPosition + (Vector3.down * 3f), elapsed / duration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}