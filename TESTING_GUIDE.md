# Test Instructions - Cultivation Lightning Turret

## 🎯 Testing Steps

### 1. Start Game & Enable Mod
- Khởi động RimWorld
- Đảm bảo Tu Tien mod được enable
- Tạo colony mới hoặc load save có sẵn

### 2. Research Phase
- Mở Research tab
- Tìm "Lôi Trận Phòng Thủ" (Cultivation Defense)
- Research nó (cần có GunTurrets trước)
- Chi phí: 2000 research points

### 3. Build Turret
- Mở Architect → Security
- Tìm "lôi trận phòng thủ" (cultivation lightning turret)
- Chọn vật liệu và xây dựng
- Chi phí: 120 material + 70 Steel + 6 Component

### 4. Test Qi System
- **Tạo người tu luyện**: Ensure có pawn với CultivationComp
- **Manual charge**: Click turret → "Nạp Qi" button
- **Auto charge**: Đặt work priority "Hauling" cho người tu luyện
- **Qi bar**: Kiểm tra cyan bar trên turret

### 5. Combat Test
- Spawn enemies: Dev mode → spawn raiders
- Turret tự động target và bắn
- Quan sát:
  - Lightning visual effects
  - Qi consumption (50/shot)
  - Electric stun on pawns
  - Burst fire (3 shots)

## 🔍 Expected Behavior

### ✅ Working Correctly:
- Turret appears in Security tab after research
- Can be built with correct materials
- Shows Qi bar (cyan) when selected
- "Nạp Qi" button appears when Qi < 1000
- Cultivators automatically charge when work assigned
- Fires lightning projectiles that consume 50 Qi
- Stops firing when Qi depleted
- Visual effects on impact
- Enemies get stunned by electric damage

### ❌ Issues to Check:
- Turret not appearing in build menu
- Research not unlocking turret
- Qi bar not visible
- No charging animation/effects
- Projectiles not firing
- No Qi consumption
- Missing visual/sound effects

## 🛠️ Debug Commands
```
// Dev mode commands for testing
~teleport                    // Quick travel
~instant build              // Build instantly  
~research all               // Unlock all research
~spawn [item] [count]       // Spawn items
~damage [pawn] [amount]     // Damage for testing
```

## 📊 Performance Monitoring
- Check for lag when multiple turrets fire
- Monitor Qi calculation overhead
- Verify save/load compatibility
- Test with different cultivation levels
