using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 測試商店、勝敗畫面與傷害計算用，正式遊戲不要放進場景。
///
/// 按鍵：
///   O = 打開商店（模擬打完一個房間）
///   K = 直接打開失敗畫面
///   V = 直接打開勝利畫面
///   T = 模擬砍一刀，在 Console 印出套用 Buff 後的傷害
/// </summary>
public class ShopTester : MonoBehaviour
{
    [Tooltip("按 T 時用的武器傷害")]
    [SerializeField] private float testWeaponDamage = 10f;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool resultShowing = GameResultUI.Instance != null && GameResultUI.Instance.IsShowing;

        if (keyboard.oKey.wasPressedThisFrame && !resultShowing)
        {
            if (ShopManager.Instance == null) Debug.LogWarning("ShopTester：場景裡沒有 ShopManager");
            else if (!ShopManager.Instance.IsOpen) ShopManager.Instance.OpenShop();
        }

        if (keyboard.kKey.wasPressedThisFrame && GameResultUI.Instance != null)
        {
            if (ShopManager.Instance != null) ShopManager.Instance.CloseShop();
            GameResultUI.Instance.ShowGameOver();
        }

        if (keyboard.vKey.wasPressedThisFrame && GameResultUI.Instance != null)
        {
            if (ShopManager.Instance != null) ShopManager.Instance.CloseShop();
            GameResultUI.Instance.ShowVictory();
        }

        if (keyboard.tKey.wasPressedThisFrame)
        {
            float damage = DamageCalculator.Calculate(testWeaponDamage, out bool isCrit);
            string buffs = PlayerStats.Instance == null
                ? "（場景裡沒有 PlayerStats，不套用 Buff）"
                : "（攻擊力 " + (PlayerStats.Instance.AttackMultiplier * 100f).ToString("0") + "%、" +
                  "爆擊率 " + (PlayerStats.Instance.CritChance * 100f).ToString("0") + "%）";

            Debug.Log("測試傷害：武器 " + testWeaponDamage + " → " + damage.ToString("0.##") +
                      (isCrit ? "  爆擊！" : "") + "  " + buffs);
        }
    }
}
