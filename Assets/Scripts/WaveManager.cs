using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private RoomManager roomManager;
    private int waves=3; 

    void Start()
    {
        countwavs();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SpawnEnemy()
    {
        if (enemy == null || roomManager == null)
        {
            Debug.LogError("Assign the enemy prefab and RoomManager in the Inspector.", this);
            return;
        }

        int interiorWidth = roomManager.map_width - 2;
        int interiorHeight = roomManager.map_height - 2;
        if (interiorWidth <= 0 || interiorHeight <= 0)
        {
            Debug.LogError("The map must be at least 3 by 3 to spawn enemies.", this);
            return;
        }

        List<Vector3> availableSpawnPoints = new List<Vector3>(roomManager.GetEnemySpawnPoints());
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("No floor spawn points are available for enemies.", this);
            return;
        }

        int enemyCount = Mathf.Min(interiorWidth * interiorHeight / 10, availableSpawnPoints.Count);

        for (int i = 0; i < enemyCount; i++)
        {
            int spawnPointIndex = Random.Range(0, availableSpawnPoints.Count);
            Vector3 position = availableSpawnPoints[spawnPointIndex];
            availableSpawnPoints.RemoveAt(spawnPointIndex);
            Instantiate(enemy, position, Quaternion.identity, transform);
        }
    }

    void countwavs()
    {
        for(int i=0;i<waves;i++)
        {
            if (gameObject.transform.childCount == 0)
            {
                SpawnEnemy();
            }
        }
    }
}
