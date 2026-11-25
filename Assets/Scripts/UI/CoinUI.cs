using TMPro;
using UnityEngine;

/// <summary>
/// 在畫面上顯示目前的金幣數量。
///
/// 使用方式：
/// 1. 在 Canvas 底下建一個 Text - TextMeshPro。
/// 2. 把這個腳本掛在那個文字物件上（Coin Text 欄位沒填的話會自動抓自己身上的文字）。
/// 3. 場景裡要有一個掛著 CurrencyManager 的物件。
/// </summary>
public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    [Tooltip("{0} 會被換成金幣數量")]
    [SerializeField] private string format = "Gold: {0}";

    private CurrencyManager currency;

    private void Awake()
    {
        if (coinText == null) coinText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        // 在 Start 才找 CurrencyManager，確保它的 Awake 已經跑完
        currency = CurrencyManager.Instance;
        if (currency == null)
        {
            Debug.LogWarning("CoinUI：場景裡找不到 CurrencyManager。", this);
            return;
        }

        currency.OnGoldChanged += Refresh;
        Refresh(currency.Gold);
    }

    private void OnDestroy()
    {
        if (currency != null) currency.OnGoldChanged -= Refresh;
    }

    private void Refresh(int gold)
    {
        if (coinText != null) coinText.text = string.Format(format, gold);
    }
}
