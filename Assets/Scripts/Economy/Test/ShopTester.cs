using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 測試商店與勝敗畫面用，正式遊戲不要放進場景。
///
/// 按鍵：
///   O = 打開商店（模擬打完一個房間）
///   K = 直接打開失敗畫面
///   V = 直接打開勝利畫面
/// </summary>
public class ShopTester : MonoBehaviour
{
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
    }
}
