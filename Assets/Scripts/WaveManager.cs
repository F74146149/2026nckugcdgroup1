using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private RoomManager roomManager;
    private int waves=3; 
    public bool spawning=false;

    // Update is called once per frame
    void Update()
    {
        
    }
    void SpawnEnemy()
    {
        if (roomManager == null)
        {
            Debug.LogError("Assign the RoomManager in the Inspector.", this);
            return;
        }

        List<GameObject> validEnemyPrefabs = new List<GameObject>();
        if (enemyPrefabs != null)
        {
            foreach (GameObject enemyPrefab in enemyPrefabs)
            {
                if (enemyPrefab != null)
                {
                    validEnemyPrefabs.Add(enemyPrefab);
                }
            }
        }

        if (validEnemyPrefabs.Count == 0)
        {
            Debug.LogError("Assign at least one enemy prefab in the Inspector.", this);
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
            GameObject enemyPrefab = validEnemyPrefabs[Random.Range(0, validEnemyPrefabs.Count)];
            Instantiate(enemyPrefab, position, Quaternion.identity, transform);
        }
    }

    public void countwaves()
    {
        spawning=true;
        for(int i=0;i<waves;i++)
        {
            if (gameObject.transform.childCount == 0)
            {
                SpawnEnemy();
            }
        }
        spawning=false;
    }
}
