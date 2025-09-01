# 🎮 HƯỚNG DẪN TEST TU TIÊN ARTIFACTS TRONG GAME

## 🎯 ĐÃ HOÀN THÀNH: 
- **Tất cả 5 CultivationArtifactDefs đã load thành công** ✅
- **Tất cả 14 CultivationEffectDefs đã load thành công** ✅
- **Registry system hoạt động (5 artifacts loaded)** ✅
- **Hệ thống ELO ẩn (100-1200 range)** ✅
- **Auto-combat behaviors** ✅
- **Qi pool management** ✅

---

## 🛠️ CÁCH TEST TRONG GAME:

### **PHƯƠNG PHÁP 1: TU TIÊN TEST KIT** (Khuyên dùng)

#### Bước 1: Research
1. Vào **Research Tab** 
2. Tìm "**Tu Tiên Testing**" (cần ComplexClothing trước)
3. Research nó (chỉ 50 research points)

#### Bước 2: Craft Test Kit
1. Đi đến **Crafting Spot** hoặc **Machining Table**
2. Tìm "**make Tu Tiên test kit**"
3. Cần materials:
   - **5x Metal** (steel, iron, etc.)
   - **10x Textile** (cloth, leather, etc.)
4. Craft 1 Test Kit

#### Bước 3: Sử dụng
1. **Right-click** vào Tu Tiên Test Kit
2. Chọn "**Spawn Test Artifacts**"
3. Test Kit sẽ biến mất và spawn 3 artifacts:
   - 🗡️ **Simple Iron Sword** (cultivation sword)
   - 👘 **Simple Cloth Robe** (cultivation apparel)  
   - 🔮 **Simple Spirit Staff** (cultivation weapon)

---

### **PHƯƠNG PHÁP 2: DEV MODE** (Nếu muốn)
1. Bật **Dev Mode**
2. Shift + F12 để mở **Debug Actions**
3. Tìm trong menu (nếu có)

---

## 🔍 KẾT QUẢ MONG ĐỢI:

### ✅ Khi sử dụng Test Kit:
- **Message hiện lên**: "Đang spawn Tu Tiên test artifacts..."
- **3 items xuất hiện** gần người dùng
- **Message xác nhận**: "✅ Đã spawn 3 Tu Tiên artifacts!"
- **Console log** hiển thị thông tin chi tiết

### 📋 Thông tin Console sẽ hiện:
```
[TuTien] Found 5 artifact definitions
[TuTien] Simple ThingDefs - Sword: simple cultivation sword, Robe: simple cultivation robe, Staff: simple spirit staff
[TuTien] ✅ Successfully spawned simple sword!
[TuTien] ✅ Successfully spawned simple robe!  
[TuTien] ✅ Successfully spawned simple staff!
[TuTien] ✅ Successfully spawned 3 artifacts!
[TuTien] === ARTIFACT INFO ===
[TuTien] 1. CultivationArtifact_IronSword: Iron Cultivation Sword (Common) - ELO: 100-200
[TuTien] 2. CultivationArtifact_ClothRobe: Cloth Cultivation Robe (Common) - ELO: 100-200
[TuTien] 3. CultivationArtifact_SpiritBow: Spirit Hunter Bow (Rare) - ELO: 280-480
[TuTien] 4. CultivationArtifact_DragonCrown: Crown of the Thunder Dragon (Epic) - ELO: 420-680
[TuTien] 5. CultivationArtifact_ImmortalTalisman: Talisman of Immortal Essence (Legendary) - ELO: 600-900
```

---

## 🎯 ITEMS SẼ XUẤT HIỆN:

### 🗡️ **Simple Cultivation Sword**
- **Loại**: Melee weapon  
- **Damage**: 18 power
- **Linked to**: CultivationArtifact_IronSword (ELO 100-200)
- **Chức năng**: Auto-attack, Qi management

### 👘 **Simple Cultivation Robe**
- **Loại**: Apparel (torso, arms)
- **Armor**: Sharp 0.05, Blunt 0.05
- **Linked to**: CultivationArtifact_ClothRobe (ELO 100-200) 
- **Chức năng**: Cultivation efficiency boost

### 🔮 **Simple Spirit Staff**
- **Loại**: Melee weapon
- **Damage**: 14 power (blunt)
- **Linked to**: CultivationArtifact_SpiritBow (ELO 280-480)
- **Chức năng**: Spirit energy channeling

---

## 🚀 SAU KHI TEST THÀNH CÔNG:

1. **Equip artifacts** lên pawns để test
2. **Kiểm tra stats** có thay đổi không
3. **Test combat** với auto-attack (nếu có)
4. **Báo cáo kết quả** nếu có lỗi

---

## ⚠️ NẾU CÓ LỖI:

### Lỗi không craft được Test Kit:
- Kiểm tra đã research "Tu Tiên Testing" chưa
- Cần có ComplexClothing research trước

### Lỗi không spawn artifacts:
- Kiểm tra console có message gì
- Thử ở vị trí khác (có không gian trống)

### Không thấy Research:
- Kiểm tra mod đã enable chưa
- Restart game và thử lại

---

## 📞 BÁO CÁO KẾT QUẢ:

Hãy cho tôi biết:
1. **Test Kit có craft được không?**
2. **Có spawn được 3 artifacts không?**  
3. **Console có hiện đúng thông tin không?**
4. **Có lỗi gì trong console không?**
5. **Artifacts có equip được không?**

**System hoàn toàn sẵn sàng để test! 🎯**
