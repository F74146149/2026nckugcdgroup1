using System;
using UnityEngine;

/// <summary>
/// 整局遊戲的流程（組員 D）。
///
///   進場景 → 第 1 間房間：生成地圖、玩家回到中間、一波一波生敵人
///   → 這間房間的敵人全部打完 → 打開商店
///   → 玩家在商店按 Continue → 下一間房間（地圖重新生成、商品變貴一點）
///   → 打完最後一間 → 勝利畫面
///
/// 玩家死掉時，組員 E 的 PlayerHealthDisplay 會打開失敗畫面，這裡不用管。
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private Transform player;

    [Header("流程")]
    [Tooltip("進場景就自動開始第 1 間房間")]
    [SerializeField] private bool startOnPlay = true;
    [Tooltip("打完幾間房間算勝利")]
    [Min(1)] [SerializeField] private int roomsToWin = 3;
    [Tooltip("每多一間房間，商店價格增加多少（0.25 = 貴 25%）")]
    [Min(0f)] [SerializeField] private float priceIncreasePerRoom = 0.25f;

    /// <summary>現在是第幾間房間（從 1 開始）</summary>
    public int CurrentRoom { get; private set; }
    public int RoomsToWin => roomsToWin;

    /// <summary>新房間開始時觸發（第幾間, 總共幾間），可以拿來顯示「Room 1 / 3」</summary>
    public event Action<int, int> OnRoomStarted;
    /// <summary>房間的敵人全部打完時觸發（第幾間）</summary>
    public event Action<int> OnRoomCleared;

    // 商店是不是「房間打完」才打開的；測試鍵 O 打開的商店關掉時不換房間
    private bool waitingForShop;
    private ShopManager subscribedShop;
    private bool subscribedWaves;

    private void Start()
    {
        if (startOnPlay) StartGame();
    }

    private void OnDestroy()
    {
        if (subscribedWaves && waveManager != null) waveManager.OnAllWavesCleared -= HandleAllWavesCleared;
        if (subscribedShop != null) subscribedShop.OnShopClosed -= HandleShopClosed;
    }

    /// <summary>從第 1 間房間開始</summary>
    public void StartGame()
    {
        if (roomManager == null || waveManager == null || player == null)
        {
            Debug.LogError("Assign the RoomManager, WaveManager, and player in the Inspector.", this);
            return;
        }

        Subscribe();
        CurrentRoom = 0;
        waitingForShop = false;
        LoadNextRoom();
    }

    private void LoadNextRoom()
    {
        CurrentRoom++;

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.PriceMultiplier = 1f + (CurrentRoom - 1) * priceIncreasePerRoom;
        }

        roomManager.GenerateMap();
        MovePlayerToStart();
        waveManager.StartWaves();

        Debug.Log("Room " + CurrentRoom + " / " + roomsToWin);
        OnRoomStarted?.Invoke(CurrentRoom, roomsToWin);
    }

    private void MovePlayerToStart()
    {
        player.position = Vector3.zero;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void HandleAllWavesCleared()
    {
        OnRoomCleared?.Invoke(CurrentRoom);

        // 最後一間 → 勝利
        if (CurrentRoom >= roomsToWin)
        {
            if (GameResultUI.Instance != null) GameResultUI.Instance.ShowVictory();
            else Debug.Log("Victory!");
            return;
        }

        // 還沒打完 → 開商店，玩家按 Continue 之後才進下一間
        if (ShopManager.Instance != null)
        {
            waitingForShop = true;
            ShopManager.Instance.OpenShop();
        }
        else
        {
            LoadNextRoom();
        }
    }

    private void HandleShopClosed()
    {
        if (!waitingForShop) return;
        waitingForShop = false;

        // 商店開著時被打開了失敗畫面，就不進下一間
        if (GameResultUI.Instance != null && GameResultUI.Instance.IsShowing) return;

        LoadNextRoom();
    }

    private void Subscribe()
    {
        if (!subscribedWaves)
        {
            waveManager.OnAllWavesCleared += HandleAllWavesCleared;
            subscribedWaves = true;
        }

        if (subscribedShop == null && ShopManager.Instance != null)
        {
            subscribedShop = ShopManager.Instance;
            subscribedShop.OnShopClosed += HandleShopClosed;
        }
    }
}
