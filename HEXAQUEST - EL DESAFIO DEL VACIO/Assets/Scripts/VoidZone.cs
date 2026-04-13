using UnityEngine;[RequireComponent(typeof(BoxCollider))]
public class VoidZone : MonoBehaviour
{[Tooltip("Reference to the GameManager to tell it we died.")]
    public GameManager gameManager;

    private void Start()
    {
        // Ensure the collider is set to trigger automatically
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that fell is the Player
        if (other.CompareTag("Player"))
        {
            gameManager.PlayerFell();
        }
    }
}