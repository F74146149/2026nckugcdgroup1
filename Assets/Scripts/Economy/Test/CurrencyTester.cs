using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 測試金幣用，正式遊戲不要放進場景。
///
/// 按鍵（按 Play 之後）：
///   G = 獲得 10 金幣（會套用掠奪加成）
///   B = 花 25 金幣（模擬在商店買東西）
///   L = 掠奪加成 +0.5（模擬買到掠奪被動）
///   X = 金幣歸零（模擬新的輪迴）
/// </summary>
public class CurrencyTester : MonoBehaviour
{
    [SerializeField] private int gainAmount = 10;
    [SerializeField] private int itemPrice = 25;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        CurrencyManager currency = CurrencyManager.Instance;
        if (keyboard == null || currency == null) return;

        if (keyboard.gKey.wasPressedThisFrame)
        {
            int gained = currency.AddGold(gainAmount);
            Debug.Log("獲得 " + gained + " 金幣，目前：" + currency.Gold);
        }

        if (keyboard.bKey.wasPressedThisFrame)
        {
            if (currency.TrySpendGold(itemPrice))
                Debug.Log("購買成功，花了 " + itemPrice + "，剩下：" + currency.Gold);
            else
                Debug.Log("金幣不夠！需要 " + itemPrice + "，只有 " + currency.Gold);
        }

        if (keyboard.lKey.wasPressedThisFrame)
        {
            if (PlayerStats.Instance != null)
            {
                // 有 PlayerStats 時，掠奪由它計算，所以加在它身上
                PlayerStats.Instance.AddModifier(this, new StatModifier(StatType.Loot, ModifierKind.Flat, 0.5f));
            }
            else
            {
                currency.LootMultiplier += 0.5f;
            }
            Debug.Log("掠奪加成變成 x" + currency.LootMultiplier);
        }

        if (keyboard.xKey.wasPressedThisFrame)
        {
            currency.ResetGold();
            Debug.Log("金幣歸零");
        }
    }
}
