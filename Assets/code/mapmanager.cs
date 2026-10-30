using UnityEngine;

public class gamemanager : MonoBehaviour
{
    [SerializeField] GameObject floor;
    [SerializeField] GameObject wall;

    [SerializeField] GameObject rock;
    [SerializeField] GameObject water;

    int height;
    int width;

    void Start()
    {
        width = Random.Range(16, 32);
        height = Random.Range(10, 20);
        GenerateMap();
    }

    void GenerateMap()
    {
        if (floor == null || wall == null || rock == null || water == null)
        {
            Debug.LogError("Assign floor, wall, rock, and water prefabs in the Inspector.", this);
            return;
        }

        float startX = -(width - 1) / 2f;
        float startY = -(height - 1) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isBorder = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                GameObject prefab;

                if (isBorder)
                {
                    prefab = wall;
                }
                else
                {
                    float tileRoll = Random.value;
                    prefab = tileRoll < 0.1f ? rock : tileRoll < 0.2f ? water : floor;
                }

                Vector3 position = new Vector3(startX + x, startY + y, 0f);
                Instantiate(prefab, position, Quaternion.identity, transform);
            }
        }
    }
}
