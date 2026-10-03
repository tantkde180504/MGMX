using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MMX.Core;

namespace MMX.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image fillImage; // Image với ImageType = Filled, Fill Method = Vertical, Origin = Bottom
        [SerializeField] private Text labelText;

        [Header("Target")]
        [SerializeField] private HealthSystem targetHealth;

        private void Start()
        {
            if (targetHealth != null)
            {
                Initialize(targetHealth);
            }
        }

        public void Initialize(HealthSystem health)
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= UpdateHealthDisplay;
            }

            targetHealth = health;
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += UpdateHealthDisplay;
                UpdateHealthDisplay(targetHealth.CurrentHealth, targetHealth.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= UpdateHealthDisplay;
            }
        }

        private void UpdateHealthDisplay(int current, int max)
        {
            if (fillImage != null && max > 0)
            {
                fillImage.fillAmount = (float)current / max;
            }
        }

        /// <summary>
        /// Hiệu ứng nạp đầy thanh máu từ 0 đến đầy lúc Boss xuất hiện (đặc trưng của dòng Mega Man X).
        /// </summary>
        public void PlayFillAnimation(int max)
        {
            StartCoroutine(FillRoutine(max));
        }

        private IEnumerator FillRoutine(int max)
        {
            if (fillImage == null) yield break;

            fillImage.fillAmount = 0f;
            float step = 1f / Mathf.Max(1, max);

            for (int i = 1; i <= max; i++)
            {
                fillImage.fillAmount = i * step;
                yield return new WaitForSeconds(0.035f);
            }

            fillImage.fillAmount = 1f;
        }
    }
}
