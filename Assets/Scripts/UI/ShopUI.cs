using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店畫面（組員 E）。
/// 規則在 ShopManager，這裡只負責顯示與按鈕。
///
/// 注意：這個腳本要掛在「一直開著」的物件上（例如 Canvas 底下的 ShopUI 物件），
/// 不要掛在 Panel 本身，因為 Panel 平常是關掉的。
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Tooltip("沒填的話自動用 ShopManager.Instance")]
    [SerializeField] private ShopManager shop;

    [Header("畫面")]
    [Tooltip("整個商店面板，平常隱藏")]
    [SerializeField] private GameObject panel;
    [Tooltip("一張商品卡片的範本（會被隱藏，用來複製）")]
    [SerializeField] private ShopItemSlotUI slotTemplate;

    [Header("按鈕")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private TMP_Text rerollButtonText;
    [SerializeField] private Button continueButton;

    [Header("提示文字（錢不夠之類）")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float messageDuration = 1.5f;

    [Header("其他")]
    [Tooltip("商店開著時暫停遊戲")]
    [SerializeField] private bool pauseWhileOpen = true;

    private readonly List<ShopItemSlotUI> slots = new List<ShopItemSlotUI>();
    private float messageTimer;
    private bool subscribed;

    private void Awake()
    {
        if (slotTemplate != null) slotTemplate.gameObject.SetActive(false);
        if (rerollButton != null) rerollButton.onClick.AddListener(OnRerollClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        if (messageText != null) messageText.text = "";
    }

    private void Start()
    {
        if (shop == null) shop = ShopManager.Instance;
        if (shop == null)
        {
            Debug.LogWarning("ShopUI：場景裡找不到 ShopManager。", this);
            if (panel != null) panel.SetActive(false);
            return;
        }

        shop.OnShopOpened += HandleOpened;
        shop.OnShopClosed += HandleClosed;
        shop.OnOffersChanged += Refresh;
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnGoldChanged += HandleGoldChanged;
        subscribed = true;

        if (panel != null) panel.SetActive(shop.IsOpen);
        if (shop.IsOpen) Refresh();
    }

    private void OnDestroy()
    {
        if (!subscribed) return;
        if (shop != null)
        {
            shop.OnShopOpened -= HandleOpened;
            shop.OnShopClosed -= HandleClosed;
            shop.OnOffersChanged -= Refresh;
        }
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnGoldChanged -= HandleGoldChanged;
    }

    private void Update()
    {
        // 用 unscaledDeltaTime，遊戲暫停時提示文字也會消失
        if (messageTimer > 0f)
        {
            messageTimer -= Time.unscaledDeltaTime;
            if (messageTimer <= 0f && messageText != null) messageText.text = "";
        }
    }

    private void HandleOpened()
    {
        if (panel != null) panel.SetActive(true);
        if (pauseWhileOpen) Time.timeScale = 0f;
        ShowMessage("");
    }

    private void HandleClosed()
    {
        if (panel != null) panel.SetActive(false);
        if (pauseWhileOpen) Time.timeScale = 1f;
    }

    private void HandleGoldChanged(int gold)
    {
        if (shop != null && shop.IsOpen) Refresh();
    }

    /// <summary>重新畫所有商品卡片</summary>
    private void Refresh()
    {
        if (shop == null || slotTemplate == null) return;

        IReadOnlyList<ShopManager.Offer> offers = shop.Offers;
        int gold = CurrencyManager.Instance != null ? CurrencyManager.Instance.Gold : 0;

        // 卡片不夠就複製
        while (slots.Count < offers.Count)
        {
            ShopItemSlotUI slot = Instantiate(slotTemplate, slotTemplate.transform.parent);
            slot.name = "Slot " + (slots.Count + 1);
            slots.Add(slot);
        }

        for (int i = 0; i < slots.Count; i++)
        {
            bool visible = i < offers.Count;
            slots[i].gameObject.SetActive(visible);
            if (!visible) continue;

            ShopManager.Offer offer = offers[i];
            int stacks = PassiveManager.Instance != null ? PassiveManager.Instance.GetStacks(offer.item) : 0;
            slots[i].Bind(i, offer, OnBuyClicked, gold >= offer.price, stacks);
        }

        if (rerollButtonText != null) rerollButtonText.text = "REROLL (" + shop.RerollCost + " G)";
        if (rerollButton != null) rerollButton.interactable = gold >= shop.RerollCost;
    }

    private void OnBuyClicked(int index)
    {
        ShopManager.BuyResult result = shop.TryBuy(index);
        switch (result)
        {
            case ShopManager.BuyResult.Success: ShowMessage("Purchased!"); break;
            case ShopManager.BuyResult.NotEnoughGold: ShowMessage("Not enough gold"); break;
            case ShopManager.BuyResult.MaxStacks: ShowMessage("Already maxed"); break;
            case ShopManager.BuyResult.SoldOut: ShowMessage("Sold out"); break;
        }
    }

    private void OnRerollClicked()
    {
        if (!shop.TryReroll()) ShowMessage("Not enough gold");
    }

    private void OnContinueClicked()
    {
        shop.CloseShop();
    }

    private void ShowMessage(string text)
    {
        if (messageText == null) return;
        messageText.text = text;
        messageTimer = string.IsNullOrEmpty(text) ? 0f : messageDuration;
    }
}
