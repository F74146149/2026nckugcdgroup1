# 組員 E：經濟、被動技能與 UI 系統：給隊友的介面說明

這份說明列出其他組員要「呼叫什麼」或「監聽什麼」，才能和組員 E 的系統接起來。
所有方法都先檢查 `Instance != null`，這樣場景裡沒有 E 的系統也不會報錯。

---

## 組員 A（玩家）

### 1. 血量上限：跟著被動能力變化

```csharp
// 開場時
maxHealth = PlayerStats.Instance.MaxHealth;

// 買到加血上限的能力時會收到通知
void OnEnable()  { if (PlayerStats.Instance != null) PlayerStats.Instance.OnStatsChanged += OnStatsChanged; }
void OnDisable() { if (PlayerStats.Instance != null) PlayerStats.Instance.OnStatsChanged -= OnStatsChanged; }

void OnStatsChanged()
{
    int newMax = PlayerStats.Instance.MaxHealth;
    if (newMax > maxHealth) currentHealth += newMax - maxHealth; // 多出來的那格補滿
    maxHealth = newMax;
    currentHealth = Mathf.Min(currentHealth, maxHealth);
}
```

### 2. 移動速度

```csharp
float speed = PlayerStats.Instance != null ? PlayerStats.Instance.MoveSpeed : PlayerSpeed;
rb.linearVelocity = movement * speed;
```

### 3. 血量 UI：血量改變時呼叫

```csharp
// 場景裡的 HealthUI（拖進 Inspector 或用 FindFirstObjectByType）
healthUI.SetHealth(currentHealth, maxHealth);
```

### 4. 接上補血（商店的補血道具要用）

商店的 Lotus Elixir 買了會回 1 滴血。血量在 A 這邊，所以要接上 `PlayerHealing`：

```csharp
void OnEnable()
{
    PlayerHealing.OnHealRequested += Heal;                    // 收到補血要求
    PlayerHealing.CanHeal = () => currentHealth < maxHealth;  // 滿血時商店不讓玩家買
}
void OnDisable()
{
    PlayerHealing.OnHealRequested -= Heal;
    PlayerHealing.CanHeal = null;
}

void Heal(int amount)
{
    currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    // 記得更新血量 UI
}
```

### 5. 死亡時打開失敗畫面

```csharp
if (GameResultUI.Instance != null) GameResultUI.Instance.ShowGameOver();
```

可以參考 `Assets/Scripts/Stats/Test/DemoPlayerHealth.cs`，裡面是完整的示範。

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
| Canvas | `HealthUI`、`CoinUI`、`StatsPanelUI`、`ShopUI`、`GameResultUI` | 可以直接從 `E_ShopTest` 場景複製 |

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
