using UnityEngine;

namespace MMX.Camera
{
    /// <summary>
    /// Camera Controller chuẩn phong cách Mega Man X4.
    /// Áp dụng các nguyên tắc tối ưu từ awesome-gamedev-agent-skills:
    /// - LateUpdate tracking độc lập với framerate (SmoothDamp).
    /// - Pixel-Perfect Grid Snapping (loại bỏ rung giật sub-pixel và hiện tượng mờ khi camera di chuyển).
    /// - Giới hạn biên màn chơi (Clamping to stage bounds & boss arena).
    /// </summary>
    public class MMXCameraController : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
        [SerializeField] private float smoothTime = 0.12f;

        [Header("Pixel Perfect Snapping (Khử mờ sub-pixel)")]
        [SerializeField] private bool snapToPixelGrid = true;
        [SerializeField] private float pixelsPerUnit = 32f;

        [Header("Stage Bounds (Giới hạn màn chơi)")]
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

            // Áp dụng giới hạn phòng Boss hoặc giới hạn Stage
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

            Vector3 nextPos = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

            // Tối ưu Pixel-Perfect: căn chỉnh vị trí camera về bội số của pixel để tránh làm mờ sprite pixel-art
            if (snapToPixelGrid && pixelsPerUnit > 0f)
            {
                float unitPerPixel = 1f / pixelsPerUnit;
                nextPos.x = Mathf.Round(nextPos.x / unitPerPixel) * unitPerPixel;
                nextPos.y = Mathf.Round(nextPos.y / unitPerPixel) * unitPerPixel;
            }

            transform.position = nextPos;
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
