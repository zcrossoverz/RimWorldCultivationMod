Tu Tiên Cultivation Stats Configuration Guide
============================================

File để chỉnh sửa: CultivationStageStatsDefs.xml

Cách thêm stage mới:
1. Copy một block CultivationStageStatsDef từ file CultivationStageStatsDefs.xml
2. Thay đổi defName, realm, stage 
3. Điều chỉnh các stats theo ý muốn
4. Save và reload mod

Giải thích các stats:

Core Cultivation:
- maxQi: Lượng qi tối đa
- qiRegenRate: Tốc độ hồi qi (mỗi giây)
- breakthroughChance: Tỷ lệ đột phá cơ bản (0.0-1.0)
- tuViGainMultiplier: Tốc độ tăng tu vi (1.0 = bình thường)

Combat Stats:
- maxHpMultiplier: Nhân HP (1.5 = 150% HP)
- meleeDamageMultiplier: Nhân sát thương cận chiến
- meleeHitChanceOffset: Bonus độ chính xác (0.1 = +10%)
- rangedDamageMultiplier: Nhân sát thương tầm xa
- armorRatingSharpMultiplier: Nhân giáp chém
- armorRatingBluntMultiplier: Nhân giáp đập

Physical Stats:
- moveSpeedOffset: Bonus tốc độ di chuyển
- carryingCapacityMultiplier: Nhân sức mang vác
- workSpeedGlobalMultiplier: Nhân tốc độ làm việc
- injuryHealingFactorMultiplier: Nhân tốc độ hồi phục

Biological Stats:
- hungerRateMultiplier: Tỷ lệ đói (0.8 = ít đói hơn 20%)
- restRateMultiplier: Tỷ lệ ngủ (0.8 = ít ngủ hơn 20%)
- immunityGainSpeedMultiplier: Tốc độ tăng miễn dịch

Mental Stats:
- mentalBreakThresholdMultiplier: Kháng breakdown (0.8 = khó breakdown hơn 20%)
- moodOffset: Bonus mood (+5.0 = +5 mood)

Realms có sẵn:
- Mortal (3 stages)
- QiCondensation (9 stages)
- FoundationEstablishment (9 stages)
- GoldenCore (9 stages)

Ví dụ tạo stage mạnh:
<TuTien.CultivationStageStatsDef>
  <defName>QiCondensationStage3</defName>
  <realm>QiCondensation</realm>
  <stage>3</stage>
  <label>Qi Condensation Stage 3</label>
  
  <maxQi>400</maxQi>
  <qiRegenRate>1.0</qiRegenRate>
  <tuViGainMultiplier>1.5</tuViGainMultiplier>
  
  <maxHpMultiplier>1.40</maxHpMultiplier>
  <meleeDamageMultiplier>1.40</meleeDamageMultiplier>
  <moveSpeedOffset>0.6</moveSpeedOffset>
  <workSpeedGlobalMultiplier>1.25</workSpeedGlobalMultiplier>
  
  <hungerRateMultiplier>0.90</hungerRateMultiplier>
  <moodOffset>5.0</moodOffset>
</TuTien.CultivationStageStatsDef>
