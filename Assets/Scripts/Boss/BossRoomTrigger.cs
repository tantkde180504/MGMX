using System.Collections;
using UnityEngine;
using MMX.Camera;
using MMX.UI;

namespace MMX.Boss
{
    [RequireComponent(typeof(Collider2D))]
    public class BossRoomTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossController boss;
        [SerializeField] private BossDoor entryDoor;
        [SerializeField] private MMXCameraController stageCamera;
        [SerializeField] private HealthBarUI bossHealthBar;

        [Header("Room Camera Bounds")]
        [SerializeField] private Vector2 bossArenaCameraMin = new Vector2(50f, 3.5f);
        [SerializeField] private Vector2 bossArenaCameraMax = new Vector2(50f, 3.5f);

        private bool hasTriggered;

        public void Initialize(BossController b, BossDoor door, MMXCameraController cam, HealthBarUI bar, Vector2 camMin, Vector2 camMax)
        {
            boss = b;
            entryDoor = door;
            stageCamera = cam;
            bossHealthBar = bar;
            bossArenaCameraMin = camMin;
            bossArenaCameraMax = camMax;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasTriggered) return;

            if (other.CompareTag("Player"))
            {
                hasTriggered = true;
                StartCoroutine(EncounterRoutine());
            }
        }

        private IEnumerator EncounterRoutine()
        {
            // 1. Ðóng và khóa c?a ph?ng Boss ngay sau lýng ngý?i chõi
            if (entryDoor != null)
            {
                entryDoor.CloseAndLock();
            }

            // 2. Khóa Camera c? ð?nh vào ph?ng Boss
            if (stageCamera != null)
            {
                stageCamera.LockToBossRoom(bossArenaCameraMin, bossArenaCameraMax);
            }

            // Ch? 0.5s ?n ð?nh v? trí
            yield return new WaitForSeconds(0.5f);

            // 3. Ðánh th?c Boss
            if (boss != null)
            {
                boss.ActivateBoss();
            }

            // 4. B?t thanh máu Boss và ch?y hi?u ?ng n?p ð?y
            if (bossHealthBar != null && boss != null && boss.Health != null)
            {
                bossHealthBar.gameObject.SetActive(true);
                bossHealthBar.Initialize(boss.Health);
                bossHealthBar.PlayFillAnimation(boss.Health.MaxHealth);
            }
        }
    }
}
