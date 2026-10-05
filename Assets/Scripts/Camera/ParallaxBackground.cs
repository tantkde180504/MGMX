using UnityEngine;

namespace MMX.Camera
{
    /// <summary>
    /// Hệ thống cuộn nền thị sai đa tầng (Multi-layer Parallax Scrolling) chuẩn Capcom Mega Man X4.
    /// Giúp mây trời, rặng núi xa và thác nước cuộn mượt mà theo chuyển động camera.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public string layerName;
            public Transform layerTransform;
            [Tooltip("Tỉ lệ thị sai trục X (0 = vô tận như bầu trời xa, 1 = bám sát tiền cảnh)")]
            public float parallaxFactorX = 0.1f;
            [Tooltip("Tỉ lệ thị sai trục Y")]
            public float parallaxFactorY = 0.05f;
            public bool autoScrollX = false;
            public float autoScrollSpeedX = 0.3f;
            public bool infiniteRepeatX = true;
            public float textureUnitSizeX = 22f;
            [HideInInspector] public Vector3 initialPosition;
        }

        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private ParallaxLayer[] layers;

        private Vector3 lastCameraPosition;

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
            }

            if (targetCamera != null)
            {
                lastCameraPosition = targetCamera.transform.position;
            }

            if (layers != null)
            {
                foreach (var layer in layers)
                {
                    if (layer.layerTransform != null)
                    {
                        layer.initialPosition = layer.layerTransform.position;
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
                if (targetCamera == null) return;
                lastCameraPosition = targetCamera.transform.position;
            }

            Vector3 deltaMovement = targetCamera.transform.position - lastCameraPosition;

            if (layers != null)
            {
                foreach (var layer in layers)
                {
                    if (layer.layerTransform == null) continue;

                    Vector3 pos = layer.layerTransform.position;

                    // Di chuyển theo camera với tỉ lệ parallax
                    pos.x += deltaMovement.x * (1f - layer.parallaxFactorX);
                    pos.y += deltaMovement.y * (1f - layer.parallaxFactorY);

                    // Cuộn tự động (mây trời lững lờ trôi)
                    if (layer.autoScrollX)
                    {
                        pos.x += layer.autoScrollSpeedX * Time.deltaTime;
                    }

                    // Tự động lặp lại vô tận (Infinite wrap around)
                    if (layer.infiniteRepeatX && layer.textureUnitSizeX > 0.5f)
                    {
                        float diffX = targetCamera.transform.position.x - pos.x;
                        if (Mathf.Abs(diffX) >= layer.textureUnitSizeX)
                        {
                            float offset = (diffX > 0 ? 1 : -1) * layer.textureUnitSizeX;
                            pos.x += offset;
                        }
                    }

                    layer.layerTransform.position = pos;
                }
            }

            lastCameraPosition = targetCamera.transform.position;
        }

        public void Initialize(UnityEngine.Camera cam, ParallaxLayer[] newLayers)
        {
            targetCamera = cam;
            layers = newLayers;
            if (targetCamera != null)
            {
                lastCameraPosition = targetCamera.transform.position;
            }
            if (layers != null)
            {
                foreach (var layer in layers)
                {
                    if (layer.layerTransform != null)
                    {
                        layer.initialPosition = layer.layerTransform.position;
                    }
                }
            }
        }
    }
}
