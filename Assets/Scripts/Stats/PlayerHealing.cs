using System;

/// <summary>
/// 補血的「連接點」（組員 E 提供，組員 A 接上）。
/// 血量是組員 A 在管，所以商店、道具不會直接改玩家血量，而是透過這裡發出「請補血」的要求。
///
/// 組員 A 要做的（在玩家血量的腳本裡）：
///   void OnEnable()
///   {
///       PlayerHealing.OnHealRequested += Heal;                    // 收到補血要求時補血
///       PlayerHealing.CanHeal = () => currentHealth < maxHealth;  // 告訴商店現在能不能補（滿血就不能買）
///   }
///   void OnDisable()
///   {
///       PlayerHealing.OnHealRequested -= Heal;
///       PlayerHealing.CanHeal = null;
///   }
///
/// 其他人要補血時：PlayerHealing.Heal(1);
/// </summary>
public static class PlayerHealing
{
    /// <summary>有人要求補血時觸發，參數是補幾滴</summary>
    public static event Action<int> OnHealRequested;

    /// <summary>
    /// 玩家現在能不能補血（例如滿血就不能）。
    /// 由玩家血量的腳本設定；沒設定的話當作「可以補」。
    /// </summary>
    public static Func<bool> CanHeal;

    /// <summary>有沒有任何腳本在接收補血（沒有的話，補血不會有效果）</summary>
    public static bool HasReceiver => OnHealRequested != null;

    /// <summary>現在補血有沒有意義</summary>
    public static bool IsHealUseful => HasReceiver && (CanHeal == null || CanHeal());

    /// <summary>要求補血，成功送出回傳 true</summary>
    public static bool Heal(int amount)
    {
        if (amount <= 0 || OnHealRequested == null) return false;
        OnHealRequested.Invoke(amount);
        return true;
    }
}
