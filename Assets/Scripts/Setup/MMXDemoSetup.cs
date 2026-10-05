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
    /// Script tiện ích tự động xây dựng toàn bộ Màn chơi Web Spider (Jungle Stage) chuẩn quy mô Mega Man X4 trong Unity.
    /// Sử dụng bộ tài nguyên Backgrounds & Terrain chính gốc từ spriters-resource.com kết hợp Parallax đa tầng.
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

        [ContextMenu("Build MMX4 Demo Scene")]
        public void BuildFullDemoStage()
        {
            // 1. Dọn dẹp các đối tượng demo cũ nếu có
            GameObject existingDemo = GameObject.Find("MMX_Demo_Environment");
            if (existingDemo != null) DestroyImmediate(existingDemo);

            GameObject root = new GameObject("MMX_Demo_Environment");

            // 2. Tải toàn bộ Sprite môi trường rừng rậm (Jungle Stage Assets từ spriters-resource.com)
            Sprite sSky = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Sky_Clouds");
            Sprite sMountains = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Distant_Mountains");
            Sprite sWaterfall = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Waterfall_Background");
            Sprite sWaterCliff = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Waterfall_Cliff");
            Sprite sDarkTrunk = Resources.Load<Sprite>("Environment/Jungle/Backgrounds/Jungle_Dark_Trunk_Backdrop");

            Sprite sGround = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Ground_MossyLog");
            Sprite sBranch = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Canopy_Branch");
            Sprite sTrunkL = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Trunk_Wall_Left");
            Sprite sTrunkR = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Trunk_Wall_Right");
            Sprite sShelf = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Hollow_Tree_Platform");
            Sprite sRiverRock = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_River_Rock_Ledge");
            Sprite sGate = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Boss_Gate_Shutter");
            Sprite sVines = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Hanging_Vines");
            Sprite sWeb = Resources.Load<Sprite>("Environment/Jungle/Terrain/Jungle_Spider_Web");

            // 3. Tạo Camera chuẩn màn hình cuộn MMX4
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
            }
            mainCam.orthographic = true;
            mainCam.orthographicSize = 4.5f; // Tỉ lệ hiển thị chuẩn MMX4 (rõ nét pixel-art, không bị thu nhỏ)
            // Màu nền khí quyển bầu trời xanh rừng rậm nhiệt đới MMX4
            mainCam.backgroundColor = new Color(0.18f, 0.36f, 0.48f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;

            MMXCameraController camController = mainCam.GetComponent<MMXCameraController>();
            if (camController == null) camController = mainCam.gameObject.AddComponent<MMXCameraController>();
            camController.SetStageBounds(new Vector2(0f, -4f), new Vector2(220f, 22f));

            // =========================================================================
            // 4. HỆ THỐNG BACKGROUND & PARALLAX SCROLLING ĐA TẦNG (SPRITERS-RESOURCE)
            // =========================================================================
            GameObject parallaxRoot = new GameObject("MMX_Parallax_Background");
            parallaxRoot.transform.SetParent(root.transform);
            ParallaxBackground parallax = parallaxRoot.AddComponent<ParallaxBackground>();

            // Tầng 1: Bầu trời & Mây xa (Sky & Clouds) - Sorting Order: -30
            Transform skyFolder = new GameObject("Layer_1_Sky").transform;
            skyFolder.SetParent(parallaxRoot.transform);
            BuildRepeatingBackground(skyFolder, sSky, new Color(0.35f, 0.60f, 0.85f), -30, new Vector2(-15f, 10f), 240f, 11.25f, 2.8f);

            // Tầng 2: Rặng núi xa & Tán lá viễn cảnh (Distant Mountains) - Sorting Order: -25
            Transform mountainFolder = new GameObject("Layer_2_Mountains").transform;
            mountainFolder.SetParent(parallaxRoot.transform);
            BuildRepeatingBackground(mountainFolder, sMountains, new Color(0.15f, 0.38f, 0.28f), -25, new Vector2(-10f, 4.5f), 240f, 11.25f, 2.5f);

            // Tầng 3: Đại thung lũng thác nước cuộn chảy (Waterfall Gorge) - Sorting Order: -20
            Transform waterfallFolder = new GameObject("Layer_3_Waterfalls").transform;
            waterfallFolder.SetParent(parallaxRoot.transform);
            // Thác nước tại khu vực vực thẳm X = 45 đến 95
            BuildWaterfallBackdrop(waterfallFolder, sWaterfall, sWaterCliff, new Vector2(68f, 1f));

            // Tầng 4: Vách gỗ lòng cây đại thụ & Hang nhện (Dark Interior) - Sorting Order: -15
            Transform interiorFolder = new GameObject("Layer_4_DarkInterior").transform;
            interiorFolder.SetParent(parallaxRoot.transform);
            BuildInteriorBackdrop(interiorFolder, sDarkTrunk, new Vector2(115f, 9.5f), new Vector2(36f, 24f));
            BuildInteriorBackdrop(interiorFolder, sDarkTrunk, new Vector2(193f, 6.5f), new Vector2(46f, 18f));

            // Khởi tạo Parallax Controller
            ParallaxBackground.ParallaxLayer[] pLayers = new ParallaxBackground.ParallaxLayer[]
            {
                new ParallaxBackground.ParallaxLayer
                {
                    layerName = "Sky",
                    layerTransform = skyFolder,
                    parallaxFactorX = 0.08f,
                    parallaxFactorY = 0.04f,
                    autoScrollX = true,
                    autoScrollSpeedX = 0.25f, // Mây lững lờ trôi
                    infiniteRepeatX = true,
                    textureUnitSizeX = 22.5f
                },
                new ParallaxBackground.ParallaxLayer
                {
                    layerName = "Mountains",
                    layerTransform = mountainFolder,
                    parallaxFactorX = 0.25f,
                    parallaxFactorY = 0.12f,
                    autoScrollX = false,
                    infiniteRepeatX = true,
                    textureUnitSizeX = 22.5f
                },
                new ParallaxBackground.ParallaxLayer
                {
                    layerName = "WaterfallGorge",
                    layerTransform = waterfallFolder,
                    parallaxFactorX = 0.50f,
                    parallaxFactorY = 0.30f,
                    autoScrollX = false,
                    infiniteRepeatX = false
                }
            };
            parallax.Initialize(mainCam, pLayers);

            // =========================================================================
            // 5. XÂY DỰNG TOÀN BỘ ĐỊA HÌNH RỪNG RẬM CHÍNH (PLAYFIELD TERRAIN - SORTING: 0)
            // =========================================================================
            Transform terrainFolder = new GameObject("Terrain_Jungle").transform;
            terrainFolder.SetParent(root.transform);

            // Bảng màu dự phòng nếu asset chưa tải xong
            Color barkColor = new Color(0.32f, 0.20f, 0.12f);
            Color foliageColor = new Color(0.12f, 0.42f, 0.22f);
            Color platformWood = new Color(0.40f, 0.26f, 0.16f);
            Color riverRockColor = new Color(0.22f, 0.28f, 0.26f);

            // -------------------------------------------------------------------------
            // KHU VỰC 1: BÌA RỪNG RẬM & BẬC THANG TÁN CÂY (X: -5 đến 45)
            // -------------------------------------------------------------------------
            // Vách thân cây cao bên trái cùng để tập leo tường (Wall Kick)
            CreateSpritePlatform(terrainFolder, "Trunk_Left_Boundary", new Vector2(-4.5f, 7f), new Vector2(2.5f, 20f), sTrunkL, barkColor);
            // Mặt đất bìa rừng (thân gỗ phủ rêu xanh mát)
            CreateSpritePlatform(terrainFolder, "Ground_Section_1", new Vector2(20f, -2.5f), new Vector2(48f, 1.6f), sGround, foliageColor);
            // Các cành cây bệ đỡ cao dần để làm quen nhảy & dash
            CreateSpritePlatform(terrainFolder, "Branch_Step_1", new Vector2(10f, 0.5f), new Vector2(7f, 0.9f), sBranch, platformWood);
            CreateSpritePlatform(terrainFolder, "Branch_Step_2", new Vector2(21f, 3.0f), new Vector2(7f, 0.9f), sBranch, platformWood);
            CreateSpritePlatform(terrainFolder, "Branch_Step_3", new Vector2(33f, 1.2f), new Vector2(6f, 0.9f), sBranch, platformWood);
            // Thân cây dọc giữa đường thử leo trèo
            CreateSpritePlatform(terrainFolder, "Tree_Trunk_Mid_1", new Vector2(28f, 7.0f), new Vector2(2.0f, 9f), sTrunkR, barkColor);

            // Dây leo tiền cảnh (Foreground Vines) tạo chiều sâu 2.5D
            CreateVinesDecoration(terrainFolder, sVines, new Vector3(6f, 3.5f, 0f), new Vector2(2.5f, 2.5f));
            CreateVinesDecoration(terrainFolder, sVines, new Vector3(18f, 6.0f, 0f), new Vector2(3.0f, 3.0f));
            CreateVinesDecoration(terrainFolder, sVines, new Vector3(32f, 4.0f, 0f), new Vector2(2.5f, 2.5f));

            // -------------------------------------------------------------------------
            // KHU VỰC 2: VỰC SÂU THÁC NƯỚC & TÁN CÂY CỔ THỤ TRÊN CAO (X: 45 đến 95)
            // -------------------------------------------------------------------------
            // Bệ đỡ vượt vực 1 (Đòi hỏi Dash-Jump để qua nếu không rơi xuống đáy thác)
            CreateSpritePlatform(terrainFolder, "Canopy_Bridge_1", new Vector2(58f, 0.2f), new Vector2(11f, 1.0f), sBranch, platformWood);
            // Đáy vực / Lòng sông đá thác nước
            CreateSpritePlatform(terrainFolder, "Chasm_Pit_Floor", new Vector2(68f, -5.5f), new Vector2(35f, 1.8f), sRiverRock, riverRockColor);
            // Tán cây cổ thụ tầng cao nhất (Giấu bình hồi máu)
            CreateSpritePlatform(terrainFolder, "High_Secret_Canopy", new Vector2(73f, 5.0f), new Vector2(12f, 1.0f), sBranch, platformWood);
            // Cành cây tiếp nối
            CreateSpritePlatform(terrainFolder, "Canopy_Bridge_2", new Vector2(87f, 1.8f), new Vector2(9f, 1.0f), sBranch, platformWood);

            CreateVinesDecoration(terrainFolder, sVines, new Vector3(56f, 3.0f, 0f), new Vector2(3.0f, 3.0f));
            CreateVinesDecoration(terrainFolder, sVines, new Vector3(85f, 4.5f, 0f), new Vector2(3.0f, 3.0f));

            // -------------------------------------------------------------------------
            // KHU VỰC 3: THÁP THÂN CÂY ĐẠI THỤ RỖNG (X: 95 đến 135)
            // Đoạn leo tháp thẳng đứng kinh điển của Web Spider: leo tường Wall Kick liên tục!
            // -------------------------------------------------------------------------
            // Mặt đất trong lòng cây
            CreateSpritePlatform(terrainFolder, "Hollow_Tree_Floor", new Vector2(115f, -2.5f), new Vector2(38f, 1.6f), sGround, barkColor);
            // Vách thân cây bên trái (cao tới Y = 22)
            CreateSpritePlatform(terrainFolder, "Great_Trunk_Wall_Left", new Vector2(96f, 9.5f), new Vector2(2.5f, 24f), sTrunkL, barkColor);
            // Vách thân cây bên phải (cao tới Y = 22)
            CreateSpritePlatform(terrainFolder, "Great_Trunk_Wall_Right", new Vector2(134f, 9.5f), new Vector2(2.5f, 24f), sTrunkR, barkColor);
            // Bậc thang nấm gỗ zig-zag bên trong thân cây rỗng
            CreateSpritePlatform(terrainFolder, "Trunk_Shelf_1", new Vector2(104f, 2.5f), new Vector2(9f, 0.9f), sShelf, platformWood);
            CreateSpritePlatform(terrainFolder, "Trunk_Shelf_2", new Vector2(126f, 6.8f), new Vector2(9f, 0.9f), sShelf, platformWood);
            CreateSpritePlatform(terrainFolder, "Trunk_Shelf_3", new Vector2(105f, 11.2f), new Vector2(9f, 0.9f), sShelf, platformWood);
            CreateSpritePlatform(terrainFolder, "Trunk_Shelf_4", new Vector2(125f, 15.5f), new Vector2(9f, 0.9f), sShelf, platformWood);
            // Cầu gỗ trên đỉnh ngọn cây thoát ra ngoài
            CreateSpritePlatform(terrainFolder, "Tree_Crown_Exit_Bridge", new Vector2(115f, 19.5f), new Vector2(36f, 1.0f), sBranch, foliageColor);

            // -------------------------------------------------------------------------
            // KHU VỰC 4: RỪNG SÂU TRƯỚC CỔNG TRÙM (X: 135 đến 168)
            // -------------------------------------------------------------------------
            CreateSpritePlatform(terrainFolder, "Descent_Branch_1", new Vector2(144f, 14.0f), new Vector2(11f, 1.0f), sBranch, platformWood);
            CreateSpritePlatform(terrainFolder, "Descent_Branch_2", new Vector2(154f, 6.5f), new Vector2(10f, 1.0f), sBranch, platformWood);
            CreateSpritePlatform(terrainFolder, "Outpost_Ground", new Vector2(162f, -2.5f), new Vector2(16f, 1.6f), sGround, foliageColor);
            // Bức tường khung cổng Boss
            CreateSpritePlatform(terrainFolder, "Boss_Gate_Wall_Top", new Vector2(168f, 8.5f), new Vector2(2.5f, 11f), sTrunkL, barkColor);
            CreateSpritePlatform(terrainFolder, "Boss_Gate_Wall_Bottom", new Vector2(168f, -2.5f), new Vector2(2.5f, 1.6f), sGround, barkColor);

            // -------------------------------------------------------------------------
            // KHU VỰC 5: ĐẤU TRƯỜNG TRÙM WEB SPIDER (X: 170 đến 218)
            // -------------------------------------------------------------------------
            Transform bossArenaFolder = new GameObject("Boss_Arena_SpiderLair").transform;
            bossArenaFolder.SetParent(root.transform);
            CreateSpritePlatform(bossArenaFolder, "Spider_Lair_Floor", new Vector2(193f, -2.5f), new Vector2(48f, 1.6f), sGround, new Color(0.18f, 0.25f, 0.20f));
            CreateSpritePlatform(bossArenaFolder, "Spider_Lair_Ceiling", new Vector2(193f, 15.5f), new Vector2(48f, 2.0f), sGround, barkColor);
            CreateSpritePlatform(bossArenaFolder, "Spider_Lair_RightWall", new Vector2(217f, 6.5f), new Vector2(2.5f, 20f), sTrunkR, barkColor);

            // Tơ nhện trang trí chính gốc MMX4 trên trần đấu trường
            CreateWebDecoration(bossArenaFolder, sWeb, new Vector3(178f, 14f, 0f), new Vector2(8f, 2f));
            CreateWebDecoration(bossArenaFolder, sWeb, new Vector3(205f, 14f, 0f), new Vector2(8f, 2f));
            CreateWebDecoration(bossArenaFolder, sWeb, new Vector3(216f, 12f, 0f), new Vector2(4f, 4f));

            // =========================================================================
            // 6. CỬA PHÒNG BOSS (BOSS SHUTTER) TẠI X = 168 (SPRITE CHUẨN MMX)
            // =========================================================================
            GameObject doorObj = new GameObject("Boss_Shutter_Door");
            doorObj.transform.SetParent(root.transform);
            doorObj.transform.position = new Vector3(168f, 0.75f, 0f);

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
            // 7. TẠO PLAYER (MEGA MAN X)
            // =========================================================================
            GameObject player = new GameObject("Player_MegaManX");
            player.tag = "Player";
            player.transform.SetParent(root.transform);
            player.transform.position = new Vector3(0f, 0f, 0f);

            Rigidbody2D pRb = player.AddComponent<Rigidbody2D>();
            BoxCollider2D pCol = player.AddComponent<BoxCollider2D>();
            pCol.size = new Vector2(0.85f, 1.6f);
            pCol.offset = new Vector2(0f, 0.8f);

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
            // 8. PHÂN BỐ KẺ ĐỊCH RỪNG RẬM (ENEMIES)
            // =========================================================================
            Transform enemiesFolder = new GameObject("Enemies_Jungle").transform;
            enemiesFolder.SetParent(root.transform);

            CreatePatrolEnemy(enemiesFolder, new Vector3(14f, -1.5f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(32f, -1.5f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(58f, 1.0f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(115f, -1.5f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(126f, 7.5f, 0f));
            CreatePatrolEnemy(enemiesFolder, new Vector3(160f, -1.5f, 0f));

            CreateFlyingHornet(enemiesFolder, new Vector3(22f, 4.5f, 0f));
            CreateFlyingHornet(enemiesFolder, new Vector3(52f, 3.5f, 0f));
            CreateFlyingHornet(enemiesFolder, new Vector3(76f, 7.5f, 0f));
            CreateFlyingHornet(enemiesFolder, new Vector3(112f, 13.0f, 0f));
            CreateFlyingHornet(enemiesFolder, new Vector3(148f, 8.5f, 0f));

            // =========================================================================
            // 9. VẬT PHẨM HỒI PHỤC (HEALTH PICKUPS)
            // =========================================================================
            Transform itemsFolder = new GameObject("Pickups").transform;
            itemsFolder.SetParent(root.transform);
            CreateHealthCapsule(itemsFolder, new Vector3(73f, 6.2f, 0f)); // Bình máu giấu trên tán cây cao!
            CreateHealthCapsule(itemsFolder, new Vector3(105f, 12.4f, 0f)); // Bình máu trong thân cây đại thụ

            // =========================================================================
            // 10. TẠO TRÙM WEB SPIDER (JUNGLE SOVEREIGN)
            // =========================================================================
            GameObject bossObj = new GameObject("Boss_WebSpider");
            bossObj.tag = "Boss";
            bossObj.transform.SetParent(bossArenaFolder);
            bossObj.transform.position = new Vector3(196f, 0f, 0f);

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
            // 11. TẠO GIAO DIỆN MÁU (HUD CANVAS)
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
            // 12. TẠO BOSS ROOM TRIGGER (TẠI X = 170)
            // =========================================================================
            GameObject triggerObj = new GameObject("Boss_Room_Trigger");
            triggerObj.transform.SetParent(root.transform);
            triggerObj.transform.position = new Vector3(170f, 0.5f, 0f);

            BoxCollider2D trigCol = triggerObj.AddComponent<BoxCollider2D>();
            trigCol.isTrigger = true;
            trigCol.size = new Vector2(2f, 5f);

            BossRoomTrigger roomTrigger = triggerObj.AddComponent<BossRoomTrigger>();
            roomTrigger.Initialize(bossController, bossDoor, camController, bossHealthBar, new Vector2(193f, 4.5f), new Vector2(193f, 4.5f));

            Debug.Log("<color=green>[MMX4 Jungle Stage]</color> Đã tái thiết kế Bản đồ Rừng rậm Web Spider với Sprite & Backgrounds gốc từ spriters-resource.com thành công!");
        }

        private void BuildRepeatingBackground(Transform parent, Sprite sprite, Color fallbackColor, int sortingOrder, Vector2 startPos, float totalWidth, float stepX, float stepY)
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
                cliffObj.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
            }

            if (sWf != null)
            {
                for (int i = -1; i <= 1; i++)
                {
                    GameObject wfObj = new GameObject($"Waterfall_Cascade_{i}");
                    wfObj.transform.SetParent(parent);
                    wfObj.transform.position = new Vector3(centerPos.x + i * 5.5f, centerPos.y - 1f, 0f);
                    SpriteRenderer srWf = wfObj.AddComponent<SpriteRenderer>();
                    srWf.sprite = sWf;
                    srWf.sortingOrder = -20;
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

        private void CreateSpritePlatform(Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite, Color fallbackColor)
        {
            GameObject plat = new GameObject(name);
            plat.transform.SetParent(parent);
            plat.transform.position = pos;

            SpriteRenderer sr = plat.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : CreateSimpleSprite(fallbackColor);
            sr.sortingOrder = 0;

            // Thiết lập kích thước hiển thị đồng bộ với Collider
            plat.transform.localScale = new Vector3(size.x, size.y, 1f);

            BoxCollider2D col = plat.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
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
