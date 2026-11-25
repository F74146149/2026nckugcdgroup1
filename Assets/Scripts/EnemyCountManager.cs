using UnityEngine;

/// <summary>
/// 記錄這一波還剩幾隻敵人（敵人死掉時會呼叫 EnemyDefeated）。
///
/// 數量由 WaveManager 每一波設定。全部打完之後要做什麼（開商店、進下一間房間）
/// 交給 WaveManager / GameManager 決定，所以這裡不再把遊戲暫停。
/// </summary>
public class EnemyCountManager : MonoBehaviour
{
    public int enemyCount = 3;

    public void EnemyDefeated()
    {
        enemyCount = Mathf.Max(0, enemyCount - 1);

        if (enemyCount == 0)
        {
            Debug.Log("All Enemies Defeated");
        }
    }
}
