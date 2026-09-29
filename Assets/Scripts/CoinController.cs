using UnityEngine;

public class CoinController : MonoBehaviour
{
    [Header("Particle Effect")]
    [SerializeField] private GameObject pickupParticlePrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.gameObject.SendMessage("ChangeTextCoin");

            // Spawn the particle at the coin's position
            if (pickupParticlePrefab != null)
            {
                GameObject particle = Instantiate(
                    pickupParticlePrefab,
                    transform.position,
                    Quaternion.identity
                );

                // Auto-destroy the particle after its duration
                // (adjust 1f to match your particle's longest lifetime)
                Destroy(particle, 1f);
            }

            Destroy(gameObject);
        }
    }
}