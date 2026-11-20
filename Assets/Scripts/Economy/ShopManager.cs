using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 商店邏輯（組員 E）：隨機抽商品、購買、重新整理（reroll）。
/// 畫面由 ShopUI 負責，這裡只管規則。
///
/// 給隊友用的：
///   ShopManager.Instance.OpenShop();            // 打完房間後打開商店（組員 D 的關卡流程呼叫）
///   ShopManager.Instance.OnShopClosed += 下一關; // 玩家按「繼續」離開商店時通知
///
/// 稀有度權重：運氣（Luck）越高，Rare / Epic 越容易出現。
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    /// <summary>商店裡的一格商品</summary>
    public class Offer
    {
        public PassiveData item;
        public int price;
        public bool sold;
    }

    /// <summary>購買結果，給 UI 顯示提示用</summary>
    public enum BuyResult
    {
        Success,
        NotEnoughGold,
        SoldOut,
        MaxStacks,
        Invalid,
    }

    [Header("商品池（所有可能出現的被動能力）")]
    [SerializeField] private List<PassiveData> itemPool = new List<PassiveData>();

    [Header("每次幾格商品")]
    [Min(1)] [SerializeField] private int slotCount = 3;

    [Header("稀有度權重（數字越大越常出現）")]
    [SerializeField] private float commonWeight = 60f;
    [SerializeField] private float rareWeight = 30f;
    [SerializeField] private float epicWeight = 10f;
    [Tooltip("每 1 點運氣，Rare 增加多少權重（Epic 增加一半）")]
    [SerializeField] private float luckBonusPerPoint = 8f;

    [Header("價格")]
    [Tooltip("價格倍率，可以隨樓層調高（例如第 3 層設 1.5）")]
    [Min(0f)] [SerializeField] private float priceMultiplier = 1f;

    [Header("重新整理（Reroll）")]
    [SerializeField] private int rerollBaseCost = 10;
    [Tooltip("每 reroll 一次，下一次多貴多少")]
    [SerializeField] private int rerollCostIncrease = 5;

    /// <summary>商店打開時觸發</summary>
    public event Action OnShopOpened;
    /// <summary>商店關閉（玩家按繼續）時觸發，組員 D 可以在這裡進下一關</summary>
    public event Action OnShopClosed;
    /// <summary>商品內容改變（抽新的、賣掉、reroll）時觸發</summary>
    public event Action OnOffersChanged;

    private readonly List<Offer> offers = new List<Offer>();
    private int rerollCount;

    public IReadOnlyList<Offer> Offers => offers;
    public bool IsOpen { get; private set; }
    public int RerollCost => rerollBaseCost + rerollCount * rerollCostIncrease;

    public float PriceMultiplier
    {
        get => priceMultiplier;
        set => priceMultiplier = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("ShopManager：場景裡有兩個 ShopManager，多的那個會被忽略。", this);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>打開商店並抽新商品</summary>
    public void OpenShop()
    {
        rerollCount = 0;
        GenerateOffers();
        IsOpen = true;
        OnShopOpened?.Invoke();
        OnOffersChanged?.Invoke();
    }

    /// <summary>關閉商店</summary>
    public void CloseShop()
    {
        if (!IsOpen) return;
        IsOpen = false;
        OnShopClosed?.Invoke();
    }

    /// <summary>購買第 index 格商品</summary>
    public BuyResult TryBuy(int index)
    {
        if (index < 0 || index >= offers.Count) return BuyResult.Invalid;

        Offer offer = offers[index];
        if (offer.item == null) return BuyResult.Invalid;
        if (offer.sold) return BuyResult.SoldOut;

        PassiveManager passives = PassiveManager.Instance;
        CurrencyManager currency = CurrencyManager.Instance;
        if (passives == null || currency == null)
        {
            Debug.LogError("ShopManager：場景裡需要 CurrencyManager 和 PassiveManager。", this);
            return BuyResult.Invalid;
        }

        if (!passives.CanAdd(offer.item)) return BuyResult.MaxStacks;
        if (!currency.TrySpendGold(offer.price)) return BuyResult.NotEnoughGold;

        passives.TryAdd(offer.item);
        offer.sold = true;
        OnOffersChanged?.Invoke();
        return BuyResult.Success;
    }

    /// <summary>花錢重新抽商品，成功回傳 true</summary>
    public bool TryReroll()
    {
        CurrencyManager currency = CurrencyManager.Instance;
        if (currency == null || !currency.TrySpendGold(RerollCost)) return false;

        rerollCount++;
        GenerateOffers();
        OnOffersChanged?.Invoke();
        return true;
    }

    // ===== 抽商品 =====

    private void GenerateOffers()
    {
        offers.Clear();

        // 可以抽的：不是空的、還沒到疊加上限
        var candidates = new List<PassiveData>();
        foreach (PassiveData p in itemPool)
        {
            if (p == null || candidates.Contains(p)) continue;
            if (PassiveManager.Instance != null && !PassiveManager.Instance.CanAdd(p)) continue;
            candidates.Add(p);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("ShopManager：商品池是空的，或所有能力都已經疊滿。", this);
            return;
        }

        // 同一次不會抽到重複的
        int count = Mathf.Min(slotCount, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            PassiveData picked = PickWeighted(candidates);
            candidates.Remove(picked);

            offers.Add(new Offer
            {
                item = picked,
                price = Mathf.Max(0, Mathf.RoundToInt(picked.price * priceMultiplier)),
                sold = false,
            });
        }
    }

    private PassiveData PickWeighted(List<PassiveData> candidates)
    {
        float total = 0f;
        foreach (PassiveData p in candidates) total += WeightOf(p.rarity);

        float roll = UnityEngine.Random.value * total;
        foreach (PassiveData p in candidates)
        {
            roll -= WeightOf(p.rarity);
            if (roll <= 0f) return p;
        }
        return candidates[candidates.Count - 1];
    }

    private float WeightOf(Rarity rarity)
    {
        float luck = PlayerStats.Instance != null ? PlayerStats.Instance.Luck : 0f;

        switch (rarity)
        {
            case Rarity.Rare: return Mathf.Max(0.01f, rareWeight + luck * luckBonusPerPoint);
            case Rarity.Epic: return Mathf.Max(0.01f, epicWeight + luck * luckBonusPerPoint * 0.5f);
            default: return Mathf.Max(0.01f, commonWeight);
        }
    }
}
