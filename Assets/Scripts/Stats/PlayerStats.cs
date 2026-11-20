using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色能力值的計算與疊加（組員 E）。
/// 整個遊戲只有一個，其他腳本用 PlayerStats.Instance 讀取。
///
/// 給隊友用的（讀取最終數值）：
///   PlayerStats.Instance.MaxHealth         // int，生命值上限
///   PlayerStats.Instance.MoveSpeed         // float，移動速度
///   PlayerStats.Instance.AttackMultiplier  // float，傷害倍率（1 = 100%）
///   PlayerStats.Instance.CritChance        // float，0 ~ 1
///   PlayerStats.Instance.RollDamage(10, out bool isCrit)  // 算出這一擊的傷害（含攻擊力與爆擊）
///
/// 數值改變時會發出 OnStatsChanged，例如買到加血能力時，A 可以讓玩家的血量上限跟著變。
///
/// 計算方式：最終值 = (基礎值 + Flat 加總) × (1 + Percent 加總)
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("基礎數值（沒有任何被動能力時）")]
    [SerializeField] private int baseMaxHealth = 3;
    [Tooltip("1 = 100% 傷害")]
    [SerializeField] private float baseAttack = 1f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [Tooltip("0.05 = 5%")]
    [Range(0f, 1f)]
    [SerializeField] private float baseCritChance = 0.05f;
    [SerializeField] private float baseLuck = 0f;
    [Tooltip("1 = 正常金幣")]
    [SerializeField] private float baseLoot = 1f;

    [Header("爆擊")]
    [Tooltip("爆擊時傷害乘以多少")]
    [SerializeField] private float critDamageMultiplier = 2f;

    [Header("限制")]
    [Tooltip("血量上限最少幾顆（避免扣血上限的能力把它扣到 0）")]
    [SerializeField] private int minMaxHealth = 1;

    [Header("切換場景時保留")]
    [SerializeField] private bool keepBetweenScenes = true;

    /// <summary>任何能力值改變時觸發</summary>
    public event Action OnStatsChanged;

    // 每一筆加成記住「是誰給的」，之後才能整批移除
    private struct Entry
    {
        public object source;
        public StatModifier modifier;
    }

    private readonly List<Entry> modifiers = new List<Entry>();

    // ===== 方便讀取的屬性 =====
    public int MaxHealth => Mathf.Max(minMaxHealth, Mathf.RoundToInt(Get(StatType.MaxHealth)));
    public float AttackMultiplier => Get(StatType.Attack);
    public float MoveSpeed => Get(StatType.MoveSpeed);
    public float CritChance => Get(StatType.CritChance);
    public float Luck => Get(StatType.Luck);
    public float LootMultiplier => Get(StatType.Loot);
    public float CritDamageMultiplier => critDamageMultiplier;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (keepBetweenScenes)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // 讓金幣的掠奪倍率一開始就正確
        SyncLootToCurrency();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>取得某項能力的基礎值</summary>
    public float GetBase(StatType stat)
    {
        switch (stat)
        {
            case StatType.MaxHealth: return baseMaxHealth;
            case StatType.Attack: return baseAttack;
            case StatType.MoveSpeed: return baseMoveSpeed;
            case StatType.CritChance: return baseCritChance;
            case StatType.Luck: return baseLuck;
            case StatType.Loot: return baseLoot;
            default: return 0f;
        }
    }

    /// <summary>取得某項能力的最終數值（已套用所有加成）</summary>
    public float Get(StatType stat)
    {
        float flat = 0f;
        float percent = 0f;

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier m = modifiers[i].modifier;
            if (m.stat != stat) continue;

            if (m.kind == ModifierKind.Flat) flat += m.value;
            else percent += m.value;
        }

        float value = (GetBase(stat) + flat) * (1f + percent);

        // 各能力的合理範圍
        if (stat == StatType.CritChance) return Mathf.Clamp01(value);
        return Mathf.Max(0f, value);
    }

    /// <summary>
    /// 加上一筆加成。source 是「誰給的」，例如某個被動能力，之後可以用 RemoveAllFrom 整批拿掉。
    /// </summary>
    public void AddModifier(object source, StatModifier modifier)
    {
        modifiers.Add(new Entry { source = source, modifier = modifier });
        NotifyChanged();
    }

    /// <summary>一次加上很多筆加成（只會通知一次）</summary>
    public void AddModifiers(object source, IEnumerable<StatModifier> list)
    {
        if (list == null) return;
        foreach (StatModifier m in list)
        {
            modifiers.Add(new Entry { source = source, modifier = m });
        }
        NotifyChanged();
    }

    /// <summary>移除某個來源給的所有加成</summary>
    public void RemoveAllFrom(object source)
    {
        int removed = modifiers.RemoveAll(e => Equals(e.source, source));
        if (removed > 0) NotifyChanged();
    }

    /// <summary>清除所有加成，回到基礎值（新的輪迴開始時）</summary>
    public void ClearAll()
    {
        if (modifiers.Count == 0) return;
        modifiers.Clear();
        NotifyChanged();
    }

    /// <summary>
    /// 算出一次攻擊的傷害：武器基礎傷害 × 攻擊力，並擲骰判斷爆擊。
    /// 給組員 B 用：int dmg = PlayerStats.Instance.RollDamage(weaponDamage, out bool isCrit);
    /// </summary>
    public int RollDamage(int baseDamage, out bool isCrit)
    {
        float damage = baseDamage * AttackMultiplier;

        isCrit = UnityEngine.Random.value < CritChance;
        if (isCrit) damage *= critDamageMultiplier;

        return Mathf.Max(1, Mathf.RoundToInt(damage));
    }

    private void NotifyChanged()
    {
        SyncLootToCurrency();
        OnStatsChanged?.Invoke();
    }

    // 掠奪能力直接影響金幣倍率
    private void SyncLootToCurrency()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.LootMultiplier = LootMultiplier;
        }
    }
}
