using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 把組員 A 的玩家血量顯示到畫面上（組員 E）。
///
/// A 目前的 PlayerController 只有兩個欄位：maxHealth（最大血量）、PlayercurrentHealth（目前血量），
/// 沒有「血量改變」的通知，所以這支程式每一幀去讀那兩個數字，有變才更新畫面。
///
/// 為什麼不直接寫 PlayerController？
///   因為 PlayerController 還沒合併進 main。如果直接寫，E 和 main 都會編譯失敗。
///   所以這裡用「名稱」去找他的程式和欄位，合併前不會報錯，合併後馬上能用。
///   如果 A 之後改了欄位名稱，只要在 Inspector 改名稱就好，不用改程式。
///
/// 另外也幫忙接上組員 E 的功能（都可以在 Inspector 關掉）：
///   - 商店的補血道具 → 直接加到 A 的目前血量
///   - 被動能力加血上限 → 改 A 的最大血量
///   - 血量歸零 → 打開失敗畫面
/// </summary>
public class PlayerHealthDisplay : MonoBehaviour
{
    [Header("玩家（留空 = 自動找 Tag 是 Player 的物件）")]
    [SerializeField] private GameObject player;
    [SerializeField] private string playerTag = "Player";

    [Header("A 的程式名稱與欄位名稱")]
    [SerializeField] private string componentName = "PlayerController";
    [SerializeField] private string currentHealthField = "PlayercurrentHealth";
    [SerializeField] private string maxHealthField = "maxHealth";

    [Header("顯示方式（用哪幾種就填哪幾種，不用的留空）")]
    [Tooltip("愛心")]
    [SerializeField] private HealthUI hearts;
    [Tooltip("血條（Slider）")]
    [SerializeField] private Slider bar;
    [Tooltip("數字，例如 3 / 5")]
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private string numberFormat = "{0} / {1}";
    [Tooltip("血條每秒移動幾滴血，0 = 直接跳到新數值")]
    [Min(0f)] [SerializeField] private float barSmoothSpeed = 8f;

    [Header("組員 E 的功能")]
    [Tooltip("商店的補血道具會加到 A 的目前血量")]
    [SerializeField] private bool applyHealing = true;
    [Tooltip("被動能力改變血量上限時，同步改 A 的最大血量")]
    [SerializeField] private bool syncMaxHealthFromStats = true;
    [Tooltip("血量歸零時打開失敗畫面")]
    [SerializeField] private bool showGameOverOnDeath = true;

    // 找到的 A 的程式與欄位
    private Component target;
    private FieldInfo currentInfo;
    private FieldInfo maxInfo;
    private bool loggedProblem;
    private float nextBindTime;

    // 上一次顯示的數字
    private int shownCurrent = -1;
    private int shownMax = -1;
    private float barValue;

    private bool subscribedStats;

    public bool IsBound => target != null && currentInfo != null && maxInfo != null;

    private void Start()
    {
        if (applyHealing)
        {
            PlayerHealing.OnHealRequested += HandleHealRequested;
            PlayerHealing.CanHeal = CanHeal;
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatsChanged += HandleStatsChanged;
            subscribedStats = true;
        }

        TryBind();
    }

    private void OnDestroy()
    {
        if (applyHealing)
        {
            PlayerHealing.OnHealRequested -= HandleHealRequested;
            if (PlayerHealing.CanHeal == (Func<bool>)CanHeal) PlayerHealing.CanHeal = null;
        }

        if (subscribedStats && PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatsChanged -= HandleStatsChanged;
        }
    }

    private void LateUpdate()
    {
        // 玩家可能比 UI 晚出現（例如關卡開始才生成），找不到就每 0.5 秒再找一次
        if (!IsBound)
        {
            if (Time.unscaledTime >= nextBindTime)
            {
                nextBindTime = Time.unscaledTime + 0.5f;
                TryBind();
            }
            if (!IsBound) return;
        }

        int current = ReadInt(currentInfo);
        int max = ReadInt(maxInfo);

        if (current != shownCurrent || max != shownMax)
        {
            bool justDied = shownCurrent > 0 && current <= 0;
            shownCurrent = current;
            shownMax = max;
            RefreshDisplay();

            if (justDied && showGameOverOnDeath && GameResultUI.Instance != null)
            {
                GameResultUI.Instance.ShowGameOver();
            }
        }

        // 血條平滑移動（用 unscaled，遊戲暫停時也會動）
        if (bar != null)
        {
            barValue = barSmoothSpeed > 0f
                ? Mathf.MoveTowards(barValue, shownCurrent, barSmoothSpeed * Time.unscaledDeltaTime)
                : shownCurrent;
            bar.value = barValue;
        }
    }

