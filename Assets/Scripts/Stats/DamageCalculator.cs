using UnityEngine;

/// <summary>
/// 根據目前的 Buff（被動能力）算出攻擊的最終傷害（組員 E）。
///
/// 公式：
///   最終傷害 = 武器傷害 × 攻擊力倍率
///   如果爆擊：再 × 爆擊倍率（預設 2 倍）。爆擊機率 = 爆擊率（預設 5%）
///
/// 例子：武器傷害 10，買了 Khopesh Edge（攻擊 +20%）
///   → 10 × 1.2 = 12，爆擊時 12 × 2 = 24
///
/// 給武器組員用（prototype-fight 的 WeaponHitBox，打到敵人那一行）：
///   enemy.TakeDamage_E(DamageCalculator.Calculate(weaponController.CurrentDamage));
///
/// 場景裡沒有 PlayerStats 時，直接回傳原本的武器傷害，不會報錯。
/// </summary>
public static class DamageCalculator
{
    /// <summary>算出這一擊的傷害（含攻擊力加成與爆擊）</summary>
    public static float Calculate(float weaponDamage)
    {
        return Calculate(weaponDamage, out _);
    }

    /// <summary>
    /// 算出這一擊的傷害。isCrit 會告訴你這一擊有沒有爆擊（可以拿來播爆擊特效）。
    /// </summary>
    public static float Calculate(float weaponDamage, out bool isCrit)
    {
        isCrit = false;

        PlayerStats stats = PlayerStats.Instance;
        if (stats == null) return weaponDamage;

        float damage = weaponDamage * stats.AttackMultiplier;

        if (Random.value < stats.CritChance)
        {
            isCrit = true;
            damage *= stats.CritDamageMultiplier;
        }

        return damage;
    }

    /// <summary>只算攻擊力加成、不擲爆擊（給 UI 顯示「現在一刀打多少」用）</summary>
    public static float Preview(float weaponDamage)
    {
        PlayerStats stats = PlayerStats.Instance;
        if (stats == null) return weaponDamage;
        return weaponDamage * stats.AttackMultiplier;
    }
}
