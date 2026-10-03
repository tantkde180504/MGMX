using System.Collections;
using UnityEngine;

namespace MMX.Core
{
    /// <summary>
    /// Hiệu ứng nhấp nháy sprite khi bị trúng đòn hoặc trong thời gian I-Frames.
    /// </summary>
    public class DamageFlash : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private float flashInterval = 0.08f;
        [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f, 0.6f);

        private Color originalColor;
        private HealthSystem healthSystem;
        private Coroutine flashCoroutine;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            if (targetRenderer != null)
            {
                originalColor = targetRenderer.color;
            }

            healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.OnDamaged += HandleDamaged;
            }
        }

        private void OnDestroy()
        {
            if (healthSystem != null)
            {
                healthSystem.OnDamaged -= HandleDamaged;
            }
        }

        private void HandleDamaged(int damage, Vector2 hitDirection)
        {
            if (targetRenderer == null) return;
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            while (healthSystem != null && healthSystem.IsInvulnerable && !healthSystem.IsDead)
            {
                if (targetRenderer != null)
                {
                    targetRenderer.color = flashColor;
                    yield return new WaitForSeconds(flashInterval);
                    targetRenderer.color = originalColor;
                    yield return new WaitForSeconds(flashInterval);
                }
                else
                {
                    yield break;
                }
            }

            if (targetRenderer != null)
            {
                targetRenderer.color = originalColor;
            }
        }
    }
}
