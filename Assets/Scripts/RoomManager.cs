using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private const float RockFreeRadius = 1f;

    [SerializeField] GameObject floor;
    [SerializeField] GameObject wall;

    [SerializeField] GameObject rock;
    [SerializeField] GameObject water;

    public int map_height;
    public int map_width;
    private readonly List<Vector3> enemySpawnPoints = new List<Vector3>();

    public void GenerateMap()
    {
        map_width = Random.Range(16, 64);
        map_height = Random.Range(10, 40);
        if (floor == null || wall == null || rock == null || water == null)
        {
            Debug.LogError("Assign floor, wall, rock, and water prefabs in the Inspector.", this);
            return;
        }

        // 進下一間房間時，先把上一張地圖清掉
        ClearMap();

        float startX = -(map_width - 1) / 2f;
        float startY = -(map_height - 1) / 2f;
        GameObject[,] terrain = new GameObject[map_width, map_height];
        enemySpawnPoints.Clear();
        PlaceClusters(terrain, rock, Random.Range(2, 4), true);
        // 水也避開正中間，不然玩家一出生可能卡在水裡
        PlaceClusters(terrain, water, Random.Range(2, 4), true);

        for (int x = 0; x < map_width; x++)
        {
            for (int y = 0; y < map_height; y++)
            {
                bool isBorder = x == 0 || x == map_width - 1 || y == 0 || y == map_height - 1;
                GameObject prefab;

                if (isBorder)
                {
                    prefab = wall;
                }
                else
                {
                    prefab = terrain[x, y] ?? floor;
                }

                Vector3 position = new Vector3(startX + x, startY + y, 10f);
                if (!isBorder && terrain[x, y] == null)
                {
                    enemySpawnPoints.Add(new Vector3(position.x,position.y,0f));
                }

                Instantiate(prefab, position, Quaternion.identity, transform);
            }
        }
    }

    public Vector3[] GetEnemySpawnPoints()
    {
        return enemySpawnPoints.ToArray();
    }

    /// <summary>刪掉目前的地圖</summary>
    public void ClearMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void PlaceClusters(GameObject[,] terrain, GameObject prefab, int clusterCount, bool avoidOrigin = false)
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int clusterIndex = 0; clusterIndex < clusterCount; clusterIndex++)
        {
            List<Vector2Int> availableCells = new List<Vector2Int>();
            for (int x = 1; x < map_width - 1; x++)
            {
                for (int y = 1; y < map_height - 1; y++)
                {
                    if (terrain[x, y] == null && (!avoidOrigin || !IsInRockFreeArea(x, y)))
                    {
                        availableCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (availableCells.Count == 0)
            {
                break;
            }

            Vector2Int seed = availableCells[Random.Range(0, availableCells.Count)];
            terrain[seed.x, seed.y] = prefab;
            List<Vector2Int> cluster = new List<Vector2Int> { seed };
            int clusterSize = Random.Range(6, 15);

            while (cluster.Count < clusterSize)
            {
                List<Vector2Int> frontier = new List<Vector2Int>();
                foreach (Vector2Int cell in cluster)
                {
                    foreach (Vector2Int direction in directions)
                    {
                        Vector2Int neighbor = cell + direction;
                        if (neighbor.x > 0 && neighbor.x < map_width - 1
                            && neighbor.y > 0 && neighbor.y < map_height - 1
                            && terrain[neighbor.x, neighbor.y] == null
                            && (!avoidOrigin || !IsInRockFreeArea(neighbor.x, neighbor.y)))
                        {
                            frontier.Add(neighbor);
                        }
                    }

                }

                if (frontier.Count == 0)
                {
                    break;
                }

                Vector2Int next = frontier[Random.Range(0, frontier.Count)];
                terrain[next.x, next.y] = prefab;
                cluster.Add(next);
            }
        }
    }

    bool IsInRockFreeArea(int x, int y)
    {
        float worldX = x - (map_width - 1) / 2f;
        float worldY = y - (map_height - 1) / 2f;
        return Mathf.Abs(worldX) <= RockFreeRadius && Mathf.Abs(worldY) <= RockFreeRadius;
    }
}
