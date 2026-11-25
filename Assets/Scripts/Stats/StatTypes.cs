using System;

/// <summary>
/// 角色的各項能力值（對應 README 的被動能力表）。
/// </summary>
public enum StatType
{
    MaxHealth,   // 生命值上限（幾顆愛心）
    Attack,      // 攻擊力倍率：1 = 100% 傷害
    MoveSpeed,   // 移動速度
    CritChance,  // 爆擊率：0 ~ 1（0.05 = 5%）
    Luck,        // 運氣：越高，商店越容易出現稀有能力
    Loot,        // 掠奪倍率：1 = 正常金幣，1.25 = 多拿 25%
}

/// <summary>
/// 加成的計算方式。
/// 最終數值 = (基礎值 + 所有 Flat 加總) × (1 + 所有 Percent 加總)
/// </summary>
public enum ModifierKind
{
    Flat,     // 直接加：例如 MaxHealth +1
    Percent,  // 百分比：0.2 代表 +20%
}

/// <summary>
/// 一筆能力值加成，例如「攻擊力 +20%」。
/// </summary>
[Serializable]
public struct StatModifier
{
    public StatType stat;
    public ModifierKind kind;
    public float value;

    public StatModifier(StatType stat, ModifierKind kind, float value)
    {
        this.stat = stat;
        this.kind = kind;
        this.value = value;
    }

    /// <summary>給玩家看的說明，例如 "+20% Attack"、"+1 Max HP"</summary>
    public string Describe()
    {
        string sign = value >= 0 ? "+" : "-";
        float abs = Math.Abs(value);

        if (kind == ModifierKind.Percent)
        {
            return sign + (abs * 100f).ToString("0.#") + "% " + StatNames.Get(stat);
        }

        // 爆擊率的 Flat 用百分比顯示比較好懂（0.05 → 5%）
        if (stat == StatType.CritChance)
        {
            return sign + (abs * 100f).ToString("0.#") + "% " + StatNames.Get(stat);
        }

        // 掠奪的 Flat 是倍率（0.25 → +25%）
        if (stat == StatType.Loot)
        {
            return sign + (abs * 100f).ToString("0.#") + "% " + StatNames.Get(stat);
        }

        return sign + abs.ToString("0.##") + " " + StatNames.Get(stat);
    }
}

/// <summary>
/// 能力值在畫面上顯示的名稱。
/// 目前用英文，因為 TextMeshPro 預設字型不支援中文；之後換成中文字型再改這裡就好。
/// </summary>
public static class StatNames
{
    public static string Get(StatType stat)
    {
        switch (stat)
        {
            case StatType.MaxHealth: return "Max HP";
            case StatType.Attack: return "Attack";
            case StatType.MoveSpeed: return "Move Speed";
            case StatType.CritChance: return "Crit Chance";
            case StatType.Luck: return "Luck";
            case StatType.Loot: return "Loot";
            default: return stat.ToString();
        }
    }
}
