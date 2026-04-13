using UnityEngine;

/// <summary>
/// Simple and efficient script to infinitely rotate background layers.
/// </summary>
public class BackgroundSpinner : MonoBehaviour
{[Tooltip("Speed of rotation. Use negative numbers to spin the other way.")]
    public float spinSpeed = 10f;[Tooltip("The axis to spin around. (0, 0, 1) is Z-axis, perfect for flat 2D images.")]
    public Vector3 spinAxis = new Vector3(0f, 0f, 1f);

    void Update()
    {
        // Rotate the object around its own axis smoothly regardless of framerate
        transform.Rotate(spinAxis * spinSpeed * Time.deltaTime, Space.Self);
    }
}