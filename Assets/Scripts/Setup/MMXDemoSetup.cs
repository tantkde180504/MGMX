using UnityEngine;
using UnityEngine.UI;
using MMX.Core;
using MMX.Player;
using MMX.Enemies;
using MMX.Boss;
using MMX.Camera;
using MMX.UI;

namespace MMX.Setup
{
    /// <summary>
    /// Script ti?n ích t? đ?ng xây d?ng toàn b? Màn chơi Web Spider (Jungle Stage) chu?n quy mô Mega Man X4 trong Unity.
    /// Có th? nh?n chu?t ph?i ch?n ""Build MMX4 Demo Scene"" trong Editor ho?c script s? t? ch?y khi b?t đ?u Play n?u scene tr?ng.
    /// </summary>
    public class MMXDemoSetup : MonoBehaviour
    {
        [Header("T? đ?ng xây d?ng khi b?t đ?u Play n?u chưa có g?")]
        [SerializeField] private bool autoBuildOnPlay = true;

        private void Start()
        {
            if (autoBuildOnPlay)
            {
                if (GameObject.FindGameObjectWithTag("Player") == null)
                {
                    BuildFullDemoStage();
                }
            }
        }

        [ContextMenu("Build MMX4 Demo Scene")]
        public void BuildFullDemoStage()
        {
            // 1. D?n d?p các đ?i tư?ng demo c? n?u có
            GameObject existingDemo = GameObject.Find("MMX_Demo_Environment");
            if (existingDemo != null) DestroyImmediate(existingDemo);

            GameObject root = new GameObject("MMX_Demo_Environment");

            // 2. T?o Camera chu?n màn h?nh cu?n MMX
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
            }
            mainCam.orthographic = true;
            mainCam.orthographicSize = 6.5f;
            // B?u tr?i đêm r?ng r?m công ngh? Web Spider (Dark Emerald Sky)
            mainCam.backgroundColor = new Color(0.04f, 0.11f, 0.07f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;

            MMXCameraController camController = mainCam.GetComponent<MMXCameraController>();
            if (camController == null) camController = mainCam.gameObject.AddComponent<MMXCameraController>();
            camController.SetStageBounds(new Vector2(0f, -4f), new Vector2(220f, 22f));

            // B?ng màu r?ng r?m Web Spider (Jungle Palette)
            Color barkColor = new Color(0.32f, 0.20f, 0.12f);       // Màu thân g? đ?i th?
            Color foliageColor = new Color(0.12f, 0.42f, 0.22f);    // Màu tán lá r?ng r?m
            Color platformWood = new Color(0.40f, 0.26f, 0.16f);    // Màu cành cây / b? đ?
            Color spiderLairColor = new Color(0.18f, 0.25f, 0.20f); // Màu hang ? nh?n

            // 3. XÂY D?NG TOÀN B? B?N Đ? R?NG R?M (5 KHU V?C)
            Transform terrainFolder = new GameObject("Terrain_Jungle").transform;
            terrainFolder.SetParent(root.transform);

            // =========================================================================
            // KHU V?C 1: B?A R?NG R?M & B?C THANG TÁN CÂY (X: -5 đ?n 45)
            // =========================================================================
            // Vách thân cây cao bên trái cùng đ? t?p leo tư?ng (Wall Kick)
            CreatePlatform(terrainFolder, "Trunk_Left_Boundary", new Vector2(-4.5f, 7f), new Vector2(2.5f, 20f), barkColor);
            // M?t đ?t b?a r?ng
            CreatePlatform(terrainFolder, "Ground_Section_1", new Vector2(20f, -2.5f), new Vector2(48f, 1.5f), foliageColor);
            // Các cành cây b? đ? cao d?n đ? làm quen nh?y & dash
            CreatePlatform(terrainFolder, "Branch_Step_1", new Vector2(10f, 0.5f), new Vector2(7f, 0.8f), platformWood);
            CreatePlatform(terrainFolder, "Branch_Step_2", new Vector2(21f, 3.0f), new Vector2(7f, 0.8f), platformWood);
            CreatePlatform(terrainFolder, "Branch_Step_3", new Vector2(33f, 1.2f), new Vector2(6f, 0.8f), platformWood);
            // Thân cây d?c gi?a đư?ng th? leo trèo
            CreatePlatform(terrainFolder, "Tree_Trunk_Mid_1", new Vector2(28f, 7.0f), new Vector2(1.8f, 9f), barkColor);

            // =========================================================================
            // KHU V?C 2: V?C SÂU & TÁN CÂY C? TH? TRÊN CAO (X: 45 đ?n 95)
            // Th? thách Dash-Jump qua h? t? th?n & bí m?t b?nh máu trên cao
            // =========================================================================
            // B? đ? vư?t v?c 1 (Đ?i h?i Dash-Jump đ? qua n?u không rơi xu?ng đáy)
            CreatePlatform(terrainFolder, "Canopy_Bridge_1", new Vector2(58f, 0.2f), new Vector2(11f, 1.0f), platformWood);
            // Đáy v?c (có r? cây bên dư?i n?u ngư?i chơi rơi xu?ng)
            CreatePlatform(terrainFolder, "Chasm_Pit_Floor", new Vector2(68f, -5.5f), new Vector2(35f, 1.5f), new Color(0.15f, 0.1f, 0.08f));
            // Tán cây c? th? t?ng cao nh?t (Gi?u b?nh h?i máu)
            CreatePlatform(terrainFolder, "High_Secret_Canopy", new Vector2(73f, 5.0f), new Vector2(12f, 1.0f), platformWood);
            // Cành cây ti?p n?i
            CreatePlatform(terrainFolder, "Canopy_Bridge_2", new Vector2(87f, 1.8f), new Vector2(9f, 1.0f), platformWood);

            // =========================================================================
            // KHU V?C 3: THÁP THÂN CÂY Đ?I TH? R?NG (X: 95 đ?n 135)
            // Đo?n leo tháp d?ng đ?ng kinh đi?n c?a Web Spider: đ?i h?i Wall Kick liên t?c!
            // =========================================================================
            // M?t đ?t trong l?ng cây
            CreatePlatform(terrainFolder, "Hollow_Tree_Floor", new Vector2(115f, -2.5f), new Vector2(38f, 1.5f), barkColor);
            // Vách thân cây bên trái (cao t?i Y = 22)
            CreatePlatform(terrainFolder, "Great_Trunk_Wall_Left", new Vector2(96f, 9.5f), new Vector2(2.5f, 24f), barkColor);
            // Vách thân cây bên ph?i (cao t?i Y = 22)
            CreatePlatform(terrainFolder, "Great_Trunk_Wall_Right", new Vector2(134f, 9.5f), new Vector2(2.5f, 24f), barkColor);
            // B?c thang r? cây zig-zag bên trong thân cây r?ng
            CreatePlatform(terrainFolder, "Trunk_Shelf_1", new Vector2(104f, 2.5f), new Vector2(9f, 0.8f), platformWood);
            CreatePlatform(terrainFolder, "Trunk_Shelf_2", new Vector2(126f, 6.8f), new Vector2(9f, 0.8f), platformWood);
            CreatePlatform(terrainFolder, "Trunk_Shelf_3", new Vector2(105f, 11.2f), new Vector2(9f, 0.8f), platformWood);
            CreatePlatform(terrainFolder, "Trunk_Shelf_4", new Vector2(125f, 15.5f), new Vector2(9f, 0.8f), platformWood);
            // C?u g? trên đ?nh ng?n cây thoát ra ngoài
            CreatePlatform(terrainFolder, "Tree_Crown_Exit_Bridge", new Vector2(115f, 19.5f), new Vector2(36f, 1.0f), foliageColor);

            // =========================================================================
            // KHU V?C 4: R?NG SÂU TRƯ?C C?NG TRÙM (X: 135 đ?n 168)
            // H? đ? cao t? ng?n cây xu?ng ti?n đ?n c?a ph?ng Boss
            // =========================================================================
            CreatePlatform(terrainFolder, "Descent_Branch_1", new Vector2(144f, 14.0f), new Vector2(11f, 1.0f), platformWood);
            CreatePlatform(terrainFolder, "Descent_Branch_2", new Vector2(154f, 6.5f), new Vector2(10f, 1.0f), platformWood);
            CreatePlatform(terrainFolder, "Outpost_Ground", new Vector2(162f, -2.5f), new Vector2(16f, 1.5f), foliageColor);
            // B?c tư?ng ngăn c?ng Boss
            CreatePlatform(terrainFolder, "Boss_Gate_Wall_Top", new Vector2(168f, 8.5f), new Vector2(2.5f, 11f), barkColor);
            CreatePlatform(terrainFolder, "Boss_Gate_Wall_Bottom", new Vector2(168f, -2.5f), new Vector2(2.5f, 1.5f), barkColor);

            // =========================================================================
            // KHU V?C 5: Đ?U TRƯ?NG TRÙM WEB SPIDER (X: 170 đ?n 218)
            // L?ng cây đ?i th? giăng tơ nh?n kh?ng l?, hai bên vách tư?ng g? cao đ? leo trèo
            // =========================================================================
            Transform bossArenaFolder = new GameObject("Boss_Arena_SpiderLair").transform;
            bossArenaFolder.SetParent(root.transform);
            CreatePlatform(bossArenaFolder, "Spider_Lair_Floor", new Vector2(193f, -2.5f), new Vector2(48f, 1.5f), spiderLairColor);
            CreatePlatform(bossArenaFolder, "Spider_Lair_Ceiling", new Vector2(193f, 15.5f), new Vector2(48f, 2.0f), barkColor);
            CreatePlatform(bossArenaFolder, "Spider_Lair_RightWall", new Vector2(217f, 6.5f), new Vector2(2.5f, 20f), barkColor);

            // Tơ nh?n trang trí trên tr?n đ?u trư?ng
            CreateWebDecoration(bossArenaFolder, new Vector3(180f, 14f, 0f), new Vector2(10f, 1.5f));
            CreateWebDecoration(bossArenaFolder, new Vector3(205f, 14f, 0f), new Vector2(10f, 1.5f));

            // =========================================================================
            // 4. C?A PH?NG BOSS (BOSS SHUTTER) T?I X = 168
            // =========================================================================
            GameObject doorObj = new GameObject("Boss_Shutter_Door");
            doorObj.transform.SetParent(root.transform);
            doorObj.transform.position = new Vector3(168f, 0.75f, 0f);

            GameObject doorMesh = new GameObject("Door_Sprite");
            doorMesh.transform.SetParent(doorObj.transform);
            doorMesh.transform.localPosition = Vector3.zero;
            SpriteRenderer doorSR = doorMesh.AddComponent<SpriteRenderer>();
            doorSR.sprite = CreateSimpleSprite(new Color(0.95f, 0.8f, 0.15f)); // C?a vàng MMX
            doorSR.color = new Color(0.95f, 0.8f, 0.15f);
            doorMesh.transform.localScale = new Vector3(0.7f, 4.5f, 1f);

            BoxCollider2D doorCol = doorObj.AddComponent<BoxCollider2D>();
            doorCol.size = new Vector2(0.7f, 4.5f);
            BossDoor bossDoor = doorObj.AddComponent<BossDoor>();

            // =========================================================================
            // 5. T?O PLAYER (MEGA MAN X)
            // =========================================================================
            GameObject player = new GameObject("Player_MegaManX");
            player.tag = "Player";
            player.transform.SetParent(root.transform);
            player.transform.position = new Vector3(0f, 0f, 0f);

            // 1. Thêm components physics, combat và controller cho Player trước
            Rigidbody2D pRb = player.AddComponent<Rigidbody2D>();
            BoxCollider2D pCol = player.AddComponent<BoxCollider2D>();
            pCol.size = new Vector2(0.85f, 1.6f);
            pCol.offset = new Vector2(0f, 0.8f);

            HealthSystem pHealth = player.AddComponent<HealthSystem>();
            DamageFlash pFlash = player.AddComponent<DamageFlash>();
            PlayerController2D pController = player.AddComponent<PlayerController2D>();
            PlayerCombat pCombat = player.AddComponent<PlayerCombat>();

            // 2. Tạo đối tượng hiển thị Visual chứa SpriteRenderer và PlayerSpriteAnimator
            GameObject pVisual = new GameObject("Visual");
            pVisual.transform.SetParent(player.transform);
            pVisual.transform.localPosition = Vector3.zero;
            pVisual.transform.localScale = Vector3.one;

            SpriteRenderer pSR = pVisual.AddComponent<SpriteRenderer>();
            Sprite xDefaultSprite = Resources.Load<Sprite>("Player/x_idle_0");
            if (xDefaultSprite != null)
            {
                pSR.sprite = xDefaultSprite;
            }
            else
            {
                pSR.sprite = CreateSimpleSprite(new Color(0.1f, 0.5f, 0.95f));
            }
            pSR.sortingOrder = 10;

            PlayerSpriteAnimator pAnim = pVisual.AddComponent<PlayerSpriteAnimator>();
            pAnim.LoadSpritesFromResourcesIfEmpty();

            camController.SetTarget(player.transform);

            // =========================================================================
            // 6. PHÂN B? K? Đ?CH R?NG R?M (ENEMIES)
            // =========================================================================
            Transform enemiesFolder = new GameObject("Enemies_Jungle").transform;
            enemiesFolder.SetParent(root.transform);

            // Quái b? trên m?t đ?t (Spike Crawlers)
            CreatePatrolEnemy(enemiesFolder, new Vector3(14f, -1.5f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(32f, -1.5f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(58f, 1.0f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(115f, -1.5f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(126f, 7.5f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(160f, -1.5f, 0f));

            // Quái ong bay lư?n trên cao (Flying Hornets)
            CreateFlyingHornet(enemiesFolder, new Vector3(22f, 4.5f, 0f));
            CreateFlyingHornet(enemiesFolder, new Vector3(52f, 3.5f, 0f)); // Bay ngay trên h? v?c
            CreateFlyingHornet(enemiesFolder, new Vector3(76f, 7.5f, 0f)); // Bay b?o v? b?nh h?i máu
            CreateFlyingHornet(enemiesFolder, new Vector3(112f, 13.0f, 0f)); // Trong thân cây đ?i th?
            CreateFlyingHornet(enemiesFolder, new Vector3(148f, 8.5f, 0f));

            // =========================================================================
            // 7. V?T PH?M H?I PH?C (HEALTH PICKUPS)
            // =========================================================================
            Transform itemsFolder = new GameObject("Pickups").transform;
            itemsFolder.SetParent(root.transform);
            CreateHealthCapsule(itemsFolder, new Vector3(73f, 6.2f, 0f)); // B?nh máu gi?u trên tán cây cao!
            CreateHealthCapsule(itemsFolder, new Vector3(105f, 12.4f, 0f)); // B?nh máu trong thân cây đ?i th?

            // =========================================================================
            // 8. T?O TRÙM WEB SPIDER (JUNGLE SOVEREIGN)
            // =========================================================================
            GameObject bossObj = new GameObject("Boss_WebSpider");
            bossObj.tag = "Boss";
            bossObj.transform.SetParent(bossArenaFolder);
            bossObj.transform.position = new Vector3(196f, 0f, 0f);

            GameObject bVisual = new GameObject("Visual");
            bVisual.transform.SetParent(bossObj.transform);
            bVisual.transform.localPosition = Vector3.zero;
            SpriteRenderer bSR = bVisual.AddComponent<SpriteRenderer>();
            // Giáp nh?n xanh l?c s?m sét pha tím đ?c trưng c?a Web Spider
            bSR.sprite = CreateSimpleSprite(new Color(0.12f, 0.58f, 0.35f));
            bVisual.transform.localScale = new Vector3(2.5f, 2.8f, 1f);

            // M?t nh?n phát sáng vàng r?c
            GameObject bEyes = new GameObject("Spider_Eyes");
            bEyes.transform.SetParent(bVisual.transform);
            bEyes.transform.localPosition = new Vector3(-0.25f, 0.2f, 0f);
            SpriteRenderer bEyesSR = bEyes.AddComponent<SpriteRenderer>();
            bEyesSR.sprite = CreateSimpleSprite(new Color(1f, 0.95f, 0.2f));
            bEyes.transform.localScale = new Vector3(0.4f, 0.3f, 1f);

            BoxCollider2D bCol = bossObj.AddComponent<BoxCollider2D>();
            bCol.size = new Vector2(2.4f, 2.7f);
            Rigidbody2D bRb = bossObj.AddComponent<Rigidbody2D>();

            HealthSystem bHealth = bossObj.AddComponent<HealthSystem>();
            DamageFlash bFlash = bossObj.AddComponent<DamageFlash>();
            BossController bossController = bossObj.AddComponent<BossController>();

            // =========================================================================
            // 9. T?O GIAO DI?N MÁU (HUD CANVAS)
            // =========================================================================
            GameObject canvasObj = new GameObject("MMX_HUD_Canvas");
            canvasObj.transform.SetParent(root.transform);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            HealthBarUI playerHealthBar = CreateHealthBarUI(canvasObj.transform, "Player_HealthBar", new Vector2(40f, -60f), new Vector2(0f, 1f), new Color(0.2f, 0.8f, 0.2f), pHealth);
            HealthBarUI bossHealthBar = CreateHealthBarUI(canvasObj.transform, "Boss_HealthBar", new Vector2(-40f, -60f), new Vector2(1f, 1f), new Color(0.2f, 0.9f, 0.5f), bHealth);
            bossHealthBar.gameObject.SetActive(false);

            // =========================================================================
            // 10. T?O BOSS ROOM TRIGGER (T?I X = 170)
            // =========================================================================
            GameObject triggerObj = new GameObject("Boss_Room_Trigger");
            triggerObj.transform.SetParent(root.transform);
            triggerObj.transform.position = new Vector3(170f, 0.5f, 0f);

            BoxCollider2D trigCol = triggerObj.AddComponent<BoxCollider2D>();
            trigCol.isTrigger = true;
            trigCol.size = new Vector2(2f, 5f);

            BossRoomTrigger roomTrigger = triggerObj.AddComponent<BossRoomTrigger>();
            // Khóa camera c? đ?nh vào đ?u trư?ng Web Spider t?i (193, 4.5)
            roomTrigger.Initialize(bossController, bossDoor, camController, bossHealthBar, new Vector2(193f, 4.5f), new Vector2(193f, 4.5f));

            Debug.Log("<color=green>[MMX4 Jungle Stage]</color> Đ? t?o thành công B?n đ? R?ng r?m & Trùm Web Spider hoàn ch?nh!");
        }

        private void CreatePlatform(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            GameObject plat = new GameObject(name);
            plat.transform.SetParent(parent);
            plat.transform.position = pos;

            SpriteRenderer sr = plat.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite(color);
            plat.transform.localScale = new Vector3(size.x, size.y, 1f);

            BoxCollider2D col = plat.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        private void CreateWebDecoration(Transform parent, Vector3 pos, Vector2 size)
        {
            GameObject web = new GameObject("Ceiling_SpiderWeb");
            web.transform.SetParent(parent);
            web.transform.position = pos;

            SpriteRenderer sr = web.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite(new Color(0.85f, 0.95f, 0.9f, 0.45f));
            sr.sortingOrder = 1;
            web.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private void CreatePatrolEnemy(Transform parent, Vector3 pos)
        {
            GameObject enemy = new GameObject("Enemy_SpikeCrawler");
            enemy.tag = "Enemy";
            enemy.transform.SetParent(parent);
            enemy.transform.position = pos;

            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite(new Color(0.75f, 0.25f, 0.2f)); // B? giáp đ? cam
            enemy.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            HealthSystem eHealth = enemy.AddComponent<HealthSystem>();
            eHealth.SetMaxHealth(3, true);
            eHealth.SetInvulnerabilityDuration(0f);
            enemy.AddComponent<DamageFlash>();
            enemy.AddComponent<BasicPatrolEnemy>();
        }

        private void CreateFlyingHornet(Transform parent, Vector3 pos)
        {
            GameObject hornet = new GameObject("Enemy_FlyingHornet");
            hornet.tag = "Enemy";
            hornet.transform.SetParent(parent);
            hornet.transform.position = pos;

            SpriteRenderer sr = hornet.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite(new Color(0.95f, 0.8f, 0.15f)); // Ong máy vàng s?c đen
            hornet.transform.localScale = new Vector3(1.1f, 0.9f, 1f);

            CircleCollider2D col = hornet.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            HealthSystem hHealth = hornet.AddComponent<HealthSystem>();
            hHealth.SetMaxHealth(2, true);
            hHealth.SetInvulnerabilityDuration(0f);
            hornet.AddComponent<DamageFlash>();
            hornet.AddComponent<FlyingHornetEnemy>();
        }

        private void CreateHealthCapsule(Transform parent, Vector3 pos)
        {
            GameObject capsule = new GameObject("Item_HealthCapsule");
            capsule.transform.SetParent(parent);
            capsule.transform.position = pos;

            SpriteRenderer sr = capsule.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite(new Color(0.2f, 0.95f, 0.3f)); // Viên năng lư?ng xanh l?c MMX
            capsule.transform.localScale = new Vector3(0.8f, 1.0f, 1f);

            BoxCollider2D col = capsule.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;

            capsule.AddComponent<HealthPickup>();
        }

        private HealthBarUI CreateHealthBarUI(Transform canvas, string name, Vector2 anchoredPos, Vector2 anchor, Color fillColor, HealthSystem health)
        {
            GameObject barRoot = new GameObject(name);
            barRoot.transform.SetParent(canvas, false);
            RectTransform rt = barRoot.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(24f, 180f);

            Image bg = barRoot.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(barRoot.transform, false);
            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = new Vector2(-4f, -4f);

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Vertical;
            fillImg.fillOrigin = (int)Image.OriginVertical.Bottom;
            fillImg.fillAmount = 1f;

            HealthBarUI barUI = barRoot.AddComponent<HealthBarUI>();
            var field = typeof(HealthBarUI).GetField("fillImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(barUI, fillImg);

            if (health != null) barUI.Initialize(health);

            return barUI;
        }

        private Sprite CreateSimpleSprite(Color color)
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
