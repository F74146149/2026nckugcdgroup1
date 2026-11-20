using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 測試用：模仿組員 A 目前的 PlayerController（欄位名稱、受傷方式都一樣）。
/// 在 PlayerController 合併進 main 之前，用它來測試 PlayerHealthDisplay。
/// 正式遊戲不要放進場景；合併後把 PlayerHealthDisplay 的 Component Name 改回 PlayerController。
///
/// 按鍵：
///   J = 受到 1 點傷害（跟 A 一樣呼叫 TakeDamage_P）
///   H = 補 1 滴血（模擬商店補血道具，走 PlayerHealing）
/// </summary>
public class FakePlayerController : MonoBehaviour
{
    // ===== 跟 A 的 PlayerController 一模一樣的欄位 =====
    public int maxHealth = 3;
    public int PlayercurrentHealth = 3;

    public void TakeDamage_P(int damage)
    {
        PlayercurrentHealth -= damage;

        if (PlayercurrentHealth <= 0)
        {
            PlayercurrentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        Time.timeScale = 0f;
    }

    // ===== 以下是測試用 =====

    private void Start()
    {
        // 測試場景按「重來」時，場景不會重新載入，所以自己回滿血
        if (GameResultUI.Instance != null) GameResultUI.Instance.OnRetryWithoutReload += Refill;
    }

    private void OnDestroy()
    {
        if (GameResultUI.Instance != null) GameResultUI.Instance.OnRetryWithoutReload -= Refill;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (GameResultUI.Instance != null && GameResultUI.Instance.IsShowing) return;
        if (ShopManager.Instance != null && ShopManager.Instance.IsOpen) return;

        if (keyboard.jKey.wasPressedThisFrame) TakeDamage_P(1);
        if (keyboard.hKey.wasPressedThisFrame) PlayerHealing.Heal(1);
    }

    private void Refill()
    {
        PlayercurrentHealth = maxHealth;
    }
}
