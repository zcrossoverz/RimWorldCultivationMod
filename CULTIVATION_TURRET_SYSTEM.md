# Hệ thống Lôi Trận Phòng Thủ - Tu Tiên Mod

## Tổng quan
Hệ thống phòng thủ đơn giản sử dụng năng lượng tu luyện để tạo ra những tia sét mạnh mẽ bảo vệ khu định cư.

## Các thành phần đã tạo

### 1. XML Definitions
- `ThingDefs_CultivationTurret.xml`: Định nghĩa turret và súng tia sét
- `ProjectileDefs_CultivationTurret.xml`: Định nghĩa đạn tia sét
- `JobDefs_CultivationTurret.xml`: Job nạp Qi cho turret
- `ResearchProjectDefs_CultivationTurret.xml`: Research để mở khóa turret
- `WorkGiverDefs_CultivationTurret.xml`: Auto work cho việc nạp Qi

### 2. C# Classes
- `CompQiStorage.cs`: Component quản lý Qi
- `Building_CultivationTurret.cs`: Turret chính với UI và logic
- `Verb_CultivationLightning.cs`: Verb bắn tia sét tiêu thụ Qi
- `Projectile_CultivationLightning.cs`: Đạn tia sét với hiệu ứng đặc biệt
- `JobDriver_ChargeCultivationTurret.cs`: AI driver cho việc nạp Qi
- `WorkGiver_ChargeCultivationTurret.cs`: Tự động gán job nạp Qi

## Cách hoạt động

### Xây dựng turret
1. Research "Lôi Trận Phòng Thủ" (cần GunTurrets trước)
2. Xây turret trong tab Security
3. Chi phí: 120 vật liệu + 70 Steel + 6 Component Industrial

### Sử dụng turret
1. **Nạp Qi**: 
   - Tự động: Người tu luyện sẽ tự động nạp Qi khi cần
   - Thủ công: Click vào turret và chọn "Nạp Qi"
   - Yêu cầu: Người nạp Qi phải có cultivation realm > Mortal

2. **Bắn**:
   - Tự động bắn khi có kẻ thù
   - Tiêu thụ 50 Qi mỗi phát
   - Tầm bắn: 28.9 tiles
   - Bắn burst 3 phát

3. **Quản lý Qi**:
   - Qi tối đa: 1000
   - Qi bar hiển thị ở trên turret (màu cyan)
   - Không thể bắn khi hết Qi

### Đặc điểm
- **Sát thương**: 25 Burn damage + explosion radius 1.9
- **Hiệu ứng**: Lightning visual + electric stun cho pawn
- **Độ chính xác**: Cao ở tầm gần, giảm ở tầm xa
- **Cooldown**: 4.8s giữa các burst

## Cân bằng
- Cần người tu luyện để vận hành → khuyến khích phát triển cultivation
- Tiêu thụ Qi → không thể spam attack
- Chi phí xây dựng hợp lý
- Tầm bắn tương đương turret thường

## Mở rộng tương lai
- Thêm nhiều loại turret khác (fire, ice, earth)
- Upgrade system cho turret
- Qi storage buildings riêng biệt
- Formation turret system
