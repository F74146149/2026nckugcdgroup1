using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 勝利 / 失敗畫面（組員 E）。
///
/// 給隊友用的：
///   GameResultUI.Instance.ShowGameOver();   // 玩家死亡時（組員 A）
///   GameResultUI.Instance.ShowVictory();    // 打完最後一層時（組員 D）
///
/// 按「重來」：金幣與被動能力歸零，重新載入遊戲場景。
/// 按「主選單」：歸零後回到主選單。
///
/// 注意：這個腳本要掛在一直開著的物件上，不要掛在 Panel 本身。
/// </summary>
public class GameResultUI : MonoBehaviour
{
    public static GameResultUI Instance { get; private set; }

    [Header("畫面")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text detailText;

    [Header("按鈕")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [Header("場景")]
    [Tooltip("按重來時載入的場景；留空 = 重新載入目前的場景")]
    [SerializeField] private string gameSceneName = "";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("文字")]
    [SerializeField] private string gameOverTitle = "YOU DIED";
    [SerializeField] private string victoryTitle = "VICTORY";
    [SerializeField] private Color gameOverColor = new Color(0.85f, 0.2f, 0.2f);
    [SerializeField] private Color victoryColor = new Color(1f, 0.82f, 0.3f);

    /// <summary>
    /// 按重來、但場景沒辦法重新載入時觸發（例如測試場景沒加進 Build Profiles）。
    /// 測試腳本可以在這裡自己把玩家狀態重設。
    /// </summary>
    public event Action OnRetryWithoutReload;

    public bool IsShowing { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameResultUI：場景裡有兩個 GameResultUI。", this);
        }
        Instance = this;

        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>顯示失敗畫面</summary>
    public void ShowGameOver()
    {
        if (IsShowing) return;
        RunRecord.RecordDeath();
        Show(gameOverTitle, gameOverColor);
    }

    /// <summary>顯示勝利畫面</summary>
    public void ShowVictory()
    {
        if (IsShowing) return;
        RunRecord.RecordClear();
        Show(victoryTitle, victoryColor);
    }

    private void Show(string title, Color color)
    {
        IsShowing = true;

        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = color;
        }

        if (detailText != null)
        {
            int gold = CurrencyManager.Instance != null ? CurrencyManager.Instance.Gold : 0;
            detailText.text =
                "Gold: " + gold + "\n" +
                "Runs: " + RunRecord.TotalRuns + "    Clears: " + RunRecord.Clears;
        }

        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Hide()
    {
        IsShowing = false;
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
    }

    /// <summary>重來（按鈕會呼叫）</summary>
    public void Retry()
    {
        Hide();
        RunReset.ResetAll();

        string target = string.IsNullOrEmpty(gameSceneName) ? SceneManager.GetActiveScene().name : gameSceneName;
        if (Application.CanStreamedLevelBeLoaded(target))
        {
            SceneManager.LoadScene(target);
        }
        else
        {
            Debug.LogWarning("GameResultUI：場景「" + target + "」不在 File → Build Profiles 的 Scene List 裡，" +
                             "所以不重新載入場景，只把狀態歸零。", this);
            OnRetryWithoutReload?.Invoke();
        }
    }

    /// <summary>回主選單（按鈕會呼叫）</summary>
    public void GoToMainMenu()
    {
        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError("GameResultUI：找不到場景「" + mainMenuSceneName + "」，" +
                           "請把它加進 File → Build Profiles 的 Scene List。", this);
            return;
        }

        Hide();
        RunReset.ResetAll();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