    // ===== 找到 A 的程式 =====

    private void TryBind()
    {
        target = null;

        GameObject go = player;
        if (go == null && !string.IsNullOrEmpty(playerTag))
        {
            go = GameObject.FindWithTag(playerTag);
        }
        if (go == null) return; // 玩家還沒出現，之後再找

        target = go.GetComponent(componentName);
        if (target == null)
        {
            LogOnce("在「" + go.name + "」上找不到程式「" + componentName + "」。" +
                    "（PlayerController 還沒合併進來的話，這是正常的）");
            return;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = target.GetType();
        currentInfo = type.GetField(currentHealthField, flags);
        maxInfo = type.GetField(maxHealthField, flags);

        if (currentInfo == null || maxInfo == null)
        {
            LogOnce(componentName + " 裡找不到欄位「" + currentHealthField + "」或「" + maxHealthField + "」，" +
                    "請到 Inspector 改成 A 現在用的名稱。");
            target = null;
            return;
        }

        loggedProblem = false;
        shownCurrent = -1;
        shownMax = -1;

        // 一開始就讓最大血量符合被動能力（例如上一層買過加血）
        ApplyStatsMaxHealth(true);

        barValue = ReadInt(currentInfo);
    }

    // ===== 畫面 =====

    private void RefreshDisplay()
    {
        int current = Mathf.Max(0, shownCurrent);
        int max = Mathf.Max(0, shownMax);

        if (hearts != null) hearts.SetHealth(current, max);

        if (bar != null)
        {
            bar.minValue = 0;
            bar.maxValue = Mathf.Max(1, max);
            if (barSmoothSpeed <= 0f) bar.value = current;
        }

        if (numberText != null) numberText.text = string.Format(numberFormat, current, max);
    }

    // ===== 組員 E 的功能 =====

    private bool CanHeal()
    {
        return IsBound && ReadInt(currentInfo) < ReadInt(maxInfo);
    }

    private void HandleHealRequested(int amount)
    {
        if (!IsBound) return;
        int max = ReadInt(maxInfo);
        int current = Mathf.Min(max, ReadInt(currentInfo) + amount);
        WriteInt(currentInfo, current);
    }

    private void HandleStatsChanged()
    {
        ApplyStatsMaxHealth(false);
    }

    private void ApplyStatsMaxHealth(bool onBind)
    {
        if (!syncMaxHealthFromStats || !IsBound || PlayerStats.Instance == null) return;

        int newMax = PlayerStats.Instance.MaxHealth;
        int oldMax = ReadInt(maxInfo);
        if (newMax == oldMax) return;

        int current = ReadInt(currentInfo);

        if (newMax > oldMax)
        {
            // 上限變大：多出來的那格直接補滿（剛找到玩家時，只有滿血才補）
            if (!onBind || current >= oldMax) current += newMax - oldMax;
        }

        current = Mathf.Clamp(current, 0, newMax);
        WriteInt(maxInfo, newMax);
        WriteInt(currentInfo, current);
    }

    // ===== 讀寫 A 的欄位（int 或 float 都可以）=====

    private int ReadInt(FieldInfo field)
    {
        object value = field.GetValue(target);
        if (value is int i) return i;
        if (value is float f) return Mathf.RoundToInt(f);
        return Convert.ToInt32(value);
    }

    private void WriteInt(FieldInfo field, int value)
    {
        if (field.FieldType == typeof(float)) field.SetValue(target, (float)value);
        else field.SetValue(target, value);
    }

    private void LogOnce(string message)
    {
        if (loggedProblem) return;
        loggedProblem = true;
        Debug.LogWarning("PlayerHealthDisplay：" + message, this);
    }
}
