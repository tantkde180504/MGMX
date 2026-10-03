using UnityEngine;

namespace MMX.Camera
{
    public class MMXCameraController : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
        [SerializeField] private float smoothTime = 0.18f;

        [Header("Stage Bounds (Gi?i h?n màn chõi thý?ng)")]
        [SerializeField] private bool clampToStage = true;
        [SerializeField] private Vector2 stageMin = new Vector2(0f, -5f);
        [SerializeField] private Vector2 stageMax = new Vector2(220f, 25f);

        private Vector3 currentVelocity;
        private bool isLockedToBossRoom;
        private Vector2 bossRoomMin;
        private Vector2 bossRoomMax;

        private void Start()
        {
            if (target == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) target = p.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = target.position + offset;

            // Áp d?ng gi?i h?n ph?ng Boss ho?c gi?i h?n Stage
            if (isLockedToBossRoom)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, bossRoomMin.x, bossRoomMax.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, bossRoomMin.y, bossRoomMax.y);
            }
            else if (clampToStage)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, stageMin.x, stageMax.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, stageMin.y, stageMax.y);
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        }

        public void LockToBossRoom(Vector2 minBounds, Vector2 maxBounds)
        {
            isLockedToBossRoom = true;
            bossRoomMin = minBounds;
            bossRoomMax = maxBounds;
        }

        public void UnlockBossRoom()
        {
            isLockedToBossRoom = false;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetStageBounds(Vector2 min, Vector2 max)
        {
            stageMin = min;
            stageMax = max;
        }
    }
}
