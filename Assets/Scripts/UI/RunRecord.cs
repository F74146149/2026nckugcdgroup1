using UnityEngine;

/// <summary>
/// 輪迴紀錄（存在玩家電腦上，關掉遊戲也不會消失）。
/// README：「記錄玩家成功通關的輪迴次數」。
/// </summary>
public static class RunRecord
{
    private const string DeathsKey = "RunRecord.Deaths";
    private const string ClearsKey = "RunRecord.Clears";

    /// <summary>死亡次數（失敗的輪迴）</summary>
    public static int Deaths => PlayerPrefs.GetInt(DeathsKey, 0);

    /// <summary>通關次數</summary>
    public static int Clears => PlayerPrefs.GetInt(ClearsKey, 0);

    /// <summary>總共輪迴幾次</summary>
    public static int TotalRuns => Deaths + Clears;

    public static void RecordDeath()
    {
        PlayerPrefs.SetInt(DeathsKey, Deaths + 1);
        PlayerPrefs.Save();
    }

    public static void RecordClear()
    {
        PlayerPrefs.SetInt(ClearsKey, Clears + 1);
        PlayerPrefs.Save();
    }

    /// <summary>清除所有紀錄（測試用）</summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(DeathsKey);
        PlayerPrefs.DeleteKey(ClearsKey);
        PlayerPrefs.Save();
    }
}
