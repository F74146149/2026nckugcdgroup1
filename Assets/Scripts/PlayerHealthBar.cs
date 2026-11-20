using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PlayerHealthBar : MonoBehaviour
{
    // 在 Inspector 拖入場景中的 Player
    [SerializeField] private PlayerController player;

    private Slider healthSlider;

    private void Awake()
    {
        // 腳本和 Slider 都掛在 PlayerHealthBar 物件上
        healthSlider = GetComponent<Slider>();
        healthSlider.minValue = 0;
    }

    private void LateUpdate()
    {
        // 如果玩家死亡後被銷毀，血條顯示為空
        if (player == null)
        {
            healthSlider.value = 0;
            return;
        }

        // 如果玩家取得增加最大 HP 的被動，血條上限也會更新
        healthSlider.maxValue = player.maxHealth;
        healthSlider.value = player.PlayercurrentHealth;
    }
}