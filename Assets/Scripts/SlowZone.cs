using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 會讓人變慢的地形，例如水。玩家和敵人都可以走進去，但在裡面移動速度會變慢。
///
/// 怎麼用：
///   1. 地形的 Collider2D 勾 Is Trigger（才不會擋住人）
///   2. 加上這個元件，設定 Speed Multiplier（0.5 = 速度變一半）
///   3. 移動的程式把速度乘上 SlowZone.MultiplierFor(rb)
///      （PlayerController、Enemycontroller、EnemyFollow 都已經乘了）
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SlowZone : MonoBehaviour
{
    [Tooltip("在裡面的速度是原本的幾倍（0.5 = 一半）")]
    [Range(0.05f, 1f)] [SerializeField] private float speedMultiplier = 0.5f;

    public float SpeedMultiplier => speedMultiplier;

    // 每個物體現在踩在哪些 SlowZone 裡（水是一格一格的，可能同時踩到好幾格）
    private static readonly Dictionary<Rigidbody2D, List<SlowZone>> inside = new Dictionary<Rigidbody2D, List<SlowZone>>();

    /// <summary>這個物體現在的速度倍率：不在任何 SlowZone 裡 = 1</summary>
    public static float MultiplierFor(Rigidbody2D body)
    {
        if (body == null) return 1f;
        if (!inside.TryGetValue(body, out List<SlowZone> zones)) return 1f;

        float multiplier = 1f;
        for (int i = zones.Count - 1; i >= 0; i--)
        {
            if (zones[i] == null)
            {
                zones.RemoveAt(i);
                continue;
            }
            multiplier = Mathf.Min(multiplier, zones[i].speedMultiplier);
        }
        return multiplier;
    }

    /// <summary>這個物體是不是正踩在會變慢的地形上</summary>
    public static bool IsInside(Rigidbody2D body)
    {
        return MultiplierFor(body) < 1f;
    }

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只算身體：武器判定範圍、子彈這種 Trigger 不算
        if (other.isTrigger) return;
        Rigidbody2D body = other.attachedRigidbody;
        if (body == null) return;

        if (!inside.TryGetValue(body, out List<SlowZone> zones))
        {
            zones = new List<SlowZone>();
            inside[body] = zones;
        }
        zones.Add(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.isTrigger) return;
        Rigidbody2D body = other.attachedRigidbody;
        if (body == null) return;

        if (inside.TryGetValue(body, out List<SlowZone> zones))
        {
            zones.Remove(this);
            if (zones.Count == 0) inside.Remove(body);
        }
    }

    // 換地圖時水會被刪掉，順便把自己從清單拿掉，也清掉已經死掉的物體
    private void OnDisable()
    {
        List<Rigidbody2D> empty = null;
        foreach (KeyValuePair<Rigidbody2D, List<SlowZone>> pair in inside)
        {
            pair.Value.RemoveAll(z => z == this || z == null);
            if (pair.Key == null || pair.Value.Count == 0)
            {
                if (empty == null) empty = new List<Rigidbody2D>();
                empty.Add(pair.Key);
            }
        }
        if (empty != null)
        {
            foreach (Rigidbody2D key in empty) inside.Remove(key);
        }
    }

    // 「Enter Play Mode 不重新載入」的設定下也從乾淨的狀態開始
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        inside.Clear();
    }
}
