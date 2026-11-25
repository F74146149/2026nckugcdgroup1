using TMPro;
using UnityEngine;

/// <summary>
/// 畫面上方顯示「Room 1 / 3    Wave 2 / 3」（組員 E）。
/// 自動找場景裡的 GameManager、WaveManager；找不到（例如測試場景）就不顯示。
/// </summary>
public class RunProgressUI : MonoBehaviour
{
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private string format = "Room {0} / {1}    Wave {2} / {3}";
    [SerializeField] private string clearedFormat = "Room {0} / {1}    Cleared!";

    private GameManager game;
    private WaveManager waves;
    private float nextSearchTime;
    private string shown;

    private void Update()
    {
        if (progressText == null) return;

        if ((game == null || waves == null) && Time.unscaledTime >= nextSearchTime)
        {
            nextSearchTime = Time.unscaledTime + 1f;
            if (game == null) game = FindFirstObjectByType<GameManager>();
            if (waves == null) waves = FindFirstObjectByType<WaveManager>();
        }

        string text = "";
        if (game != null && waves != null && game.CurrentRoom > 0)
        {
            text = waves.spawning
                ? string.Format(format, game.CurrentRoom, game.RoomsToWin, Mathf.Max(1, waves.CurrentWave), waves.TotalWaves)
                : string.Format(clearedFormat, game.CurrentRoom, game.RoomsToWin);
        }

        if (text != shown)
        {
            shown = text;
            progressText.text = text;
        }
    }
}
