using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 被動能力管理（組員 E）：記錄玩家擁有哪些被動、各疊了幾層，並把效果加到 PlayerStats。
///
/// 常用方法：
///   PassiveManager.Instance.TryAdd(passive);    // 取得一個被動（商店買到、Boss 掉落都可以用）
///   PassiveManager.Instance.GetStacks(passive); // 目前疊了幾層
///   PassiveManager.Instance.ClearAll();         // 新的輪迴開始時清空
/// </summary>
public class PassiveManager : MonoBehaviour
{
    public static PassiveManager Instance { get; private set; }

    [Header("切換場景時保留")]
    [SerializeField] private bool keepBetweenScenes = true;

    /// <summary>取得被動能力時觸發，參數是（能力, 目前層數）</summary>
    public event Action<PassiveData, int> OnPassiveAdded;

    /// <summary>被動能力被清空時觸發</summary>
    public event Action OnPassivesCleared;

    private readonly Dictionary<PassiveData, int> stacks = new Dictionary<PassiveData, int>();
    private readonly List<PassiveData> order = new List<PassiveData>(); // 取得順序，給 UI 顯示用

    /// <summary>目前擁有的被動能力（依取得順序）</summary>
    public IReadOnlyList<PassiveData> Owned => order;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (keepBetweenScenes)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public int GetStacks(PassiveData passive)
    {
        if (passive == null) return 0;
        return stacks.TryGetValue(passive, out int n) ? n : 0;
    }

    /// <summary>還能不能再拿這個被動（有沒有到疊加上限）</summary>
    public bool CanAdd(PassiveData passive)
    {
        if (passive == null) return false;
        return passive.maxStacks <= 0 || GetStacks(passive) < passive.maxStacks;
    }

    /// <summary>取得一個被動能力，成功回傳 true</summary>
    public bool TryAdd(PassiveData passive)
    {
        if (!CanAdd(passive)) return false;

        int n = GetStacks(passive) + 1;
        stacks[passive] = n;
        if (n == 1) order.Add(passive);

        // 每疊一層就再加一次效果；來源都是這個 passive，清除時可以整批移除
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddModifiers(passive, passive.modifiers);
        }
        else
        {
            Debug.LogWarning("PassiveManager：場景裡沒有 PlayerStats，被動能力的效果不會生效。", this);
        }

        OnPassiveAdded?.Invoke(passive, n);
        return true;
    }

    /// <summary>清空所有被動能力（新的輪迴開始時）</summary>
    public void ClearAll()
    {
        if (PlayerStats.Instance != null)
        {
            foreach (PassiveData p in order)
            {
                PlayerStats.Instance.RemoveAllFrom(p);
            }
        }

        stacks.Clear();
        order.Clear();
        OnPassivesCleared?.Invoke();
    }
}
