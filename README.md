# Mega Man X4 (Unity 2D) - Web Spider Jungle Stage

Dự án tái hiện hệ thống gameplay hành động cổ điển và cấu trúc màn chơi rừng rậm của trùm **Web Spider** trong **Mega Man X4** (Capcom) trên nền tảng **Unity 2D** (Tương thích hoàn toàn Unity 2022 LTS và Unity 6).

---

## 🎮 1. Bảng điều khiển (Controls)

| Thao tác | Phím Bàn phím | Chi tiết cơ chế |
| :--- | :---: | :--- |
| **Di chuyển** | **Chỉ phím mũi tên** (`←` / `→`) | Nhấn phím mũi tên Trái / Phải để chạy |
| **Nhảy / Đạp tường** | **`X`** | Nhấn nhẹ nhảy thấp, giữ nhảy cao; khi bám tường bấm `X` để đạp tường (Wall Kick) |
| **Leo tường liên tục** | **Giữ mũi tên vào tường + nhấp `X`** | Leo thoăn thoắt lên đỉnh thân cây đại thụ |
| **Lướt (Dash / Air Dash)** | **`Z`** | Lướt đất & lướt trên không; nhấn `X` trong khi lướt để **Dash-Jump** |
| **Bắn / Tích năng lượng** | **`C`** | Nhấn nhả bắn đạn chanh (Lemon); giữ để sạc đạn cấp 1 (Semi-Charge) & cấp 2 (Full-Charge Plasma) |
| **Trượt tường (Wall Slide)** | **Áp sát vào tường khi đang rơi** | Tự động hãm tốc độ trượt từ từ kèm tia lửa ma sát |

---

## 🌲 2. Quy mô Màn chơi Rừng rậm Web Spider (>215m - 5 Phân khu)

Bản đồ được mở rộng chiều dài lên tới **hơn 215 đơn vị (units)** với các tầng cao độ từ Y = -6 lên tới Y = 22:

1. **Phân khu 1: Bìa rừng rậm & Tán cây thấp (X: -5 đến 45)**:
   - Thân cây đại thụ làm quen với cơ chế nhảy, lướt và bám tường.
   - Kẻ địch: Bọ bọc giáp bò tuần tra và Ong máy bay lượn.
2. **Phân khu 2: Vực sâu & Tán cây cổ thụ tầng cao (X: 45 đến 95)**:
   - Vực sâu ngăn cách đòi hỏi kỹ thuật **Dash-Jump** (`Z` + `X`).
   - Bình năng lượng lớn (**Health Capsule**) ẩn giấu trên tán cây cao tại X = 73, Y = 6.2.
3. **Phân khu 3: Tháp thân cây rỗng khổng lồ (X: 95 đến 135)**:
   - Tái hiện trường đoạn leo tháp cao tới Y = 22 bằng kỹ thuật đạp tường liên tục.
4. **Phân khu 4: Rừng sâu & Cửa cuốn phòng trùm (X: 135 đến 168)**:
   - Địch phòng thủ dày đặc trước cổng.
   - **Cửa cuốn phòng trùm (Boss Shutter Door)**: Tự động trượt mở khi tiếp cận và khóa kín khi bước qua.
5. **Phân khu 5: Hang ổ Trùm Web Spider (X: 170 đến 218)**:
   - Đấu trường khổng lồ cao 16m trong lòng cây cổ thụ.
   - **Trùm Web Spider**:
     - **Lightning Web**: Phóng chùm 3 cầu mạng nhện sấm sét nan quạt.
     - **Silk Drop Slam**: Nhảy vút lên trần nhà đu tơ rơi bổ nhào dập đất.
     - **Spider Scuttle Dash**: Bò trườn thần tốc càn quét mặt đất.
     - Thanh máu Boss nạp đầy từ dưới lên và hiệu ứng nổ kết liễu hoành tráng.

---

## 🎨 3. Hoạt ảnh Sprite Mega Man X (32-bit PS1 MMX4)

Nhân vật Mega Man X được cấu hình đầy đủ bộ sprite 32-bit trích xuất từ asset chính thức:
- **Idle**: Dáng đứng thở và chớp mắt (5 frames).
- **Run & Run Shoot**: Chu kỳ chạy bộ 8 frames đồng bộ nhịp bắn buster.
- **Dash & Dash Shoot**: Dáng trượt thấp sát mặt đất.
- **Jump & Fall**: Bật nhảy, điểm cực đại, rơi xuống và tiếp đất.
- **Wall Slide & Wall Slide Shoot**: Bám tường với tia lửa ma sát.
- **Buster Projectiles**: Đạn chanh vàng, đạn tụ cấp 2, Plasma shot cấp 3.

---

## 🚀 4. Hướng dẫn Chạy dự án trong Unity

1. Mở **Unity Hub** -> Add Project -> Chọn thư mục `D:\MGMX`.
2. Mở Scene chính của dự án.
3. Nếu scene chưa có môi trường, chọn GameObject chứa script `MMXDemoSetup` -> Chuột phải chọn **"Build MMX4 Demo Scene"** (hoặc chỉ cần nhấn nút **Play ▶**).
