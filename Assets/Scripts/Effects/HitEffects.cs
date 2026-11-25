using UnityEngine;

/// <summary>
/// 打擊特效（組員 E）。
///
///   HitEffects.EnemyHit(敵人, 傷害, 是否爆擊)
///     → 敵人一瞬間變白 + 頭上跳出傷害數字（爆擊是黃色、比較大）
///     WeaponHitBox、ProjectilContoller 打中敵人時已經會呼叫
///
///   HitEffects.PlayerHit(玩家)
///     → 玩家閃紅色（大約閃到無敵時間結束）
///     PlayerHealthDisplay 發現血量變少時已經會呼叫
/// </summary>
public static class HitEffects
{
    public static float EnemyWhiteTime = 0.08f;
    public static Color PlayerFlashColor = new Color(1f, 0.25f, 0.25f);

    public static void EnemyHit(GameObject enemy, float damage, bool isCrit)
    {
        if (enemy == null) return;

        HitFlash.FlashWhite(enemy, EnemyWhiteTime);
        DamagePopup.Spawn(enemy.transform.position + Vector3.up * 0.6f, damage, isCrit);
    }

    public static void PlayerHit(GameObject player)
    {
        if (player == null) return;

        // 0.08 秒一亮一暗，閃 6 下 ≈ 0.9 秒，跟玩家 1 秒的無敵時間差不多
        HitFlash.Flash(player, PlayerFlashColor, 0.08f, 6);
    }
}
