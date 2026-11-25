using System;
using UnityEngine;

/// <summary>
/// 金幣管理（組員 E）。
/// 整個遊戲只有一個，其他腳本用 CurrencyManager.Instance 來呼叫。
///
/// 常用方法：
///   CurrencyManager.Instance.AddGold(5);          // 獲得金幣（會套用掠奪加成）
///   CurrencyManager.Instance.TrySpendGold(30);    // 花錢，錢不夠會回傳 false
///   CurrencyManager.Instance.CanAfford(30);       // 只檢查買不買得起
///   CurrencyManager.Instance.ResetGold();         // 新的輪迴開始時歸零
///
/// 範例：敵人死掉時給金幣（加在敵人的 Die() 裡）
///   if (CurrencyManager.Instance != null) CurrencyManager.Instance.AddGold(goldDrop);
///
/// 想在金幣改變時做事（例如更新 UI），訂閱事件：
///   CurrencyManager.Instance.OnGoldChanged += 你的方法;   // 方法參數是 (int 目前金幣)
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("金幣")]
    [SerializeField] private int startingGold = 0;

    [Header("掠奪加成（1 = 沒有加成，1.5 = 多拿 50%）")]
    [Tooltip("之後由被動能力「掠奪 Loot」來調整")]
    [SerializeField] private float lootMultiplier = 1f;

    [Header("切換場景時保留金幣")]
    [Tooltip("勾選後，換房間 / 換場景金幣不會消失")]
    [SerializeField] private bool keepBetweenScenes = true;

    /// <summary>金幣數量改變時觸發，參數是目前的金幣數量</summary>
    public event Action<int> OnGoldChanged;

    public int Gold { get; private set; }

    public float LootMultiplier
    {
        get => lootMultiplier;
        set => lootMultiplier = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        // 確保只有一個 CurrencyManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Gold = Mathf.Max(0, startingGold);

        if (keepBetweenScenes)
        {
            // DontDestroyOnLoad 只能用在最上層的物件
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// 獲得金幣。
    /// </summary>
    /// <param name="amount">基本數量</param>
    /// <param name="applyLoot">要不要套用掠奪加成（例如任務獎勵可以不套用）</param>
    /// <returns>實際獲得的數量</returns>
    public int AddGold(int amount, bool applyLoot = true)
    {
        if (amount <= 0) return 0;

        // 四捨五入（Mathf.RoundToInt 遇到 .5 會取偶數，12.5 會變 12，所以自己寫）
        int gained = applyLoot ? Mathf.FloorToInt(amount * lootMultiplier + 0.5f) : amount;
        Gold += gained;
        OnGoldChanged?.Invoke(Gold);
        return gained;
    }

    /// <summary>買不買得起</summary>
    public bool CanAfford(int cost)
    {
        return cost <= Gold;
    }

    /// <summary>
    /// 花費金幣。錢不夠的話不會扣，並回傳 false。
    /// </summary>
    public bool TrySpendGold(int cost)
    {
        if (cost < 0) return false;
        if (!CanAfford(cost)) return false;

        Gold -= cost;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    /// <summary>金幣回到初始值（新的輪迴開始時呼叫）</summary>
    public void ResetGold()
    {
        Gold = Mathf.Max(0, startingGold);
        OnGoldChanged?.Invoke(Gold);
    }
}
