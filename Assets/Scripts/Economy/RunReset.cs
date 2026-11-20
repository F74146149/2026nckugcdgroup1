/// <summary>
/// 新的輪迴開始時，把組員 E 負責的東西全部歸零。
/// 任何人都可以呼叫：RunReset.ResetAll();
/// （例如玩家死亡後按「重來」、或從主選單開始新遊戲）
/// </summary>
public static class RunReset
{
    public static void ResetAll()
    {
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.ResetGold();

        // 清空被動能力時，會一併移除 PlayerStats 上對應的加成
        if (PassiveManager.Instance != null) PassiveManager.Instance.ClearAll();

        // 保險起見，把其他來源的加成也清掉
        if (PlayerStats.Instance != null) PlayerStats.Instance.ClearAll();
    }
}
