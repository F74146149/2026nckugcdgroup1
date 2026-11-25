using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 遊戲開始前的主選單。
/// 這個腳本只提供「按鈕要做的事」，按鈕本身在 Unity 裡用 Button 的 On Click() 接上來。
///
/// 使用方式：
/// 1. 把這個腳本掛在主選單場景的 Canvas 上。
/// 2. 「開始遊戲」按鈕的 On Click() → 拖入 Canvas → 選 MainMenuUI.StartGame
/// 3. 「設定」按鈕 → MainMenuUI.OpenSettings；設定面板裡的「返回」按鈕 → MainMenuUI.CloseSettings
/// 4. 「離開」按鈕 → MainMenuUI.QuitGame
/// 5. 要切換的場景必須加進 File → Build Profiles 的 Scene List，不然會讀不到。
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("按下開始後要載入的場景名稱（要跟場景檔名一模一樣）")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("面板（可以不填）")]
    [SerializeField] private GameObject mainPanel;      // 放開始 / 設定 / 離開按鈕的面板
    [SerializeField] private GameObject settingsPanel;  // 設定面板

    private void Start()
    {
        // 從遊戲中回到主選單時，時間可能被暫停過（例如玩家死亡時 timeScale = 0），先恢復
        Time.timeScale = 1f;
        ShowMain();
    }

    /// <summary>開始遊戲：載入遊戲場景</summary>
    public void StartGame()
    {
        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError("MainMenuUI：找不到場景「" + gameSceneName + "」。" +
                           "請確認名稱正確，並且已加入 File → Build Profiles 的 Scene List。", this);
            return;
        }

        // 新的一局：金幣、被動能力歸零
        RunReset.ResetAll();
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>打開設定面板</summary>
    public void OpenSettings()
    {
        if (settingsPanel == null) return;
        if (mainPanel != null) mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    /// <summary>關閉設定面板，回到主選單</summary>
    public void CloseSettings()
    {
        ShowMain();
    }

    /// <summary>離開遊戲</summary>
    public void QuitGame()
    {
        Debug.Log("離開遊戲");
#if UNITY_EDITOR
        // 在 Unity 編輯器裡測試時，停止 Play
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowMain()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}
