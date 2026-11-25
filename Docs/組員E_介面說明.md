# 組員 E：經濟、被動技能與 UI 系統：給隊友的介面說明

這份說明列出其他組員要「呼叫什麼」或「監聽什麼」，才能和組員 E 的系統接起來。
所有方法都先檢查 `Instance != null`，這樣場景裡沒有 E 的系統也不會報錯。

---

## 組員 A（玩家）

### 血量：不用改程式

E 的 `PlayerHealthDisplay` 會直接讀 `PlayerController` 現在的兩個欄位：

| 欄位 | 用途 |
|---|---|
| `maxHealth` | 最大血量 |
| `PlayercurrentHealth` | 目前血量 |

它會自動做這些事，**A 不用寫任何程式**：

- 血量變了 → 更新愛心、血條、數字
- 買到加血上限的被動 → 改 `maxHealth`，多出來的那格補滿
- 買到補血道具 → 加到 `PlayercurrentHealth`（滿血時商店不讓玩家買）
- 血量歸零 → 打開失敗畫面

**請 A 注意三件事：**

1. **如果要改這兩個欄位的名稱，先跟 E 說。** E 只要在 Inspector 改名稱就好，不用改程式。
2. 玩家物件的 Tag 要是 **Player**（EnemyController 本來就靠這個 Tag 判斷）。
3. **不要另外接 `PlayerHealing`**，不然補血會補兩次。

### 血量是「格」，不是 100 滴

玩家的血量上限由 E 的 `PlayerStats` 決定（基礎 **3 格**，買被動可以加），進遊戲時會覆蓋 Prefab 上的 100。
所以**敵人的傷害要用「格」來算**：`contactDamage = 1` 就是扣一格。

被打到之後有 **1 秒無敵時間**（`PlayerController.invincibleTime`），期間玩家會閃紅色，
這樣被一群敵人同時碰到也不會一瞬間扣光。

### 移動速度：已經接好（integration）

`PlayerController.CurrentSpeed` = `PlayerSpeed` × 移動速度被動加成 × 地形（站在水裡 × 0.5）。
A 調 `PlayerSpeed` 就是調基本速度，被動能力會照比例加上去。

---

## 組員 B（武器）

### 算傷害（含攻擊力加成與爆擊）

**已經在 `integration` 接好了**：`WeaponHitBox`（刀）和 `ProjectilContoller`（子彈）打到敵人時都會這樣做：

```csharp
float damage = DamageCalculator.Calculate(武器傷害, out bool isCrit); // 套用攻擊力、爆擊
enemy.TakeDamage_E(damage);
HitEffects.EnemyHit(enemy.gameObject, damage, isCrit);              // 敵人瞬間變白 + 跳傷害數字
```

`CurrentDamage` 本身沒動，它還是代表武器原本的傷害。**之後新增武器，打到敵人時也照這三行寫。**

計算方式：`最終傷害 = 武器傷害 × 攻擊力倍率`，爆擊時再 × 2（爆擊率預設 5%）。
每打中一隻敵人各自擲一次爆擊，所以一刀砍到三隻，可能只有其中一隻爆擊。
場景裡沒有 `PlayerStats` 時會直接回傳原本的傷害，不會報錯。

子彈碰到 Tag 是 `Wall` 的東西會消失：D 的牆和石頭已經改成 `Wall`，水不會擋子彈。

### 武器圖片 = 攻擊範圍

換武器時，`WeaponHitBox` 會自動：

1. 把圖片縮放成 WeaponData 的 **Weapon Length**（Unity 單位；匕首 1.5、斧頭 1.9、劍 2.2）
2. 刀柄放在離玩家中心 0.35 的地方
3. 攻擊範圍改成**圖片的形狀**（照圖片透明度描出來的外框），原本的方形 Collider 會關掉

所以畫面上刀砍到哪裡，就打得到哪裡。新武器的圖片請做成**橫的、刀柄在左邊**，背景要透明。
想讓某把武器攻擊範圍大一點，就把它的 Weapon Length 調大。

---

## 組員 C（敵人）

### 已經在 `integration` 接好的

- `Enemycontroller` 死掉會掉 **Gold Drop**（預設 5）枚金幣，會自動套用掠奪加成；同一幀被打兩次也只會死一次。
- 用程式生出來的敵人會自己用 Tag 找玩家。
- 在水裡移動速度 × 0.5（`EnemyFollow` 也是）。
- `EnemyFollow` 打中玩家會真的扣血（**Attack Damage**，預設 1）。

