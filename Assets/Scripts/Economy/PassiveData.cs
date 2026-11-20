using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>稀有度：越稀有越少出現在商店</summary>
public enum Rarity
{
    Common,
    Rare,
    Epic,
}

/// <summary>
/// 一個被動能力的資料（組員 E）。
/// 新增方式：Project 視窗右鍵 → Create → 組員E → 被動能力，然後在 Inspector 填資料。
/// 做好之後拖進 ShopManager 的 Item Pool，商店就有機會抽到它。
/// </summary>
[CreateAssetMenu(fileName = "Passive_New", menuName = "組員E/被動能力")]
public class PassiveData : ScriptableObject
{
    [Header("顯示")]
    [Tooltip("商店上顯示的名稱（目前字型只支援英文）")]
    public string displayName = "New Passive";

    [Tooltip("說明。留空的話會自動用下面的加成產生，例如 \"+1 Max HP\"")]
    [TextArea(2, 4)]
    public string description = "";

    [Tooltip("圖示（可以不填，會用稀有度顏色代替）")]
    public Sprite icon;

    [Header("商店")]
    [Min(0)] public int price = 20;
    public Rarity rarity = Rarity.Common;

    [Tooltip("最多能疊加幾次，0 = 不限")]
    [Min(0)] public int maxStacks = 0;

    [Header("效果")]
    public List<StatModifier> modifiers = new List<StatModifier>();

    /// <summary>取得說明文字（沒填的話自動產生）</summary>
    public string GetDescription()
    {
        if (!string.IsNullOrWhiteSpace(description)) return description;

        var sb = new StringBuilder();
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (i > 0) sb.Append('\n');
            sb.Append(modifiers[i].Describe());
        }
        return sb.ToString();
    }

    /// <summary>稀有度在畫面上的顏色</summary>
    public static Color RarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Rare: return new Color(0.25f, 0.55f, 0.95f);   // 藍
            case Rarity.Epic: return new Color(0.75f, 0.35f, 0.95f);   // 紫
            default: return new Color(0.75f, 0.68f, 0.5f);             // 沙色
        }
    }
}
