using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] GameObject floor;
    [SerializeField] GameObject wall;

    [SerializeField] GameObject rock;
    [SerializeField] GameObject water;

    int map_height;
    int map_width;

    void Start()
    {
        map_width = Random.Range(16, 64);
        map_height = Random.Range(10, 40);
        GenerateMap();
    }

    void StartGenerateMap()
    {
        if (floor == null || wall == null || rock == null || water == null)
        {
            Debug.LogError("Assign floor, wall, rock, and water prefabs in the Inspector.", this);
            return;
        }

        float startX = -(map_width - 1) / 2f;
        float startY = -(map_height - 1) / 2f;
        GameObject[,] terrain = new GameObject[map_width, map_height];
        PlaceClusters(terrain, rock, Random.Range(2, 4));
        PlaceClusters(terrain, water, Random.Range(2, 4));

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

                Vector3 position = new Vector3(startX + x, startY + y, 0f);
                Instantiate(prefab, position, Quaternion.identity, transform);
            }
        }
    }

    void PlaceClusters(GameObject[,] terrain, GameObject prefab, int clusterCount)
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
                    if (terrain[x, y] == null)
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
                            && terrain[neighbor.x, neighbor.y] == null)
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
}
