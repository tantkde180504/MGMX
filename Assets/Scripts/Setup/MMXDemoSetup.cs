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
    /// Xây dựng hoàn chỉnh bản đồ Web Spider (Jungle Stage - Mega Man X4) trực tiếp từ bộ dữ liệu trích xuất gốc (spriters-resource.com).
    /// Loại bỏ hoàn toàn map demo hộp chữ nhật cũ, sử dụng 100% hình ảnh map gốc kết hợp hệ thống vật lý và Parallax đa tầng.
    /// </summary>
    public class MMXDemoSetup : MonoBehaviour
    {
        [Header("Tự động xây dựng khi bắt đầu Play nếu chưa có gì")]
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

        [ContextMenu("Build Authentic Web Spider Stage")]
        public void BuildFullDemoStage()
        {
            // 1. Dọn dẹp môi trường demo cũ
            GameObject existingDemo = GameObject.Find("MMX_Demo_Environment");
            if (existingDemo != null) DestroyImmediate(existingDemo);

            GameObject root = new GameObject("MMX_Demo_Environment");

            // 2. Tải toàn bộ 5 phần bản đồ gốc (Authentic Stage Tracks từ file Spriters Resource HD)
            Sprite sPart1Canopy = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part1_Canopy");
            Sprite sPart2Trunk = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part2_HollowTrunk");
            Sprite sPart3River = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part3_WaterfallRiver");
            Sprite sPart4Corridor = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part4_BossCorridor");
            Sprite sPart5Arena = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part5_BossArena");

            // Tải Backgrounds & Chi tiết trang trí
            Sprite sSky = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Sky_Clouds");
            Sprite sMountains = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Distant_Mountains");
            Sprite sWaterfall = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Waterfall_Background");
            Sprite sWaterCliff = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Waterfall_Cliff");
            Sprite sDarkTrunk = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Dark_Trunk_Backdrop");

            Sprite sGate = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Boss_Gate_Shutter");
            Sprite sWeb = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Spider_Web");
            Sprite sVines = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Hanging_Vines");

            // 3. Cấu hình Camera chuẩn MMX4
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
            }
            mainCam.orthographic = true;
            mainCam.orthographicSize = 4.5f; // Tỉ lệ hiển thị chuẩn pixel-perfect MMX4
            mainCam.backgroundColor = new Color(0.18f, 0.36f, 0.48f); // Bầu trời rừng nhiệt đới
            mainCam.clearFlags = CameraClearFlags.SolidColor;

            MMXCameraController camController = mainCam.GetComponent<MMXCameraController>();
            if (camController == null) camController = mainCam.gameObject.AddComponent<MMXCameraController>();
            // Giới hạn camera bao trùm toàn bộ chiều dài (0 đến 325) và độ sâu (-50 đến 48)
            camController.SetStageBounds(new Vector2(0f, -48f), new Vector2(325f, 48f));

            // =========================================================================
            // 4. HỆ THỐNG PARALLAX SCROLLING ĐA TẦNG (SPRITERS RESOURCE)
            // =========================================================================
            GameObject parallaxRoot = new GameObject("MMX_Parallax_Background");
            parallaxRoot.transform.SetParent(root.transform);
            ParallaxBackground parallax = parallaxRoot.AddComponent<ParallaxBackground>();

            // Tầng 1: Bầu trời & Mây xa (Sky & Clouds) - Sorting Order: -30
            Transform skyFolder = new GameObject("Layer_1_Sky").transform;
            skyFolder.SetParent(parallaxRoot.transform);
            BuildRepeatingBackground(skyFolder, sSky, new Color(0.35f, 0.60f, 0.85f), -30, new Vector2(-10f, 38f), 180f, 11.25f);

            // Tầng 2: Rặng núi xa (Distant Mountains) - Sorting Order: -25
            Transform mountainFolder = new GameObject("Layer_2_Mountains").transform;
            mountainFolder.SetParent(parallaxRoot.transform);
            BuildRepeatingBackground(mountainFolder, sMountains, new Color(0.15f, 0.38f, 0.28f), -25, new Vector2(-10f, 24f), 180f, 11.25f);

            // Tầng 3: Đại thung lũng thác nước cuộn chảy (Waterfall Gorge) - Sorting Order: -20
            Transform waterfallFolder = new GameObject("Layer_3_Waterfalls").transform;
            waterfallFolder.SetParent(parallaxRoot.transform);
            BuildWaterfallBackdrop(waterfallFolder, sWaterfall, sWaterCliff, new Vector2(218f, -25f));

            // Tầng 4: Vách gỗ lòng cây đại thụ & Hang nhện (Dark Interior) - Sorting Order: -15
            Transform interiorFolder = new GameObject("Layer_4_DarkInterior").transform;
            interiorFolder.SetParent(parallaxRoot.transform);
            BuildInteriorBackdrop(interiorFolder, sDarkTrunk, new Vector2(170.4f, -23f), new Vector2(25f, 46f));
            BuildInteriorBackdrop(interiorFolder, sDarkTrunk, new Vector2(310.2f, -40.5f), new Vector2(22f, 11f));

            ParallaxBackground.ParallaxLayer[] pLayers = new ParallaxBackground.ParallaxLayer[]
            {
                new ParallaxBackground.ParallaxLayer
                {
                    layerName = "Sky",
                    layerTransform = skyFolder,
                    parallaxFactorX = 0.06f,
                    parallaxFactorY = 0.03f,
                    autoScrollX = true,
                    autoScrollSpeedX = 0.20f,
                    infiniteRepeatX = true,
                    textureUnitSizeX = 22.5f
                },
                new ParallaxBackground.ParallaxLayer
                {
                    layerName = "Mountains",
                    layerTransform = mountainFolder,
                    parallaxFactorX = 0.20f,
                    parallaxFactorY = 0.08f,
                    autoScrollX = false,
                    infiniteRepeatX = true,
                    textureUnitSizeX = 22.5f
                },
                new ParallaxBackground.ParallaxLayer
                {
                    layerName = "Waterfalls",
                    layerTransform = waterfallFolder,
                    parallaxFactorX = 0.45f,
                    parallaxFactorY = 0.20f,
                    autoScrollX = false,
                    infiniteRepeatX = false
                }
            };
            parallax.Initialize(mainCam, pLayers);

            // =========================================================================
            // 5. TÁI TẠO BẢN ĐỒ WEB SPIDER GỐC (5 KHU VỰC THỰC TẾ - VISUAL TRACKS & COLLIDERS)
            // =========================================================================
            Transform stageFolder = new GameObject("Authentic_WebSpider_Stage").transform;
            stageFolder.SetParent(root.transform);

            // -------------------------------------------------------------------------
            // PHẦN 1: RỪNG TÁN CÂY TRÊN CAO (CANOPY RUN: X = 0 đến 158.25, Y = 0 đến 45.94)
            // -------------------------------------------------------------------------
            GameObject part1Obj = CreateStageTrackVisual(stageFolder, "Track_Part1_Canopy", sPart1Canopy, new Vector3(79.125f, 22.97f, 0f), new Vector2(158.25f, 45.94f));
            Transform p1Col = new GameObject("Colliders_Part1").transform;
            p1Col.SetParent(part1Obj.transform);

            // Vách tường chắn bên trái cùng ngăn rớt lùi
            CreateInvisibleCollider(p1Col, "Wall_Left_Limit", new Vector2(-1.5f, 25f), new Vector2(3f, 40f));
            // Mặt đất xuất phát (Mossy Start Floor)
            CreateInvisibleCollider(p1Col, "Canopy_Start_Floor", new Vector2(15f, 21.5f), new Vector2(32f, 2f));
            // Các cành cây bệ đỡ cao vút và thân gỗ ngã
            CreateInvisibleCollider(p1Col, "Canopy_Branch_1", new Vector2(38f, 24.5f), new Vector2(12f, 1.2f));
            CreateInvisibleCollider(p1Col, "Canopy_Branch_2", new Vector2(56f, 28.0f), new Vector2(14f, 1.2f));
            CreateInvisibleCollider(p1Col, "Canopy_Branch_3", new Vector2(76f, 23.5f), new Vector2(16f, 1.2f));
            CreateInvisibleCollider(p1Col, "Canopy_Bridge_Chasm", new Vector2(98f, 19.0f), new Vector2(20f, 1.5f));
            // Tán cây cổ thụ giấu bí mật trên cao
            CreateInvisibleCollider(p1Col, "Canopy_High_Secret", new Vector2(112f, 32.0f), new Vector2(14f, 1.2f));
            // Đoạn đường dẫn vào miệng thân cây rỗng
            CreateInvisibleCollider(p1Col, "Canopy_Trunk_Approach", new Vector2(138f, 16.5f), new Vector2(36f, 2f));
            // Vách thân cây bao quanh lối vào
            CreateInvisibleCollider(p1Col, "Trunk_Entrance_Ceiling", new Vector2(150f, 30f), new Vector2(18f, 4f));

            // Dây leo tiền cảnh (Foreground Vines)
            CreateVinesDecoration(stageFolder, sVines, new Vector3(12f, 26f, 0f), new Vector2(3.5f, 3.5f));
            CreateVinesDecoration(stageFolder, sVines, new Vector3(45f, 31f, 0f), new Vector2(4f, 4f));
            CreateVinesDecoration(stageFolder, sVines, new Vector3(88f, 27f, 0f), new Vector2(3.5f, 3.5f));
            CreateVinesDecoration(stageFolder, sVines, new Vector3(130f, 22f, 0f), new Vector2(4f, 4f));

            // -------------------------------------------------------------------------
            // PHẦN 2: THÂN CÂY ĐẠI THỤ THẲNG ĐỨNG (HOLLOW TRUNK DESCENT: X = 158.25 đến 182.63, Y = -45.94 đến 0)
            // -------------------------------------------------------------------------
            GameObject part2Obj = CreateStageTrackVisual(stageFolder, "Track_Part2_HollowTrunk", sPart2Trunk, new Vector3(170.44f, -22.97f, 0f), new Vector2(24.38f, 45.94f));
            Transform p2Col = new GameObject("Colliders_Part2").transform;
            p2Col.SetParent(part2Obj.transform);

            // Vách trái thân cây đại thụ (cao từ Y = -46 đến 0) để Wall Slide & Wall Kick
            CreateInvisibleCollider(p2Col, "Trunk_Left_Wall", new Vector2(158.8f, -23f), new Vector2(2f, 46f));
            // Vách phải thân cây đại thụ (cao từ Y = -46 đến 0) để Wall Slide & Wall Kick
            CreateInvisibleCollider(p2Col, "Trunk_Right_Wall", new Vector2(182.0f, -23f), new Vector2(2f, 46f));

            // Các bệ nấm gỗ & cành cây bên trong lòng cây theo bậc zig-zag
            CreateInvisibleCollider(p2Col, "Trunk_Shelf_1", new Vector2(165f, -6.5f), new Vector2(10f, 1f));
            CreateInvisibleCollider(p2Col, "Trunk_Shelf_2", new Vector2(175f, -15.5f), new Vector2(10f, 1f));
            CreateInvisibleCollider(p2Col, "Trunk_Shelf_3", new Vector2(165f, -24.5f), new Vector2(10f, 1f));
            CreateInvisibleCollider(p2Col, "Trunk_Shelf_4", new Vector2(175f, -33.5f), new Vector2(10f, 1f));
            CreateInvisibleCollider(p2Col, "Trunk_Shelf_Bottom", new Vector2(168f, -42.5f), new Vector2(14f, 1.2f));

            // -------------------------------------------------------------------------
            // PHẦN 3: VỰC SÂU THÁC NƯỚC & LÒNG SÔNG ĐÁ (WATERFALL RIVER: X = 182.63 đến 252.94, Y = -45.94 đến -3.94)
            // -------------------------------------------------------------------------
            GameObject part3Obj = CreateStageTrackVisual(stageFolder, "Track_Part3_WaterfallRiver", sPart3River, new Vector3(217.78f, -24.94f, 0f), new Vector2(70.31f, 42.0f));
            Transform p3Col = new GameObject("Colliders_Part3").transform;
            p3Col.SetParent(part3Obj.transform);

            // Bờ đá thác nước lối ra khỏi thân cây
            CreateInvisibleCollider(p3Col, "River_Exit_Bank", new Vector2(188f, -44f), new Vector2(12f, 2f));
            // Các tảng đá bệ đỡ vượt dòng thác cuộn
            CreateInvisibleCollider(p3Col, "River_Rock_Step_1", new Vector2(202f, -42.5f), new Vector2(8f, 1.5f));
            CreateInvisibleCollider(p3Col, "River_Rock_Step_2", new Vector2(216f, -39.0f), new Vector2(9f, 1.5f));
            CreateInvisibleCollider(p3Col, "River_Rock_Step_3", new Vector2(230f, -41.5f), new Vector2(9f, 1.5f));
            CreateInvisibleCollider(p3Col, "River_Far_Bank", new Vector2(246f, -43.5f), new Vector2(16f, 2f));

            // Đáy vực nước (Hazard/Respawn)
            CreateInvisibleCollider(p3Col, "River_Water_Bed", new Vector2(218f, -48f), new Vector2(68f, 2f));

            // -------------------------------------------------------------------------
            // PHẦN 4: HÀNH LANG CỔNG TRÙM (BOSS CORRIDOR: X = 252.94 đến 300, Y = -45.94 đến -37.69)
            // -------------------------------------------------------------------------
            GameObject part4Obj = CreateStageTrackVisual(stageFolder, "Track_Part4_BossCorridor", sPart4Corridor, new Vector3(276.5f, -41.81f, 0f), new Vector2(47.1f, 8.25f));
            Transform p4Col = new GameObject("Colliders_Part4").transform;
            p4Col.SetParent(part4Obj.transform);

            // Mặt đất bằng phẳng dẫn đến cửa Boss
            CreateInvisibleCollider(p4Col, "Boss_Approach_Floor", new Vector2(276.5f, -44.5f), new Vector2(48f, 2f));
            CreateInvisibleCollider(p4Col, "Boss_Approach_Ceiling", new Vector2(276.5f, -37.5f), new Vector2(48f, 2f));
            // Tường cổng Boss bên trên cánh cửa
            CreateInvisibleCollider(p4Col, "Boss_Door_Header_Wall", new Vector2(299f, -36f), new Vector2(2.5f, 6f));

            // -------------------------------------------------------------------------
            // PHẦN 5: ĐẤU TRƯỜNG TRÙM WEB SPIDER (SPIDER LAIR ARENA: X = 300 đến 320.44, Y = -45.94 đến -35.63)
            // -------------------------------------------------------------------------
            GameObject part5Obj = CreateStageTrackVisual(stageFolder, "Track_Part5_BossArena", sPart5Arena, new Vector3(310.22f, -40.78f, 0f), new Vector2(20.44f, 10.31f));
            Transform p5Col = new GameObject("Colliders_Part5").transform;
            p5Col.SetParent(part5Obj.transform);

            // Sàn đấu trường Web Spider
            CreateInvisibleCollider(p5Col, "Spider_Arena_Floor", new Vector2(310.2f, -44.5f), new Vector2(22f, 2f));
            // Trần đấu trường
            CreateInvisibleCollider(p5Col, "Spider_Arena_Ceiling", new Vector2(310.2f, -36.0f), new Vector2(22f, 2f));
            // Vách tường phải đấu trường
            CreateInvisibleCollider(p5Col, "Spider_Arena_Right_Wall", new Vector2(320.5f, -40.5f), new Vector2(2f, 10f));

            // Tơ nhện trang trí đấu trường chính gốc
            CreateWebDecoration(part5Obj.transform, sWeb, new Vector3(304f, -37f, 0f), new Vector2(4f, 2f));
            CreateWebDecoration(part5Obj.transform, sWeb, new Vector3(316f, -37f, 0f), new Vector2(4f, 2f));
            CreateWebDecoration(part5Obj.transform, sWeb, new Vector3(319f, -39f, 0f), new Vector2(2.5f, 2.5f));

            // =========================================================================
            // 6. CỬA PHÒNG BOSS (BOSS SHUTTER) TẠI X = 299 (CỬA CUỐN MMX CHÍNH GỐC)
            // =========================================================================
            GameObject doorObj = new GameObject("Boss_Shutter_Door");
            doorObj.transform.SetParent(root.transform);
            doorObj.transform.position = new Vector3(299f, -41.25f, 0f);

            GameObject doorMesh = new GameObject("Door_Sprite");
            doorMesh.transform.SetParent(doorObj.transform);
            doorMesh.transform.localPosition = Vector3.zero;
            SpriteRenderer doorSR = doorMesh.AddComponent<SpriteRenderer>();
            doorSR.sprite = sGate != null ? sGate : CreateSimpleSprite(new Color(0.95f, 0.8f, 0.15f));
            doorSR.sortingOrder = 5;
            doorMesh.transform.localScale = new Vector3(1.2f, 4.5f, 1f);

            BoxCollider2D doorCol = doorObj.AddComponent<BoxCollider2D>();
            doorCol.size = new Vector2(0.9f, 4.5f);
            BossDoor bossDoor = doorObj.AddComponent<BossDoor>();

            // =========================================================================
            // 7. TẠO PLAYER (MEGA MAN X) XUẤT PHÁT TẠI KHU VỰC TÁN CÂY (CANOPY)
            // =========================================================================
            GameObject player = new GameObject("Player_MegaManX");
            player.tag = "Player";
            player.transform.SetParent(root.transform);
            player.transform.position = new Vector3(3f, 23.5f, 0f); // Bắt đầu trên mặt đất tán cây

            Rigidbody2D pRb = player.AddComponent<Rigidbody2D>();
            BoxCollider2D pColBox = player.AddComponent<BoxCollider2D>();
            pColBox.size = new Vector2(0.85f, 1.6f);
            pColBox.offset = new Vector2(0f, 0.8f);

            HealthSystem pHealth = player.AddComponent<HealthSystem>();
            DamageFlash pFlash = player.AddComponent<DamageFlash>();
            PlayerController2D pController = player.AddComponent<PlayerController2D>();
            PlayerCombat pCombat = player.AddComponent<PlayerCombat>();

            GameObject pVisual = new GameObject("Visual");
            pVisual.transform.SetParent(player.transform);
            pVisual.transform.localPosition = Vector3.zero;
            pVisual.transform.localScale = Vector3.one;

            SpriteRenderer pSR = pVisual.AddComponent<SpriteRenderer>();
            Sprite xDefaultSprite = Resources.Load<Sprite>("Player/x_idle_0");
            pSR.sprite = xDefaultSprite != null ? xDefaultSprite : CreateSimpleSprite(new Color(0.1f, 0.5f, 0.95f));
            pSR.sortingOrder = 10;

            PlayerSpriteAnimator pAnim = pVisual.AddComponent<PlayerSpriteAnimator>();
            pAnim.LoadSpritesFromResourcesIfEmpty();

            camController.SetTarget(player.transform);

            // =========================================================================
            // 8. BỐ TRÍ KẺ ĐỊCH RỪNG RẬM (ENEMIES) DỌC THEO CÁC CUNG ĐƯỜNG
            // =========================================================================
            Transform enemiesFolder = new GameObject("Enemies_Jungle").transform;
            enemiesFolder.SetParent(root.transform);

            // Quái bọ bò trên cành cây tán lá (Canopy Crawlers)
            CreatePatrolEnemy(enemiesFolder, new Vector3(20f, 22.5f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(56f, 29.0f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(98f, 20.0f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(138f, 17.5f, 0f));

            // Quái ong bay trên khoảng không (Canopy Flying Hornets)
            CreateFlyingHornet(enemiesFolder, new Vector3(35f, 28f, 0f));
            CreateFlyingHornet(enemiesFolder, new Vector3(72f, 27f, 0f));
            CreateFlyingHornet(enemiesFolder, new Vector3(110f, 25f, 0f));

            // Quái bên trong thân cây đại thụ rỗng (Trunk enemies)
            CreatePatrolEnemy(enemiesFolder, new Vector3(165f, -5.5f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(175f, -23.5f, 0f));
            CreateFlyingHornet(enemiesFolder, new Vector3(170f, -18f, 0f));
            CreateFlyingHornet(enemiesFolder, new Vector3(170f, -35f, 0f));

            // Quái tại thung lũng thác nước (River enemies)
            CreatePatrolEnemy(enemiesFolder, new Vector3(202f, -41f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(230f, -40f, 0f));
            CreateFlyingHornet(enemiesFolder, new Vector3(216f, -34f, 0f));

            // =========================================================================
            // 9. VẬT PHẨM HỒI MÁU (HEALTH PICKUPS)
            // =========================================================================
            Transform itemsFolder = new GameObject("Pickups").transform;
            itemsFolder.SetParent(root.transform);
            CreateHealthCapsule(itemsFolder, new Vector3(112f, 33.2f, 0f)); // Bình máu giấu trên tán cây cao!
            CreateHealthCapsule(itemsFolder, new Vector3(165f, -23.3f, 0f)); // Bình máu giấu trong thân cây đại thụ

            // =========================================================================
            // 10. TRÙM WEB SPIDER (JUNGLE SOVEREIGN)
            // =========================================================================
            GameObject bossObj = new GameObject("Boss_WebSpider");
            bossObj.tag = "Boss";
            bossObj.transform.SetParent(stageFolder);
            bossObj.transform.position = new Vector3(312f, -42f, 0f); // Ở giữa đấu trường

            GameObject bVisual = new GameObject("Visual");
            bVisual.transform.SetParent(bossObj.transform);
            bVisual.transform.localPosition = Vector3.zero;
            SpriteRenderer bSR = bVisual.AddComponent<SpriteRenderer>();
            bSR.sprite = CreateSimpleSprite(new Color(0.12f, 0.58f, 0.35f));
            bSR.sortingOrder = 10;
            bVisual.transform.localScale = new Vector3(2.5f, 2.8f, 1f);

            GameObject bEyes = new GameObject("Spider_Eyes");
            bEyes.transform.SetParent(bVisual.transform);
            bEyes.transform.localPosition = new Vector3(-0.25f, 0.2f, 0f);
            SpriteRenderer bEyesSR = bEyes.AddComponent<SpriteRenderer>();
            bEyesSR.sprite = CreateSimpleSprite(new Color(1f, 0.95f, 0.2f));
            bEyesSR.sortingOrder = 11;
            bEyes.transform.localScale = new Vector3(0.4f, 0.3f, 1f);

            BoxCollider2D bCol = bossObj.AddComponent<BoxCollider2D>();
            bCol.size = new Vector2(2.4f, 2.7f);
            Rigidbody2D bRb = bossObj.AddComponent<Rigidbody2D>();

            HealthSystem bHealth = bossObj.AddComponent<HealthSystem>();
            DamageFlash bFlash = bossObj.AddComponent<DamageFlash>();
            BossController bossController = bossObj.AddComponent<BossController>();

            // =========================================================================
            // 11. GIAO DIỆN MÁU (HUD CANVAS)
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
            // 12. BOSS ROOM TRIGGER (TẠI X = 301)
            // =========================================================================
            GameObject triggerObj = new GameObject("Boss_Room_Trigger");
            triggerObj.transform.SetParent(root.transform);
            triggerObj.transform.position = new Vector3(301f, -41f, 0f);

            BoxCollider2D trigCol = triggerObj.AddComponent<BoxCollider2D>();
            trigCol.isTrigger = true;
            trigCol.size = new Vector2(2f, 6f);

            BossRoomTrigger roomTrigger = triggerObj.AddComponent<BossRoomTrigger>();
            // Khóa camera vào giữa đấu trường Web Spider tại (310.2, -40.5)
            roomTrigger.Initialize(bossController, bossDoor, camController, bossHealthBar, new Vector2(310.2f, -40.5f), new Vector2(310.2f, -40.5f));

            Debug.Log("<color=green>[MMX4 Web Spider Stage]</color> Đã xây dựng hoàn chỉnh 100% bản đồ Web Spider từ file Spriters-Resource!");
        }

        private GameObject CreateStageTrackVisual(Transform parent, string name, Sprite sprite, Vector3 centerPos, Vector2 size)
        {
            GameObject trackObj = new GameObject(name);
            trackObj.transform.SetParent(parent);
            trackObj.transform.position = centerPos;

            SpriteRenderer sr = trackObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 0; // Địa hình màn chơi chính
            return trackObj;
        }

        private void CreateInvisibleCollider(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject colObj = new GameObject(name);
            colObj.transform.SetParent(parent);
            colObj.transform.position = pos;

            BoxCollider2D col = colObj.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private void BuildRepeatingBackground(Transform parent, Sprite sprite, Color fallbackColor, int sortingOrder, Vector2 startPos, float totalWidth, float stepX)
        {
            float curX = startPos.x;
            int index = 0;
            while (curX < startPos.x + totalWidth)
            {
                GameObject seg = new GameObject($"BgSegment_{index}");
                seg.transform.SetParent(parent);
                seg.transform.position = new Vector3(curX, startPos.y, 0f);

                SpriteRenderer sr = seg.AddComponent<SpriteRenderer>();
                sr.sprite = sprite != null ? sprite : CreateSimpleSprite(fallbackColor);
                sr.sortingOrder = sortingOrder;

                curX += stepX;
                index++;
            }
        }

        private void BuildWaterfallBackdrop(Transform parent, Sprite sWf, Sprite sCliff, Vector2 centerPos)
        {
            if (sCliff != null)
            {
                GameObject cliffObj = new GameObject("Waterfall_Cliff_Gorge");
                cliffObj.transform.SetParent(parent);
                cliffObj.transform.position = new Vector3(centerPos.x, centerPos.y, 0f);
                SpriteRenderer srCliff = cliffObj.AddComponent<SpriteRenderer>();
                srCliff.sprite = sCliff;
                srCliff.sortingOrder = -22;
                cliffObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            }

            if (sWf != null)
            {
                for (int i = -1; i <= 1; i++)
                {
                    GameObject wfObj = new GameObject($"Waterfall_Cascade_{i}");
                    wfObj.transform.SetParent(parent);
                    wfObj.transform.position = new Vector3(centerPos.x + i * 14f, centerPos.y - 2f, 0f);
                    SpriteRenderer srWf = wfObj.AddComponent<SpriteRenderer>();
                    srWf.sprite = sWf;
                    srWf.sortingOrder = -20;
                    wfObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
                }
            }
        }

        private void BuildInteriorBackdrop(Transform parent, Sprite sInterior, Vector2 centerPos, Vector2 size)
        {
            GameObject bgObj = new GameObject("Interior_DarkTrunk_Backdrop");
            bgObj.transform.SetParent(parent);
            bgObj.transform.position = new Vector3(centerPos.x, centerPos.y, 0f);

            SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
            sr.sprite = sInterior != null ? sInterior : CreateSimpleSprite(new Color(0.10f, 0.08f, 0.06f));
            sr.color = new Color(0.85f, 0.85f, 0.85f, 0.95f);
            sr.sortingOrder = -15;
            bgObj.transform.localScale = new Vector3(size.x / 4f, size.y / 7f, 1f);
        }

        private void CreateVinesDecoration(Transform parent, Sprite sVines, Vector3 pos, Vector2 size)
        {
            GameObject vineObj = new GameObject("Foreground_HangingVines");
            vineObj.transform.SetParent(parent);
            vineObj.transform.position = pos;

            SpriteRenderer sr = vineObj.AddComponent<SpriteRenderer>();
            sr.sprite = sVines != null ? sVines : CreateSimpleSprite(new Color(0.18f, 0.55f, 0.28f, 0.85f));
            sr.sortingOrder = 15; // Tiền cảnh (vượt qua trước nhân vật)
            vineObj.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private void CreateWebDecoration(Transform parent, Sprite sWeb, Vector3 pos, Vector2 size)
        {
            GameObject web = new GameObject("Ceiling_SpiderWeb");
            web.transform.SetParent(parent);
            web.transform.position = pos;

            SpriteRenderer sr = web.AddComponent<SpriteRenderer>();
            sr.sprite = sWeb != null ? sWeb : CreateSimpleSprite(new Color(0.9f, 0.95f, 0.92f, 0.6f));
            sr.color = new Color(1f, 1f, 1f, 0.75f);
            sr.sortingOrder = 2;
            web.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private void CreatePatrolEnemy(Transform parent, Vector3 pos)
        {
            GameObject enemy = new GameObject("Enemy_SpikeCrawler");
            enemy.tag = "Enemy";
            enemy.transform.SetParent(parent);
            enemy.transform.position = pos;

            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite(new Color(0.75f, 0.25f, 0.2f));
            sr.sortingOrder = 5;
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
            sr.sprite = CreateSimpleSprite(new Color(0.95f, 0.8f, 0.15f));
            sr.sortingOrder = 5;
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
            sr.sprite = CreateSimpleSprite(new Color(0.2f, 0.95f, 0.3f));
            sr.sortingOrder = 6;
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