**還要 C 決定的**：正式的敵人要用 `Enemycontroller` 還是 `EnemyFollow`。
`EnemyFollow` 目前沒有血量，武器打不死它；要用它的話，請把它跟 `Enemycontroller` 合成一支，
或是讓同一個敵人身上兩支都放（`Enemycontroller` 管血量和死亡，`EnemyFollow` 管移動和攻擊，
這時候要把 `Enemycontroller` 的移動關掉）。

自己做新的地形也可以讓人變慢：Collider2D 勾 **Is Trigger**，再加上 `SlowZone` 元件，
移動程式把速度乘上 `SlowZone.MultiplierFor(rb)`。

---

## 組員 D（關卡流程）

**已經在 `integration` branch 接好了**，D 之後請從 integration 繼續改（先把 integration merge 進 D）。

流程：進場景 → 第 1 間房間 → 一波一波生敵人 → 全部打完 → 商店 → 按 Continue → 下一間房間 → 打完最後一間 → 勝利畫面

| 程式 | 做什麼 | Inspector 可以調的 |
|---|---|---|
| `GameManager` | 整局流程：開局、開商店、換房間、勝利 | Rooms To Win（幾間算贏）、Price Increase Per Room（每間商品貴多少） |
| `WaveManager` | 一波清完才生下一波；敵人放在它底下，用「還剩幾隻」判斷打完沒 | Enemy Prefabs、Waves、Time Between Waves、Max Enemies Per Wave、Safe Radius |
| `RoomManager` | 生成地圖；換房間時會先清掉舊地圖 | 地板、牆、石頭、水的 prefab |
| `water.prefab` | 水可以走過去，但在裡面速度 × 0.5（`SlowZone`） | SlowZone 的 Speed Multiplier |
| `EnemyCountManager` | 只記數量，**不再暫停遊戲** | |

想在畫面上顯示「Room 1 / 3」「Wave 2 / 3」可以監聽：

```csharp
gameManager.OnRoomStarted += (room, total) => { /* 更新文字 */ };
waveManager.OnWaveStarted += (wave, total) => { /* 更新文字 */ };
```

---

## 場景裡需要放的東西

| 物件 | 元件 | 說明 |
|---|---|---|
| RunSystems | `CurrencyManager`、`PlayerStats`、`PassiveManager` | 換場景會保留，整局只要一個 |
| ShopManager | `ShopManager` | Item Pool 拖入 `Assets/Data/Passives` 裡的被動能力 |
| Canvas | `PlayerHealthDisplay`、`HealthUI`、`CoinUI`、`StatsPanelUI`、`ShopUI`、`GameResultUI` | 可以直接從 `E_ShopTest` 場景複製 |

> `E_ShopTest` 裡的玩家是假的（`FakePlayerController`，欄位跟 A 的一樣）。
> 換成真的玩家時：刪掉假玩家，把 HUD 上 `PlayerHealthDisplay` 的 **Component Name** 改回 `PlayerController`，
> **Player** 欄位留空（會自動找 Tag 是 Player 的物件）或拖入真的玩家。

想看全部接好的樣子：打開 `Assets/Scenes/E_ShopTest.unity` 按 Play。
需要重建這個場景：上方選單 **Tools → 組員E → 建立商店測試場景**。

## 新增被動能力

Project 視窗右鍵 → **Create → 組員E → 被動能力**，填好名稱、價格、稀有度、效果，
再拖進 ShopManager 的 Item Pool 就會出現在商店
（或打開有 ShopManager 的場景，按 **Tools → 組員E → 把所有被動能力與道具加入商店**）。

**消耗品**（例如補血藥）：勾選 **Consumable**，填 **Heal Amount**。
買了立刻生效，不會留在擁有清單，也不會套用 Modifiers；每次開商店都可能再出現。

效果計算：`最終值 = (基礎值 + Flat 加總) × (1 + Percent 加總)`

| 能力 | 基礎值 | 例子 |
|---|---|---|
| MaxHealth | 3 | Flat +1 → 4 顆心 |
| Attack | 1（= 100% 傷害） | Percent 0.2 → 120% |
| MoveSpeed | 5 | Percent 0.15 → 5.75 |
| CritChance | 0.05（= 5%） | Flat 0.05 → 10% |
| Luck | 0 | Flat 1 → 商店更容易出 Rare / Epic |
| Loot | 1（= 100% 金幣） | Flat 0.25 → 金幣多 25% |
