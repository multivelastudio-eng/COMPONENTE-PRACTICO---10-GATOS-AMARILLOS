using System.Collections;
using UnityEngine;

// Aquí definimos tus 7 colores en inglés.
public enum PlatformColor 
{ 
    Yellow,     // Hegaxono_Amarillo
    Blue,       // Hegaxono_Azul
    Cyan,       // Hegaxono_Central
    Orange,     // Hegaxono_Naranja
    Red,        // Hegaxono_Rojo
    Pink,       // Hegaxono_Rosa
    Green       // Hegaxono_Verde
}

public class HexagonPlatform : MonoBehaviour
{[Header("Platform Settings")]
    public PlatformColor platformColor; // El color de ESTA plataforma
    public float dropDistance = 15f;    // Qué tan abajo va a caer
    public float dropSpeed = 8f;        // Qué tan rápido cae

    private Vector3 originalPosition;
    private Vector3 targetPosition;

    void Start()
    {
        // Guardamos dónde está el hexágono al inicio del juego
        originalPosition = transform.position;
        targetPosition = originalPosition;
    }

    void Update()
    {
        // Esto mueve el hexágono suavemente hacia su destino (arriba o abajo)
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dropSpeed);
    }

    // El GameManager llama a esto para que el piso caiga
    public void Drop()
    {
        targetPosition = originalPosition + (Vector3.down * dropDistance);
    }

    // El GameManager llama a esto para que el piso regrese a la normalidad
    public void ResetPlatform()
    {
        targetPosition = originalPosition;
    }
}