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

### 移動速度（可選）

想讓「移動速度」被動能力生效的話：

```csharp
float speed = PlayerStats.Instance != null ? PlayerStats.Instance.MoveSpeed : PlayerSpeed;
rb.linearVelocity = movement * speed;
```

---

## 組員 B（武器）

### 算傷害（含攻擊力加成與爆擊）

```csharp
int damage = weaponData.damage;
bool isCrit = false;
if (PlayerStats.Instance != null)
{
    damage = PlayerStats.Instance.RollDamage(weaponData.damage, out isCrit);
}
enemy.TakeDamage_E(damage);
// isCrit 可以拿來播爆擊特效
```

---

## 組員 C（敵人）

### 敵人死亡時給金幣

```csharp
[SerializeField] private int goldDrop = 5;

private void Die()
{
    if (CurrencyManager.Instance != null) CurrencyManager.Instance.AddGold(goldDrop); // 會自動套用掠奪加成
    // ...原本的死亡程式
}
```

---

## 組員 D（關卡流程）

### 打完房間 → 打開商店 → 玩家按繼續 → 下一關

```csharp
void Start()
{
    if (ShopManager.Instance != null) ShopManager.Instance.OnShopClosed += GoToNextLevel;
}

void OnRoomCleared()
{
    if (ShopManager.Instance != null) ShopManager.Instance.OpenShop();
    else GoToNextLevel();
}
```

### 樓層越深，商品越貴（可選）

```csharp
ShopManager.Instance.PriceMultiplier = 1f + floor * 0.25f;
```

### 打完最後一層

```csharp
if (GameResultUI.Instance != null) GameResultUI.Instance.ShowVictory();
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
