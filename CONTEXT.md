# Mega Man X4 (Unity 2D) - Context & Architectural Model

## 1. Domain Glossary (Thuật ngữ cốt lõi)

- **X-Buster**: Vũ khí cơ bản của Mega Man X. Có khả năng bắn đạn thường (Lemon) và tích tụ năng lượng (Charge).
- **Charge Level (Cấp độ sạc)**:
  - **Level 0 (Uncharged)**: Đạn chanh nhỏ (1 sát thương). Tối đa 3 viên trên màn hình cùng lúc.
  - **Level 1 (Semi-Charge)**: Đạn xanh lục vừa (2 sát thương), tốc độ và diện tích va chạm lớn hơn.
  - **Level 2 (Full Charge / Plasma Shot)**: Đại bác năng lượng xanh dương/hồng đặc trưng của X4 (4 sát thương), uy lực cực mạnh.
- **Dash (Lướt)**: Động tác lướt nhanh trên mặt đất với vận tốc gấp đôi chạy thường, có thể lướt trên không (Air Dash).
- **Dash Jump (Nhảy lướt)**: Giữ nguyên đà vận tốc (momentum) của Dash suốt toàn bộ quỹ đạo nhảy cho đến khi chạm đất.
- **Wall Slide (Trượt tường)**: Giảm tốc độ rơi khi bám vào vách tường thẳng đứng.
- **Wall Kick / Wall Jump (Đạp tường nhảy)**: Bật ngược ra khỏi tường theo phương chéo lên trên, cho phép leo tường liên tục.
- **I-Frames (Invulnerability Frames)**: Trạng thái bất tử tạm thời sau khi dính sát thương (nhân vật nhấp nháy).
- **Knockback (Độ giật/Bật lùi)**: Lực đẩy lùi ngắn khi trúng đòn làm ngắt động tác.
- **Boss Shutter / Boss Door (Cửa Boss)**: Cửa chia cắt màn chơi thường và phòng trùm. Khi người chơi bước qua, cửa đóng lại, camera khóa cứng và bắt đầu sequence Boss.
- **Boss Intro Sequence**: Boss rơi xuống/xuất hiện -> Boss tạo dáng (Roar/Pose) -> Cột máu Boss nạp đầy từ dưới lên -> Nhạc nổi lên và trận đấu bắt đầu.

---

## 2. Architectural Decisions (ADR)

### ADR-001: Bộ điều khiển vật lý (Kinematic vs Rigidbody2D)
- **Quyết định**: Sử dụng `Rigidbody2D` ở chế độ `Interpolate` kết hợp với raycast kiểm tra tiếp đất (`GroundCheck`) và tường (`WallCheck`), điều khiển trực tiếp `velocity` thay vì dùng lực (AddForce).
- **Lý do**: Mega Man X yêu cầu phản hồi tức thời (instant acceleration), nhảy dừng đột ngột khi nhả nút (variable jump height), trượt tường có ma sát cố định và dash tức thì. Việc gán vận tốc trực tiếp qua Rigidbody2D đem lại cảm giác điều khiển chuẩn xác 100% như trên máy PS1/SNES, đồng thời vẫn tương thích hoàn hảo với hệ thống vật lý va chạm và Trigger 2D của Unity.

### ADR-002: State Machine cho Player & Boss
- **Quyết định**: Sử dụng mô hình Finite State Machine (FSM) phân tách rõ ràng các trạng thái (`Idle`, `Running`, `Jumping`, `Falling`, `Dashing`, `WallSliding`, `Hurt`).
- **Lý do**: Tránh tình trạng "spaghetti if-else" khi xử lý các pha phức tạp như Dash-Jump chuyển sang Wall-Slide hoặc bị trúng đòn giữa không trung.

### ADR-003: Hệ thống đạn và sát thương (IDamageable)
- **Quyết định**: Giao diện `IDamageable` trừu tượng hóa việc nhận sát thương cho cả Player, Quái và Boss.
- **Lý do**: Đạn bắn không cần biết mục tiêu cụ thể là Boss hay quái thường, giúp hệ thống dễ dàng mở rộng khi bổ sung thêm vũ khí đặc biệt (Special Weapons) sau này.

### ADR-004: Tự sinh Placeholder Graphics & Demo Scene (Zero External Assets Required)
- **Quyết định**: Tạo script sinh Sprite 2D dạng hình học (box, capsule, circle) với màu sắc đặc trưng ngay trong runtime/editor, đồng thời cung cấp script tự động tạo Level Demo hoàn chỉnh (Ground, Walls, Enemy, Boss Door, Boss Room, UI Canvas).
- **Lý do**: Người dùng có thể bấm Play và chơi thử ngay lập tức mà không cần chuẩn bị sprite asset từ trước. Tất cả Prefab và Component đều có sẵn Inspector slot cho SpriteRenderer, Animator và VFX Prefab để thay thế sau này.

### ADR-005: Thiết lập phím điều khiển (Key Bindings)
- **Quyết định**:
  - Di chuyển: Chỉ dùng phím mũi tên Trái/Phải (`KeyCode.LeftArrow`, `KeyCode.RightArrow`).
  - Nhảy (Jump / Wall Kick): Phím `X` (`KeyCode.X`).
  - Lướt (Dash / Air Dash): Phím `Z` (`KeyCode.Z`).
  - Bắn / Tích năng lượng (Buster / Charge): Phím `C` (`KeyCode.C`).
- **Lý do**: Mô phỏng bố cục tay cầm kinh điển (ngón trỏ/giữa/cái bên tay phải đặt tự nhiên lên Z, X, C để vừa giữ sạc C, vừa bấm Z để Dash và X để Jump cùng lúc). Tay trái chuyên trách di chuyển mũi tên.
