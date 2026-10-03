using UnityEngine;
using MMX.Core;

namespace MMX.Core
{
    [RequireComponent(typeof(Collider2D))]
    public class HealthPickup : MonoBehaviour
    {
        [Header("Heal Amount")]
        [SerializeField] private int healAmount = 8; // B?nh nãng lý?ng h?i 8 máu

        private bool hasPickedUp;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasPickedUp) return;

            if (other.CompareTag("Player"))
            {
                HealthSystem health = other.GetComponent<HealthSystem>();
                if (health != null && !health.IsDead)
                {
                    health.Heal(healAmount);
                    hasPickedUp = true;
                    Destroy(gameObject);
                }
            }
        }
    }
}
