using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 測試用的假玩家血量，正式遊戲不要放進場景。
/// 示範「血量上限跟著 PlayerStats 變」與「死亡時打開失敗畫面」怎麼接，組員 A 可以參考。
///
/// 按鍵：
///   J = 受到 1 點傷害（扣到 0 會打開失敗畫面）
///   H = 補 1 滴血
/// </summary>
public class DemoPlayerHealth : MonoBehaviour
{
    [SerializeField] private HealthUI healthUI;

    private int maxHealth;
    private int currentHealth;

    private void Start()
    {
        maxHealth = PlayerStats.Instance != null ? PlayerStats.Instance.MaxHealth : 3;
        currentHealth = maxHealth;

        if (PlayerStats.Instance != null) PlayerStats.Instance.OnStatsChanged += HandleStatsChanged;
        if (GameResultUI.Instance != null) GameResultUI.Instance.OnRetryWithoutReload += Refill;

        // 接上補血（商店的補血道具會透過這裡補血）
        PlayerHealing.OnHealRequested += Heal;
        PlayerHealing.CanHeal = CanHeal;

        Refresh();
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null) PlayerStats.Instance.OnStatsChanged -= HandleStatsChanged;
        if (GameResultUI.Instance != null) GameResultUI.Instance.OnRetryWithoutReload -= Refill;

        PlayerHealing.OnHealRequested -= Heal;
        if (PlayerHealing.CanHeal == (System.Func<bool>)CanHeal) PlayerHealing.CanHeal = null;
    }

    private bool CanHeal()
    {
        return currentHealth < maxHealth;
    }

    private void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Refresh();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (GameResultUI.Instance != null && GameResultUI.Instance.IsShowing) return;

        if (keyboard.jKey.wasPressedThisFrame) TakeDamage(1);

        if (keyboard.hKey.wasPressedThisFrame) Heal(1);
    }

    private void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Refresh();

        if (currentHealth == 0 && GameResultUI.Instance != null)
        {
            GameResultUI.Instance.ShowGameOver();
        }
    }

    // 買到加血上限的能力時：上限變大，多出來的那格直接補滿
    private void HandleStatsChanged()
    {
        int newMax = PlayerStats.Instance.MaxHealth;
        if (newMax > maxHealth) currentHealth += newMax - maxHealth;
        maxHealth = newMax;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Refresh();
    }

    private void Refill()
    {
        maxHealth = PlayerStats.Instance != null ? PlayerStats.Instance.MaxHealth : maxHealth;
        currentHealth = maxHealth;
        Refresh();
    }

    private void Refresh()
    {
        if (healthUI != null) healthUI.SetHealth(currentHealth, maxHealth);
    }
}
