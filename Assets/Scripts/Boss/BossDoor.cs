using System.Collections;
using UnityEngine;

namespace MMX.Boss
{
    public class BossDoor : MonoBehaviour
    {
        [Header("Door Components")]
        [SerializeField] private Transform doorMeshOrSprite;
        [SerializeField] private Collider2D doorCollider;
        [SerializeField] private float openHeight = 4.5f;
        [SerializeField] private float slideSpeed = 7f;
        [SerializeField] private float approachDistance = 2.8f; // C? ly ti?p c?n ð? t? ð?ng m? c?a

        private Transform playerTransform;
        private Vector3 closedPosition;
        private Vector3 openPosition;
        private bool isOpen;
        private bool isLocked;

        private void Awake()
        {
            if (doorMeshOrSprite == null)
            {
                Transform child = transform.Find("Door_Sprite");
                doorMeshOrSprite = child != null ? child : transform;
            }

            if (doorCollider == null)
            {
                doorCollider = GetComponent<Collider2D>();
            }

            closedPosition = doorMeshOrSprite.localPosition;
            openPosition = closedPosition + Vector3.up * openHeight;
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            if (isLocked) return;

            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            // T? ð?ng m? c?a khi Player ti?n ð?n g?n m?t trý?c c?a
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            bool inFront = playerTransform.position.x <= transform.position.x + 0.3f;

            if (!isOpen && inFront && dist <= approachDistance)
            {
                OpenDoor();
            }
        }

        private void FindPlayer()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // D? ph?ng: N?u ch?m tr?c ti?p vào cánh c?a
            if (!isLocked && !isOpen && collision.gameObject.CompareTag("Player"))
            {
                OpenDoor();
            }
        }

        public void OpenDoor()
        {
            if (isLocked || isOpen) return;
            isOpen = true;

            // Chuy?n collider sang Trigger ngay l?p t?c ð? ngý?i chõi ði xuyên qua ðý?c
            if (doorCollider != null)
            {
                doorCollider.isTrigger = true;
            }

            StopAllCoroutines();
            StartCoroutine(SlideDoor(openPosition, false));
        }

        public void CloseAndLock()
        {
            isLocked = true;
            isOpen = false;

            StopAllCoroutines();
            StartCoroutine(SlideDoor(closedPosition, true));
        }

        private IEnumerator SlideDoor(Vector3 targetPos, bool lockOnFinish)
        {
            while (Vector3.Distance(doorMeshOrSprite.localPosition, targetPos) > 0.05f)
            {
                doorMeshOrSprite.localPosition = Vector3.MoveTowards(doorMeshOrSprite.localPosition, targetPos, slideSpeed * Time.deltaTime);
                yield return null;
            }
            doorMeshOrSprite.localPosition = targetPos;

            if (doorCollider != null)
            {
                if (lockOnFinish)
                {
                    doorCollider.isTrigger = false;
                    doorCollider.enabled = true;
                }
                else
                {
                    doorCollider.enabled = false;
                }
            }
        }
    }
}
