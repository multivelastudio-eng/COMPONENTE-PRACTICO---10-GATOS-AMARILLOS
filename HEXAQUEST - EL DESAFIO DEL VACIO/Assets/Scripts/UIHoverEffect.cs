using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// FASE 5 - Mejora de UI: Efecto visual de escala animada al pasar el cursor sobre un boton del menu.
/// Implementa IPointerEnterHandler y IPointerExitHandler de Unity para detectar el hover.
/// Aplica una animacion suave de escala (efecto "Pop") para dar feedback claro al usuario.
///
/// COMO USAR:
/// 1. Seleccionar cada boton del Menu Principal en la Jerarquia (JUGAR, OPCIONES, SALIR).
/// 2. Hacer clic en "Add Component" en el Inspector.
/// 3. Buscar y agregar "UIHoverEffect".
/// 4. Ajustar los valores de 'Hover Scale' y 'Animation Speed' si se desea.
/// </summary>
public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuracion del Efecto (Fase 5)")]
    [Tooltip("Factor de escala al que llega el boton cuando el cursor pasa por encima. 1.15 = 15% mas grande.")]
    public float hoverScale = 1.15f;

    [Tooltip("Velocidad de la animacion de escala. Valores mas altos = animacion mas rapida.")]
    public float animationSpeed = 10f;

    // Estado interno
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;

    void Awake()
    {
        // Guardar el tamano original del boton al iniciar
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    /// <summary>
    /// FASE 5: Se ejecuta cuando el cursor ENTRA al area del boton.
    /// Inicia la animacion de aumento de escala.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale;
        RestartScaleAnimation();

        // LOG DE CONSOLA: Evidencia del efecto hover para la guia
        Debug.Log("[HEXAQUEST - UIHoverEffect] HOVER ENTER en boton: " + gameObject.name + " -> Escala objetivo: " + hoverScale);
    }

    /// <summary>
    /// FASE 5: Se ejecuta cuando el cursor SALE del area del boton.
    /// Restaura la escala original del boton con animacion suave.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
        RestartScaleAnimation();

        // LOG DE CONSOLA: Evidencia del efecto hover para la guia
        Debug.Log("[HEXAQUEST - UIHoverEffect] HOVER EXIT en boton: " + gameObject.name + " -> Volviendo a escala original.");
    }

    /// <summary>
    /// Detiene cualquier animacion en curso e inicia una nueva para evitar conflictos.
    /// </summary>
    private void RestartScaleAnimation()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(AnimateScale());
    }

    /// <summary>
    /// Corrutina que anima suavemente la escala del boton desde su estado actual hasta 'targetScale'.
    /// Usa Lerp para un efecto de interpolacion suave y profesional.
    /// </summary>
    private IEnumerator AnimateScale()
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
            yield return null;
        }

        // Asegurar que llegue exactamente al valor final
        transform.localScale = targetScale;
    }
}
