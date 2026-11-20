using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 顯示玩家目前的能力值與擁有的被動能力（組員 E）。
/// 可以放在 HUD 角落，或放在商店畫面旁邊讓玩家比較。
/// 掛在一個 Text - TextMeshPro 上即可。
/// </summary>
public class StatsPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statsText;

    [Tooltip("要不要列出擁有的被動能力")]
    [SerializeField] private bool showPassives = true;

    private PlayerStats stats;
    private PassiveManager passives;

    private void Awake()
    {
        if (statsText == null) statsText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        stats = PlayerStats.Instance;
        passives = PassiveManager.Instance;

        if (stats != null) stats.OnStatsChanged += Refresh;
        if (passives != null)
        {
            passives.OnPassiveAdded += HandlePassiveAdded;
            passives.OnPassivesCleared += Refresh;
        }

        Refresh();
    }

    private void OnDestroy()
    {
        if (stats != null) stats.OnStatsChanged -= Refresh;
        if (passives != null)
        {
            passives.OnPassiveAdded -= HandlePassiveAdded;
            passives.OnPassivesCleared -= Refresh;
        }
    }

    private void HandlePassiveAdded(PassiveData passive, int stacks)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (statsText == null) return;

        if (stats == null)
        {
            statsText.text = "(No PlayerStats in scene)";
            return;
        }

        var sb = new StringBuilder();
        sb.Append("Max HP      ").Append(stats.MaxHealth).Append('\n');
        sb.Append("Attack      ").Append((stats.AttackMultiplier * 100f).ToString("0")).Append("%\n");
        sb.Append("Move Speed  ").Append(stats.MoveSpeed.ToString("0.0")).Append('\n');
        sb.Append("Crit        ").Append((stats.CritChance * 100f).ToString("0")).Append("%\n");
        sb.Append("Luck        ").Append(stats.Luck.ToString("0.#")).Append('\n');
        sb.Append("Loot        ").Append((stats.LootMultiplier * 100f).ToString("0")).Append('%');

        if (showPassives && passives != null && passives.Owned.Count > 0)
        {
            sb.Append("\n\nPassives");
            foreach (PassiveData p in passives.Owned)
            {
                int n = passives.GetStacks(p);
                sb.Append("\n- ").Append(p.displayName);
                if (n > 1) sb.Append(" x").Append(n);
            }
        }

        statsText.text = sb.ToString();
    }
}
