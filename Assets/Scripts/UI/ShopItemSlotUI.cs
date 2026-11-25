using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店裡的一張商品卡片（組員 E）。
/// 由 ShopUI 複製產生，不用自己呼叫。
/// </summary>
public class ShopItemSlotUI : MonoBehaviour
{
    [Header("文字")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text rarityText;

    [Header("圖片")]
    [Tooltip("能力圖示；沒有圖示時會顯示稀有度顏色")]
    [SerializeField] private Image iconImage;
    [Tooltip("卡片外框，會變成稀有度顏色")]
    [SerializeField] private Image frameImage;

    [Header("購買")]
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;
    [Tooltip("賣掉後蓋在卡片上的物件（可以不填）")]
    [SerializeField] private GameObject soldOverlay;

    private int index;
    private Action<int> onBuy;

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(() => onBuy?.Invoke(index));
        }
    }

    /// <summary>把一格商品的資料顯示到卡片上</summary>
    public void Bind(int slotIndex, ShopManager.Offer offer, Action<int> buyCallback, bool canAfford, int ownedStacks)
    {
        index = slotIndex;
        onBuy = buyCallback;

        PassiveData item = offer.item;
        Color rarityColor = PassiveData.RarityColor(item.rarity);

        if (nameText != null) nameText.text = item.displayName;
        if (descriptionText != null) descriptionText.text = item.GetDescription();
        if (rarityText != null)
        {
            string stackInfo = ownedStacks > 0 ? "  (Owned x" + ownedStacks + ")" : "";
            string kind = item.consumable ? " ITEM" : "";
            rarityText.text = item.rarity.ToString().ToUpper() + kind + stackInfo;
            rarityText.color = rarityColor;
        }

        if (priceText != null)
        {
            priceText.text = offer.sold ? "-" : offer.price + " G";
            priceText.color = (canAfford || offer.sold) ? Color.white : new Color(1f, 0.4f, 0.4f);
        }

        if (iconImage != null)
        {
            if (item.icon != null)
            {
                iconImage.sprite = item.icon;
                iconImage.color = Color.white;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.color = rarityColor;
            }
        }

        if (frameImage != null) frameImage.color = rarityColor;

        if (buyButton != null) buyButton.interactable = !offer.sold && canAfford;
        if (buyButtonText != null) buyButtonText.text = offer.sold ? "SOLD" : "BUY";
        if (soldOverlay != null) soldOverlay.SetActive(offer.sold);
    }
}
