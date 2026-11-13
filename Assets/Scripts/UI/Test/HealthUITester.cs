using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 測試用的「假玩家」，只用來測試 HealthUI，正式遊戲不要放進場景。
/// 等組員 A 的玩家血量做好，改成由他的程式呼叫 HealthUI.SetHealth 即可。
///
/// 按鍵（按 Play 之後）：
///   空白鍵 = 扣 1 滴血
///   H      = 補 1 滴血
///   M      = 最大血量 +1（模擬買到加血的被動能力）
///   R      = 回滿血
/// </summary>
public class HealthUITester : MonoBehaviour
{
    [SerializeField] private HealthUI healthUI;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth = 5;

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            currentHealth = Mathf.Max(0, currentHealth - 1);
            Debug.Log("扣血，目前血量：" + currentHealth);
            Refresh();
        }

        if (keyboard.hKey.wasPressedThisFrame)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + 1);
            Refresh();
        }

        if (keyboard.mKey.wasPressedThisFrame)
        {
            maxHealth++;
            Refresh();
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            currentHealth = maxHealth;
            Refresh();
        }
    }

    private void Refresh()
    {
        if (healthUI != null)
        {
            healthUI.SetHealth(currentHealth, maxHealth);
        }
    }
}
