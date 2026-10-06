#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MMX.Setup
{
    [CustomEditor(typeof(MMXDemoSetup))]
    public class MMXDemoSetupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Vẽ các thuộc tính mặc định (autoBuildOnPlay)
            DrawDefaultInspector();

            MMXDemoSetup setup = (MMXDemoSetup)target;

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("CÔNG CỤ TẠO & CHỈNH SỬA MAP THỦ CÔNG", EditorStyles.boldLabel);

            // Nút 1: Sinh toàn bộ Map ra Scene để chỉnh tay
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
            if (GUILayout.Button("🔨 SINH TOÀN BỘ MAP RA SCENE (ĐỂ CHỈNH BẰNG TAY)", GUILayout.Height(38)))
            {
                if (EditorUtility.DisplayDialog("Xác nhận tạo Map", 
                    "Thao tác này sẽ sinh toàn bộ cây thư mục GameObjects của map Web Spider (7 khu vực, colliders, parallax, boss) trực tiếp ra Scene để bạn có thể kéo thả, chỉnh sửa thủ công bằng chuột trong Scene View.\n\nTiếp tục?", "Tạo Map", "Hủy"))
                {
                    Undo.RegisterFullObjectHierarchyUndo(setup.gameObject, "Build Authentic Stage");
                    setup.BuildFullDemoStage();

                    // Tự động tắt autoBuildOnPlay để giữ nguyên các chỉnh sửa thủ công của bạn khi Play
                    SerializedProperty autoBuildProp = serializedObject.FindProperty("autoBuildOnPlay");
                    if (autoBuildProp != null)
                    {
                        autoBuildProp.boolValue = false;
                        serializedObject.ApplyModifiedProperties();
                    }

                    EditorSceneManager.MarkSceneDirty(setup.gameObject.scene);
                    Debug.Log("<color=green>[MMXDemoSetup] Đã sinh toàn bộ Map ra Scene! Bạn có thể chỉnh sửa thủ công ngay bây giờ.</color>");
                }
            }

            EditorGUILayout.Space(6);

            // Nút 2: Xóa Map trên Scene
            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
            if (GUILayout.Button("🗑️ XÓA MAP TRÊN SCENE", GUILayout.Height(28)))
            {
                GameObject env = GameObject.Find("MMX_Demo_Environment");
                if (env != null)
                {
                    Undo.DestroyObjectImmediate(env);
                    EditorSceneManager.MarkSceneDirty(setup.gameObject.scene);
                    Debug.Log("[MMXDemoSetup] Đã xóa MMX_Demo_Environment khỏi Scene.");
                }
                else
                {
                    EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy GameObject 'MMX_Demo_Environment' trên Scene hiện tại.", "OK");
                }
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(12);
            EditorGUILayout.HelpBox(
                "📋 HƯỚNG DẪN CHỈNH MAP THỦ CÔNG TRONG UNITY:\n\n" +
                "1. Bấm nút [SINH TOÀN BỘ MAP RA SCENE] ở trên.\n" +
                "2. Mở cửa sổ Hierarchy -> Chọn 'MMX_Demo_Environment' -> 'Authentic_WebSpider_Stage'.\n" +
                "3. Trong thư mục mỗi Part (ví dụ: Colliders_Part1, Colliders_Part2...):\n" +
                "   - Chọn từng sàn 'Floor_...' hoặc tường 'Wall_...'.\n" +
                "   - Nhìn sang cửa sổ Inspector -> Tìm 'Box Collider 2D' -> Bấm nút 'Edit Collider' (icon 4 dấu chấm xanh) để kéo dãn sàn/tường trực quan bằng chuột trong Scene View!\n" +
                "4. Kéo thêm vật thể/trang trí:\n" +
                "   - Kéo trực tiếp các Sprite từ thư mục 'Assets/Resources/Environment/Jungle/Terrain' vào Scene View.\n" +
                "5. Di chuyển vị trí Nhân vật / Quái vật / Boss:\n" +
                "   - Chọn đối tượng trên Hierarchy -> dùng phím tắt W (Move Tool) để kéo đến vị trí mong muốn.\n" +
                "6. Lưu Scene (Ctrl + S) và bấm nút Play để trải nghiệm!",
                MessageType.Info);
        }
    }
}
#endif
