using UnityEngine;

/// <summary>
/// 打擊特效（組員 E）。
///
///   HitEffects.EnemyHit(敵人, 傷害, 是否爆擊)
///     → 敵人閃一下顏色 + 頭上跳出傷害數字（爆擊是黃色大字，後面加「!」）
///     WeaponHitBox、ProjectilContoller 打中敵人時已經會呼叫
///
///   HitEffects.PlayerHit(玩家)
///     → 玩家閃紅色幾下
///     PlayerHealthDisplay 發現血量變少時已經會呼叫
/// </summary>
public static class HitEffects
{
    public static Color EnemyFlashColor = new Color(1f, 0.35f, 0.35f);
    public static Color CritFlashColor = new Color(1f, 0.85f, 0.2f);
    public static Color PlayerFlashColor = new Color(1f, 0.25f, 0.25f);

    public static void EnemyHit(GameObject enemy, float damage, bool isCrit)
    {
        if (enemy == null) return;

        HitFlash.Flash(enemy, isCrit ? CritFlashColor : EnemyFlashColor, 0.1f, 1);
        DamagePopup.Spawn(enemy.transform.position + Vector3.up * 0.6f, damage, isCrit);
    }

    public static void PlayerHit(GameObject player)
    {
        if (player == null) return;

        HitFlash.Flash(player, PlayerFlashColor, 0.08f, 3);
    }
}
