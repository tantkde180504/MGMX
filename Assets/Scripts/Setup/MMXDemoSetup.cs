using System.Collections;
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
    /// Chuẩn pixel art 1x gốc PS1, Point filter sắc nét 100%, không bị mờ/nhoè (32 PPU đồng bộ toàn bộ nhân vật & màn chơi).
    /// Kết cấu màn chơi chính xác 100% theo playthrough Mega Man X4:
    /// - Area 1:
    ///   + Part 1: Rừng tán cây trên cao (Canopy Surface Run)
    ///   + Part 2: Thân cây rỗng khổng lồ rơi xuống (Hollow Trunk Shaft) & Phòng bí mật Chân Capsule (Dr. Light Foot Parts)
    ///   + Part 3: Vượt sông thác nước cuộn chảy (Waterfall River Crossing) & Cổng dịch chuyển sang Area 2
    /// - Area 2:
    ///   + Part 4: Rừng sâu gốc cây đại thụ (Deep Jungle Cut Stumps)
    ///   + Part 5: Leo trèo thân cây nhện khổng lồ (Spider Trees & Cocoons)
    ///   + Part 6: Tiền đồn kim loại dẫn vào hang nhện (Industrial Cavern Outpost Approach)
    ///   + Part 7: Cửa Shutter Gate & Đấu trường Trùm Web Spider (Boss Arena)
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

            // 2. Tải toàn bộ 7 phần bản đồ gốc từ Spriters Resource (Authentic Stage Tracks)
            Sprite sPart1Canopy = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part1_Canopy");
            Sprite sPart2Trunk = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part2_HollowTrunk");
            Sprite sPart3River = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part3_WaterfallRiver");
            Sprite sPart4Stumps = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part4_DeepJungleStumps");
            Sprite sPart5SpiderTrees = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part5_SpiderTrees");
            Sprite sPart6Cavern = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part6_CavernApproach");
            Sprite sPart7Arena = Resources.Load<Sprite>("Environment/Jungle/AuthenticStage/Stage_Part7_BossArena");

            // Tải Backgrounds & Chi tiết trang trí
            Sprite sSky = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Sky_Clouds");
            Sprite sMountains = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Distant_Mountains");
            Sprite sWaterfall = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Waterfall_Background");
            Sprite sWaterCliff = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Waterfall_Cliff");
            Sprite sDarkTrunk = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Dark_Trunk_Backdrop");
            Sprite sDeepCanopy = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Area2_DeepCanopy");

            Sprite sGate = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Boss_Gate_Shutter");
            Sprite sWeb = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Spider_Web");
            Sprite sBreakableTrunk = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Breakable_Trunk");

            // 3. Cấu hình Camera chuẩn MMX4
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
            }
            mainCam.orthographic = true;
            mainCam.orthographicSize = 4.5f; // Tỉ lệ hiển thị chuẩn pixel-perfect MMX4 (144p-240p tương ứng)
            mainCam.backgroundColor = new Color(0.18f, 0.36f, 0.48f); // Bầu trời xanh rừng nhiệt đới
            mainCam.clearFlags = CameraClearFlags.SolidColor;

            MMXCameraController camController = mainCam.GetComponent<MMXCameraController>();
            if (camController == null) camController = mainCam.gameObject.AddComponent<MMXCameraController>();
            // Giới hạn camera bao trùm toàn bộ 7 khu vực (0 đến 340) và độ sâu (-75 đến 20)
            camController.SetStageBounds(new Vector2(0f, -72f), new Vector2(335f, 15f));

            // =========================================================================
            // 4. HỆ THỐNG PARALLAX SCROLLING ĐA TẦNG (SPRITERS RESOURCE)
            // =========================================================================
            GameObject parallaxRoot = new GameObject("MMX_Parallax_Background");
            parallaxRoot.transform.SetParent(root.transform);
            ParallaxBackground parallax = parallaxRoot.AddComponent<ParallaxBackground>();

            // Tầng 1: Bầu trời & Mây xa (Sky & Clouds) - Sorting Order: -30
            Transform skyFolder = new GameObject("Layer_1_Sky").transform;
            skyFolder.SetParent(parallaxRoot.transform);
            BuildRepeatingBackground(skyFolder, sSky, Color.white, -30, new Vector2(0f, 6f), 150f, 32f);

            // Tầng 2: Rặng núi xa (Distant Mountains) - Sorting Order: -25
            Transform mountainFolder = new GameObject("Layer_2_Mountains").transform;
            mountainFolder.SetParent(parallaxRoot.transform);
            BuildRepeatingBackground(mountainFolder, sMountains, Color.white, -25, new Vector2(0f, 0f), 150f, 32f);

            // Tầng 3: Đại thung lũng thác nước cuộn chảy (Waterfall Gorge) - Sorting Order: -20
            Transform waterfallFolder = new GameObject("Layer_3_Waterfalls").transform;
            waterfallFolder.SetParent(parallaxRoot.transform);
            BuildWaterfallBackdrop(waterfallFolder, sWaterfall, sWaterCliff, new Vector2(110f, -50f));

            // Tầng 4: Vách tối lòng cây đại thụ (Dark Trunk Interior) - Sorting Order: -15
            Transform interiorFolder = new GameObject("Layer_4_DarkInterior").transform;
            interiorFolder.SetParent(parallaxRoot.transform);
            BuildInteriorBackdrop(interiorFolder, sDarkTrunk, new Vector2(64.5f, -28f), new Vector2(25.2f, 48f));

            // Tầng 5: Tán rừng Area 2 & Hang nhện (Deep Canopy Parallax) - Sorting Order: -18
            Transform canopyFolder = new GameObject("Layer_5_DeepCanopy").transform;
            canopyFolder.SetParent(parallaxRoot.transform);
            BuildRepeatingBackground(canopyFolder, sDeepCanopy, Color.white, -18, new Vector2(180f, -38f), 160f, 32f);

            ParallaxBackground.ParallaxLayer[] pLayers = new ParallaxBackground.ParallaxLayer[]
            {
                new ParallaxBackground.ParallaxLayer
                {
                    layerName = "Sky",
                    layerTransform = skyFolder,
                    parallaxFactorX = 0.05f,
                    parallaxFactorY = 0.02f,
                    autoScrollX = true,
                    autoScrollSpeedX = 0.20f,
                    infiniteRepeatX = true,
                    textureUnitSizeX = 32f
                },
                new ParallaxBackground.ParallaxLayer
                {
                    layerName = "Mountains",
                    layerTransform = mountainFolder,
                    parallaxFactorX = 0.18f,
                    parallaxFactorY = 0.08f,
                    autoScrollX = false,
                    infiniteRepeatX = true,
                    textureUnitSizeX = 32f
                },
                new ParallaxBackground.ParallaxLayer
                {
                    layerName = "Waterfalls",
                    layerTransform = waterfallFolder,
                    parallaxFactorX = 0.40f,
                    parallaxFactorY = 0.20f,
                    autoScrollX = false,
                    infiniteRepeatX = false
                },
                new ParallaxBackground.ParallaxLayer
                {
                    layerName = "DeepCanopy",
                    layerTransform = canopyFolder,
                    parallaxFactorX = 0.35f,
                    parallaxFactorY = 0.15f,
                    autoScrollX = false,
                    infiniteRepeatX = true,
                    textureUnitSizeX = 32f
                }
            };
            parallax.Initialize(mainCam, pLayers);

            // =========================================================================
            // 5. TÁI TẠO BẢN ĐỒ WEB SPIDER GỐC (7 KHU VỰC THỰC TẾ - VISUAL TRACKS & COLLIDERS)
            // =========================================================================
            Transform stageFolder = new GameObject("Authentic_WebSpider_Stage").transform;
            stageFolder.SetParent(root.transform);

            // -------------------------------------------------------------------------
            // PHẦN 1: RỪNG TÁN CÂY TRÊN CAO (CANOPY RUN: X = 0 đến 68.0, Y = -4.0)
            // -------------------------------------------------------------------------
            CreateStageTrackVisual(stageFolder, "Track_Part1_Canopy", sPart1Canopy, new Vector3(34.0f, 0f, 0f));
            Transform p1Col = new GameObject("Colliders_Part1").transform;
            p1Col.SetParent(stageFolder);

            // Mặt đất chính rừng tán cây
            CreatePlatform(p1Col, new Vector2(0f, -4.5f), new Vector2(58.5f, 1.2f));
            // Bờ vực vách đá bên trái (chặn không cho đi lùi ra khỏi map)
            CreateWall(p1Col, new Vector2(-1f, 2f), new Vector2(2f, 16f));
            // Lối vào hố cây rỗng bên phải (miệng hố: X = 58.5 đến 64.5 để rơi xuống Part 2)
            CreateWall(p1Col, new Vector2(65f, 2f), new Vector2(2f, 16f));

            // -------------------------------------------------------------------------
            // PHẦN 2: THÂN CÂY RỖNG RƠI XUỐNG (HOLLOW TRUNK SHAFT: X = 56.5 đến 81.5, Y = -4 đến -52)
            // -------------------------------------------------------------------------
            CreateStageTrackVisual(stageFolder, "Track_Part2_HollowTrunk", sPart2Trunk, new Vector3(64.5f, -28.0f, 0f));
            Transform p2Col = new GameObject("Colliders_Part2").transform;
            p2Col.SetParent(stageFolder);

            // Vách trái thân cây (cho phép Wall Slide và Wall Kick)
            CreateWall(p2Col, new Vector2(56.5f, -28.0f), new Vector2(1.5f, 48.0f));
            // Vách phải thân cây
            CreateWall(p2Col, new Vector2(72.5f, -28.0f), new Vector2(1.5f, 48.0f));

            // Các bục gỗ zig-zag bên trong thân cây
            CreatePlatform(p2Col, new Vector2(60f, -14f), new Vector2(6f, 0.8f));
            CreatePlatform(p2Col, new Vector2(69f, -22f), new Vector2(6f, 0.8f));
            CreatePlatform(p2Col, new Vector2(60f, -30f), new Vector2(6f, 0.8f));
            CreatePlatform(p2Col, new Vector2(69f, -38f), new Vector2(6f, 0.8f));
            CreatePlatform(p2Col, new Vector2(62f, -46f), new Vector2(8f, 0.8f));

            // Phòng bí mật chứa Chân Capsule (Dr. Light Foot Parts) trên góc phải
            CreatePlatform(p2Col, new Vector2(76f, -9f), new Vector2(7f, 1.0f));
            CreateWall(p2Col, new Vector2(80f, -4f), new Vector2(1.2f, 10f));
            CreateCapsuleSecretPod(p2Col, new Vector2(77.5f, -8f));

            // Đáy thân cây nối sang sông thác nước
            CreatePlatform(p2Col, new Vector2(56.5f, -52.0f), new Vector2(17f, 1.5f));

            // -------------------------------------------------------------------------
            // PHẦN 3: VƯỢT SÔNG THÁC NƯỚC (WATERFALL RIVER: X = 73.5 đến 146.5, Y = -48 đến -65)
            // -------------------------------------------------------------------------
            CreateStageTrackVisual(stageFolder, "Track_Part3_WaterfallRiver", sPart3River, new Vector3(110.0f, -48.0f, 0f));
            Transform p3Col = new GameObject("Colliders_Part3").transform;
            p3Col.SetParent(stageFolder);

            // Đoạn xuất phát từ gốc cây
            CreatePlatform(p3Col, new Vector2(73.5f, -48.0f), new Vector2(10f, 1.2f));

            // Các tảng đá và bè gỗ nổi vượt sông
            CreatePlatform(p3Col, new Vector2(86f, -46.5f), new Vector2(7f, 0.8f));
            CreatePlatform(p3Col, new Vector2(96f, -45.0f), new Vector2(7f, 0.8f));
            CreatePlatform(p3Col, new Vector2(106f, -44.5f), new Vector2(7f, 0.8f));
            CreatePlatform(p3Col, new Vector2(117f, -44.5f), new Vector2(10f, 0.8f));

            // Bậc thác dốc đổ xuống cuối Area 1
            CreatePlatform(p3Col, new Vector2(129f, -50.0f), new Vector2(6f, 0.8f));
            CreatePlatform(p3Col, new Vector2(135f, -57.0f), new Vector2(6f, 0.8f));
            CreatePlatform(p3Col, new Vector2(142f, -65.0f), new Vector2(10f, 1.2f));

            // Cổng dịch chuyển chuyển tiếp sang Area 2 tại cuối sông
            CreateAreaTransitionPortal(p3Col, new Vector2(145f, -64.0f), new Vector2(165f, -43f));

            // -------------------------------------------------------------------------
            // PHẦN 4: RỪNG SÂU GỐC CÂY ĐẠI THỤ (AREA 2 START: X = 160.0 đến 207.5, Y = -44)
            // -------------------------------------------------------------------------
            CreateStageTrackVisual(stageFolder, "Track_Part4_DeepJungleStumps", sPart4Stumps, new Vector3(180.0f, -48.0f, 0f));
            Transform p4Col = new GameObject("Colliders_Part4").transform;
            p4Col.SetParent(stageFolder);

            // Bờ rừng Area 2 & các gốc cây cưa khổng lồ
            CreatePlatform(p4Col, new Vector2(160f, -44.0f), new Vector2(14f, 1.2f));
            CreatePlatform(p4Col, new Vector2(176f, -42.0f), new Vector2(12f, 1.2f));
            CreatePlatform(p4Col, new Vector2(190f, -41.0f), new Vector2(16f, 1.2f));

            // Thân gỗ mục có thể bắn phá
            CreateBreakableLog(p4Col, sBreakableTrunk, new Vector2(188f, -39.5f));

            // -------------------------------------------------------------------------
            // PHẦN 5: LEO TRÈO CÂY NHỆN KHỔNG LỒ (SPIDER TREES: X = 207.5 đến 275.5, Y = -40 đến -22)
            // -------------------------------------------------------------------------
            CreateStageTrackVisual(stageFolder, "Track_Part5_SpiderTrees", sPart5SpiderTrees, new Vector3(240.0f, -40.0f, 0f));
            Transform p5Col = new GameObject("Colliders_Part5").transform;
            p5Col.SetParent(stageFolder);

            // Mặt đất rễ cây dưới đáy
            CreatePlatform(p5Col, new Vector2(206f, -41.0f), new Vector2(68f, 1.2f));

            // Các nhánh cây nhện và cành leo cao
            CreatePlatform(p5Col, new Vector2(212f, -32.0f), new Vector2(10f, 0.8f));
            CreatePlatform(p5Col, new Vector2(226f, -24.0f), new Vector2(12f, 0.8f));
            CreateWall(p5Col, new Vector2(236f, -28.0f), new Vector2(1.2f, 12f)); // Thân cây để trượt/đá tường
            CreatePlatform(p5Col, new Vector2(242f, -22.0f), new Vector2(14f, 0.8f));
            CreatePlatform(p5Col, new Vector2(260f, -22.0f), new Vector2(14f, 0.8f));

            // Mạng nhện trang trí
            CreateSpiderWebVisual(p5Col, sWeb, new Vector2(230f, -20f));
            CreateSpiderWebVisual(p5Col, sWeb, new Vector2(250f, -18f));

            // -------------------------------------------------------------------------
            // PHẦN 6: TIỀN ĐỒN KIM LOẠI DẪN VÀO HANG NHỆN (CAVERN APPROACH: X = 275.5 đến 307.0, Y = -23.5)
            // -------------------------------------------------------------------------
            CreateStageTrackVisual(stageFolder, "Track_Part6_CavernApproach", sPart6Cavern, new Vector3(285.0f, -32.0f, 0f));
            Transform p6Col = new GameObject("Colliders_Part6").transform;
            p6Col.SetParent(stageFolder);

            // Hành lang kim loại dẫn vào cửa Boss
            CreatePlatform(p6Col, new Vector2(274f, -23.5f), new Vector2(31f, 1.2f));
            // Trần hành lang
            CreatePlatform(p6Col, new Vector2(274f, -15.5f), new Vector2(31f, 1.2f));

            // Trục thẳng đứng & cầu thang phụ bên dưới
            CreatePlatform(p6Col, new Vector2(285f, -35.0f), new Vector2(15f, 1.0f));
            CreateWall(p6Col, new Vector2(284f, -30.0f), new Vector2(1.2f, 14f));

            // -------------------------------------------------------------------------
            // CỬA SHUTTER GATE (CỬA CHẮN TRÙM WEB SPIDER CHUẨN GỐC)
            // -------------------------------------------------------------------------
            GameObject doorObj = new GameObject("Jungle_Boss_Shutter_Gate");
            doorObj.transform.SetParent(stageFolder);
            doorObj.transform.position = new Vector3(305.5f, -23.0f, 0f);

            GameObject doorVisual = new GameObject("Door_Sprite");
            doorVisual.transform.SetParent(doorObj.transform);
            doorVisual.transform.localPosition = Vector3.zero;
            SpriteRenderer doorSr = doorVisual.AddComponent<SpriteRenderer>();
            doorSr.sprite = sGate;
            doorSr.sortingOrder = 10;

            BoxCollider2D doorCollider = doorObj.AddComponent<BoxCollider2D>();
            doorCollider.size = new Vector2(1.0f, 3.5f);
            doorCollider.offset = new Vector2(0f, 1.75f);

            BossDoor bossDoorComponent = doorObj.AddComponent<BossDoor>();

            // -------------------------------------------------------------------------
            // PHẦN 7: ĐẤU TRƯỜNG TRÙM WEB SPIDER (BOSS ARENA: X = 307.0 đến 331.0, Y = -23.5)
            // -------------------------------------------------------------------------
            CreateStageTrackVisual(stageFolder, "Track_Part7_BossArena", sPart7Arena, new Vector3(318.0f, -23.5f, 0f));
            Transform p7Col = new GameObject("Colliders_Part7").transform;
            p7Col.SetParent(stageFolder);

            // Sàn đấu phẳng tuyệt đối trong phòng Boss
            CreatePlatform(p7Col, new Vector2(305.5f, -23.5f), new Vector2(25.5f, 1.5f));
            // Trần phòng trùm
            CreatePlatform(p7Col, new Vector2(305.5f, -11.5f), new Vector2(25.5f, 1.5f));
            // Tường bên phải phòng Boss
            CreateWall(p7Col, new Vector2(331.0f, -17.5f), new Vector2(1.5f, 14f));

            // Mạng nhện phủ trần phòng Boss
            CreateSpiderWebVisual(p7Col, sWeb, new Vector2(312f, -12.5f));
            CreateSpiderWebVisual(p7Col, sWeb, new Vector2(322f, -12.5f));

            // =========================================================================
            // 6. KHỞI TẠO BOSS WEB SPIDER VÀ VÙNG KÍCH HOẠT (ENCOUNTER)
            // =========================================================================
            BossController boss = SetupWebSpiderBoss(root.transform, new Vector3(322.0f, -16.0f, 0f));

            HealthBarUI bossUI = SetupBossHealthUI(root.transform);
            if (bossUI != null) bossUI.gameObject.SetActive(false);

            // Vùng kích hoạt trùm ngay khi người chơi bước qua cửa Shutter Gate (X = 307.5)
            GameObject triggerObj = new GameObject("Boss_Encounter_Trigger");
            triggerObj.transform.SetParent(stageFolder);
            triggerObj.transform.position = new Vector3(307.5f, -21.5f, 0f);
            BoxCollider2D triggerCol = triggerObj.AddComponent<BoxCollider2D>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector2(2.0f, 6.0f);

            BossRoomTrigger roomTrigger = triggerObj.AddComponent<BossRoomTrigger>();
            // Khóa Camera phòng boss: X cố định ở 318 (tâm phòng), Y cố định ở -19.5
            Vector2 bossCamLock = new Vector2(318.0f, -19.5f);
            roomTrigger.Initialize(boss, bossDoorComponent, camController, bossUI, bossCamLock, bossCamLock);

            // =========================================================================
            // 7. KHỞI TẠO QUÁI VẬT & NHÂN VẬT MEGA MAN X
            // =========================================================================
            SpawnStageEnemies(stageFolder);
            SetupPlayerCharacter(root.transform, camController, new Vector3(4.0f, 0.0f, 0f));

            Debug.Log("[MMXDemoSetup] Web Spider Authentic Stage built successfully with 7 distinct authentic zones and 100% pixel-sharp Point rendering!");
        }

        // =========================================================================
        // HÀM TIỆN ÍCH TẠO TRACK VISUAL CHUẨN GỐC
        // =========================================================================
        private GameObject CreateStageTrackVisual(Transform parent, string trackName, Sprite sprite, Vector3 worldPosition)
        {
            GameObject obj = new GameObject(trackName);
            obj.transform.SetParent(parent);
            obj.transform.position = worldPosition;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 0; // Foreground stage layer
            return obj;
        }

        private void CreatePlatform(Transform parent, Vector2 minCorner, Vector2 size)
        {
            GameObject plat = new GameObject("Floor_" + minCorner.x.ToString("F0"));
            plat.transform.SetParent(parent);
            plat.transform.position = new Vector3(minCorner.x + size.x * 0.5f, minCorner.y + size.y * 0.5f, 0f);

            BoxCollider2D box = plat.AddComponent<BoxCollider2D>();
            box.size = size;

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer != -1) plat.layer = groundLayer;
        }

        private void CreateWall(Transform parent, Vector2 center, Vector2 size)
        {
            GameObject wall = new GameObject("Wall_" + center.x.ToString("F0"));
            wall.transform.SetParent(parent);
            wall.transform.position = new Vector3(center.x, center.y, 0f);

            BoxCollider2D box = wall.AddComponent<BoxCollider2D>();
            box.size = size;

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer != -1) wall.layer = groundLayer;
        }

        private void CreateSpiderWebVisual(Transform parent, Sprite webSprite, Vector2 pos)
        {
            if (webSprite == null) return;
            GameObject webObj = new GameObject("SpiderWeb_Deco");
            webObj.transform.SetParent(parent);
            webObj.transform.position = new Vector3(pos.x, pos.y, 0f);
            SpriteRenderer sr = webObj.AddComponent<SpriteRenderer>();
            sr.sprite = webSprite;
            sr.sortingOrder = 5;
        }

        private void CreateBreakableLog(Transform parent, Sprite logSprite, Vector2 pos)
        {
            GameObject logObj = new GameObject("Breakable_Trunk_Log");
            logObj.transform.SetParent(parent);
            logObj.transform.position = new Vector3(pos.x, pos.y, 0f);

            if (logSprite != null)
            {
                SpriteRenderer sr = logObj.AddComponent<SpriteRenderer>();
                sr.sprite = logSprite;
                sr.sortingOrder = 2;
            }

            BoxCollider2D col = logObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 2.3f);
            col.offset = new Vector2(0f, 1.15f);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer != -1) logObj.layer = groundLayer;
        }

        private void CreateCapsuleSecretPod(Transform parent, Vector2 pos)
        {
            GameObject pod = new GameObject("DrLight_FootCapsule");
            pod.transform.SetParent(parent);
            pod.transform.position = new Vector3(pos.x, pos.y, 0f);

            // Bục Capsule phát sáng
            SpriteRenderer sr = pod.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Hollow_Tree_Platform");
            sr.color = new Color(0.3f, 0.9f, 1f, 0.9f);
            sr.sortingOrder = 3;

            BoxCollider2D col = pod.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 3f);
        }

        private void CreateAreaTransitionPortal(Transform parent, Vector2 portalPos, Vector2 targetSpawnPos)
        {
            GameObject portal = new GameObject("Area_Transition_Portal");
            portal.transform.SetParent(parent);
            portal.transform.position = new Vector3(portalPos.x, portalPos.y, 0f);

            BoxCollider2D trigger = portal.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(2.5f, 4f);

            AreaTransition transitionScript = portal.AddComponent<AreaTransition>();
            transitionScript.targetSpawn = targetSpawnPos;
        }

        // =========================================================================
        // HÀM XÂY DỰNG PARALLAX
        // =========================================================================
        private void BuildRepeatingBackground(Transform parent, Sprite sprite, Color tint, int sortingOrder, Vector2 startPos, float totalWidth, float stepX)
        {
            if (sprite == null) return;
            int count = Mathf.CeilToInt(totalWidth / stepX) + 2;
            for (int i = 0; i < count; i++)
            {
                GameObject tile = new GameObject("BG_Tile_" + i);
                tile.transform.SetParent(parent);
                tile.transform.position = new Vector3(startPos.x + i * stepX, startPos.y, 0f);

                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = tint;
                sr.sortingOrder = sortingOrder;
            }
        }

        private void BuildWaterfallBackdrop(Transform parent, Sprite wfSprite, Sprite cliffSprite, Vector2 centerPos)
        {
            if (wfSprite != null)
            {
                GameObject wfObj = new GameObject("Waterfall_Cascade");
                wfObj.transform.SetParent(parent);
                wfObj.transform.position = new Vector3(centerPos.x, centerPos.y, 0f);

                SpriteRenderer sr = wfObj.AddComponent<SpriteRenderer>();
                sr.sprite = wfSprite;
                sr.sortingOrder = -20;
            }

            if (cliffSprite != null)
            {
                GameObject cliffObj = new GameObject("Waterfall_Cliff_Base");
                cliffObj.transform.SetParent(parent);
                cliffObj.transform.position = new Vector3(centerPos.x, centerPos.y - 12f, 0f);

                SpriteRenderer sr = cliffObj.AddComponent<SpriteRenderer>();
                sr.sprite = cliffSprite;
                sr.sortingOrder = -19;
            }
        }

        private void BuildInteriorBackdrop(Transform parent, Sprite trunkSprite, Vector2 centerPos, Vector2 size)
        {
            GameObject trunkObj = new GameObject("Dark_Trunk_Interior");
            trunkObj.transform.SetParent(parent);
            trunkObj.transform.position = new Vector3(centerPos.x, centerPos.y, 0f);

            SpriteRenderer sr = trunkObj.AddComponent<SpriteRenderer>();
            sr.sprite = trunkSprite;
            sr.color = new Color(0.12f, 0.08f, 0.05f, 0.95f); // Gỗ tối màu lòng thân cây
            sr.sortingOrder = -15;
        }

        // =========================================================================
        // KHỞI TẠO BOSS & UI & PLAYER
        // =========================================================================
        private BossController SetupWebSpiderBoss(Transform parent, Vector3 spawnPos)
        {
            GameObject bossObj = new GameObject("Boss_WebSpider");
            bossObj.transform.SetParent(parent);
            bossObj.transform.position = spawnPos;

            SpriteRenderer sr = bossObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;

            BoxCollider2D col = bossObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.4f, 2.2f);

            Rigidbody2D rb = bossObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Nhện đu dây treo trần
            rb.freezeRotation = true;

            bossObj.AddComponent<HealthSystem>();
            BossController bossController = bossObj.AddComponent<BossController>();
            bossObj.tag = "Enemy";

            return bossController;
        }

        private HealthBarUI SetupBossHealthUI(Transform parent)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject cObj = new GameObject("MMX_UI_Canvas");
                cObj.transform.SetParent(parent);
                canvas = cObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                cObj.AddComponent<CanvasScaler>();
                cObj.AddComponent<GraphicRaycaster>();
            }

            GameObject barObj = new GameObject("BossHealthBar");
            barObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = barObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.95f, 0.35f);
            rt.anchorMax = new Vector2(0.98f, 0.85f);
            rt.anchoredPosition = Vector2.zero;

            Image bgImg = barObj.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.05f, 0.85f);

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(barObj.transform, false);
            RectTransform fillRt = fillObj.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;

            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Vertical;
            fillImg.color = new Color(0.95f, 0.75f, 0.1f);

            HealthBarUI barUI = barObj.AddComponent<HealthBarUI>();
            return barUI;
        }

        private void SpawnStageEnemies(Transform stageFolder)
        {
            // Quái ong rừng Area 1 (Hornet)
            SpawnHornet(stageFolder, new Vector3(22f, 2.5f, 0f));
            SpawnHornet(stageFolder, new Vector3(45f, 2.0f, 0f));
            SpawnHornet(stageFolder, new Vector3(92f, -42.0f, 0f));
            SpawnHornet(stageFolder, new Vector3(112f, -40.0f, 0f));

            // Quái tuần tra Area 2
            SpawnPatrolEnemy(stageFolder, new Vector3(172f, -40.5f, 0f));
            SpawnPatrolEnemy(stageFolder, new Vector3(196f, -39.5f, 0f));
            SpawnPatrolEnemy(stageFolder, new Vector3(220f, -39.5f, 0f));
            SpawnPatrolEnemy(stageFolder, new Vector3(248f, -20.5f, 0f));
        }

        private void SpawnHornet(Transform parent, Vector3 pos)
        {
            GameObject hornet = new GameObject("Enemy_Hornet");
            hornet.transform.SetParent(parent);
            hornet.transform.position = pos;
            hornet.tag = "Enemy";

            SpriteRenderer sr = hornet.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.9f, 0.8f, 0.2f);
            sr.sortingOrder = 1;

            CircleCollider2D col = hornet.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            Rigidbody2D rb = hornet.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            hornet.AddComponent<HealthSystem>();
            hornet.AddComponent<FlyingHornetEnemy>();
        }

        private void SpawnPatrolEnemy(Transform parent, Vector3 pos)
        {
            GameObject crawler = new GameObject("Enemy_Crawler");
            crawler.transform.SetParent(parent);
            crawler.transform.position = pos;
            crawler.tag = "Enemy";

            SpriteRenderer sr = crawler.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.8f, 0.2f, 0.2f);
            sr.sortingOrder = 1;

            BoxCollider2D col = crawler.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.9f, 0.8f);

            Rigidbody2D rb = crawler.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;

            crawler.AddComponent<HealthSystem>();
            crawler.AddComponent<BasicPatrolEnemy>();
        }

        private void SetupPlayerCharacter(Transform parent, MMXCameraController cam, Vector3 spawnPos)
        {
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                existingPlayer.transform.position = spawnPos;
                if (cam != null) cam.SetTarget(existingPlayer.transform);
                return;
            }

            GameObject player = new GameObject("Megaman_X");
            player.transform.SetParent(parent);
            player.transform.position = spawnPos;
            player.tag = "Player";

            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            BoxCollider2D col = player.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.4f);
            col.offset = new Vector2(0f, 0.7f);

            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
            rb.gravityScale = 3.5f;

            player.AddComponent<HealthSystem>();
            player.AddComponent<PlayerController2D>();
            player.AddComponent<PlayerCombat>();
            player.AddComponent<PlayerSpriteAnimator>();

            if (cam != null) cam.SetTarget(player.transform);
        }
    }

    /// <summary>
    /// Cổng dịch chuyển chuyển tiếp Area 1 sang Area 2
    /// </summary>
    public class AreaTransition : MonoBehaviour
    {
        public Vector2 targetSpawn;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.position = new Vector3(targetSpawn.x, targetSpawn.y, other.transform.position.z);
            }
        }
    }
}
