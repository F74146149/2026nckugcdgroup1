using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int enemyCount = 3;

    public void EnemyDefeated()
    {
        enemyCount--;

        if (enemyCount <= 0)
        {
            Debug.Log("All Enemies Defeated");

            Time.timeScale = 0f;
        }
    }
}
