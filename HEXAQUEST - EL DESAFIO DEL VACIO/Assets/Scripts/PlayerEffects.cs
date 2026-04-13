using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles cinematic visual effects for the player, like fading out upon death.
/// Includes Physics Killswitches to prevent collision bugs.
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
        // Turn off Controls immediately
        if (pController != null) pController.enabled = false; 
        
        if (rb != null) 
        {
            // BUG FIX: ORDER OF OPERATIONS FOR UNITY 6
            // 1. First, stop all physical momentum while it's still a dynamic body
            rb.linearVelocity = Vector3.zero; 
            
            // 2. THEN, make it kinematic (ghost) so it ignores world collisions
            rb.isKinematic = true; 
            rb.useGravity = false;
        }
        
        if (col != null) col.enabled = false;

        StartCoroutine(FadeOutAndFallRoutine());
    }

    public void ResetVisuals()
    {
        for (int i = 0; i < characterRenderers.Length; i++)
        {
            if(characterRenderers[i].material.HasProperty("_Color") && i < originalColors.Count)
            {
                characterRenderers[i].material.color = originalColors[i];
            }
        }
        
        // Restore Physics and Controls upon respawn
        if (rb != null) 
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        if (col != null) col.enabled = true;
        if (pController != null) pController.enabled = true;
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