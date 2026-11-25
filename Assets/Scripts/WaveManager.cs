using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一波一波生敵人（組員 D）。
///
///   StartWaves() → 生第 1 波 → 這波全部死掉 → 等一下 → 生下一波 → …
///   → 最後一波也打完 → 發出 OnAllWavesCleared（GameManager 收到後開商店）
///
/// 生出來的敵人都放在 WaveManager 底下，用「底下還剩幾隻」判斷這波打完了沒，
/// 所以敵人的程式不用特別通知這裡，只要死的時候 Destroy 自己就好。
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Tooltip("會隨機從這些敵人裡挑")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [Tooltip("留空會自動找場景裡的 RoomManager")]
    [SerializeField] private RoomManager roomManager;

    [Header("波數")]
    [Min(1)] [SerializeField] private int waves = 3;
    [Tooltip("一波打完到下一波之間等幾秒")]
    [Min(0f)] [SerializeField] private float timeBetweenWaves = 1.5f;

    [Header("每波敵人數量")]
    [Tooltip("地圖內部每幾格生 1 隻敵人（數字越小敵人越多）")]
    [Min(1)] [SerializeField] private int tilesPerEnemy = 10;
    [Tooltip("一波最多幾隻")]
    [Min(1)] [SerializeField] private int maxEnemiesPerWave = 6;
    [Tooltip("離玩家至少幾格才會生敵人，避免一出生就撞到玩家")]
    [Min(0f)] [SerializeField] private float safeRadius = 3f;

    /// <summary>正在打波次中（還沒全部打完）</summary>
    [NonSerialized] public bool spawning = false;

    /// <summary>現在是第幾波（從 1 開始）</summary>
    public int CurrentWave { get; private set; }
    public int TotalWaves => waves;
    /// <summary>場上還活著幾隻</summary>
    public int AliveEnemies => CountAliveEnemies();

    /// <summary>新的一波開始（第幾波, 總共幾波）</summary>
    public event Action<int, int> OnWaveStarted;
    /// <summary>所有波次都打完了</summary>
    public event Action OnAllWavesCleared;

    private bool waitingForNextWave;
    private float nextWaveTimer;
    private Transform player;
    private EnemyCountManager enemyCounter;

    private void Awake()
    {
        if (roomManager == null) roomManager = FindFirstObjectByType<RoomManager>();
    }

    /// <summary>從第 1 波開始</summary>
    public void StartWaves()
    {
        ClearEnemies();
        CurrentWave = 0;
        waitingForNextWave = false;
        spawning = true;
        SpawnNextWave();
    }

    /// <summary>舊的名字，跟 StartWaves 一樣</summary>
    public void countwaves()
    {
        StartWaves();
    }

    private void Update()
    {
        if (!spawning) return;

        if (waitingForNextWave)
        {
            nextWaveTimer -= Time.deltaTime;
            if (nextWaveTimer <= 0f)
            {
                waitingForNextWave = false;
                SpawnNextWave();
            }
            return;
        }

        if (CountAliveEnemies() > 0) return;

        // 這一波打完了
        if (CurrentWave >= waves)
        {
            spawning = false;
            Debug.Log("All waves cleared");
            OnAllWavesCleared?.Invoke();
        }
        else
        {
            waitingForNextWave = true;
            nextWaveTimer = timeBetweenWaves;
        }
    }

    private void SpawnNextWave()
    {
        CurrentWave++;
        int count = SpawnEnemy();

        // 讓舊的 EnemyCountManager 也知道這波有幾隻
        if (enemyCounter == null) enemyCounter = FindFirstObjectByType<EnemyCountManager>();
        if (enemyCounter != null) enemyCounter.enemyCount = count;

        Debug.Log("Wave " + CurrentWave + " / " + waves + ": " + count + " enemies");
        OnWaveStarted?.Invoke(CurrentWave, waves);
    }

    /// <summary>生一波敵人，回傳生了幾隻</summary>
    private int SpawnEnemy()
    {
        if (roomManager == null)
        {
            Debug.LogError("Assign the RoomManager in the Inspector.", this);
            return 0;
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
            return 0;
        }

        int interiorWidth = roomManager.map_width - 2;
        int interiorHeight = roomManager.map_height - 2;
        if (interiorWidth <= 0 || interiorHeight <= 0)
        {
            Debug.LogError("The map must be at least 3 by 3 to spawn enemies.", this);
            return 0;
        }

        // 只用離玩家夠遠的地板
        Vector3 playerPos = GetPlayerPosition();
        List<Vector3> availableSpawnPoints = new List<Vector3>();
        foreach (Vector3 point in roomManager.GetEnemySpawnPoints())
        {
            Vector2 offset = point - playerPos;
            if (offset.magnitude >= safeRadius) availableSpawnPoints.Add(point);
        }

        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("No floor spawn points are available for enemies.", this);
            return 0;
        }

        int enemyCount = Mathf.Clamp(interiorWidth * interiorHeight / tilesPerEnemy, 1, maxEnemiesPerWave);
        enemyCount = Mathf.Min(enemyCount, availableSpawnPoints.Count);

        for (int i = 0; i < enemyCount; i++)
        {
            int spawnPointIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
            Vector3 position = availableSpawnPoints[spawnPointIndex];
            availableSpawnPoints.RemoveAt(spawnPointIndex);
            GameObject enemyPrefab = validEnemyPrefabs[UnityEngine.Random.Range(0, validEnemyPrefabs.Count)];
            Instantiate(enemyPrefab, position, Quaternion.identity, transform);
        }

        return enemyCount;
    }

    private int CountAliveEnemies()
    {
        int alive = 0;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy) alive++;
        }
        return alive;
    }

    private void ClearEnemies()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private Vector3 GetPlayerPosition()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindWithTag("Player");
            if (found != null) player = found.transform;
        }
        return player != null ? player.position : Vector3.zero;
    }
}
