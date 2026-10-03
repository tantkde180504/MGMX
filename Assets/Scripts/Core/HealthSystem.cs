using System;
using UnityEngine;

namespace MMX.Core
{
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 32; // Thanh máu MMX chu?n thý?ng là 32 v?ch
        [SerializeField] private float invulnerabilityDuration = 1.2f;

        private int currentHealth;
        private float invulnerableTimer;
        private bool isInvulnerable;

        public event Action<int, int> OnHealthChanged; // (current, max)
        public event Action<int, Vector2> OnDamaged; // (damage, hitDirection)
        public event Action OnDeath;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0;
        public bool IsInvulnerable => isInvulnerable;

        private void Awake()
        {
            if (currentHealth <= 0) currentHealth = maxHealth;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Update()
        {
            if (isInvulnerable)
            {
                invulnerableTimer -= Time.deltaTime;
                if (invulnerableTimer <= 0f)
                {
                    isInvulnerable = false;
                }
            }
        }

        public void TakeDamage(int damage, Vector2 hitDirection, float knockbackForce = 0f)
        {
            if (IsDead || isInvulnerable) return;

            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnDamaged?.Invoke(damage, hitDirection);

            if (currentHealth > 0)
            {
                if (invulnerabilityDuration > 0f)
                {
                    TriggerInvulnerability(invulnerabilityDuration);
                }
            }
            else
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TriggerInvulnerability(float duration)
        {
            if (duration <= 0f) return;
            isInvulnerable = true;
            invulnerableTimer = duration;
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isInvulnerable = false;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void SetMaxHealth(int newMax, bool resetCurrent = true)
        {
            maxHealth = Mathf.Max(1, newMax);
            if (resetCurrent) currentHealth = maxHealth;
            else currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void SetInvulnerabilityDuration(float duration)
        {
            invulnerabilityDuration = Mathf.Max(0f, duration);
        }
    }
}
