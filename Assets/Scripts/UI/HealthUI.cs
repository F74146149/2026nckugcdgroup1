using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用愛心顯示玩家血量。
/// 其他腳本只要呼叫 SetHealth(目前血量, 最大血量) 就會更新畫面。
///
/// 使用方式：
/// 1. 在 Canvas 底下建一個空物件（例如 HeartContainer），加上 Horizontal Layout Group。
/// 2. 在 HeartContainer 底下放一個 Image 當作「一顆愛心」的範本。
/// 3. 把這個腳本掛在任何物件上，把那個 Image 拖到 Heart Template 欄位。
/// 4. 有愛心圖片的話拖進 Full Heart / Empty Heart；沒有的話會用顏色代替。
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("愛心範本（Canvas 底下的一個 Image）")]
    [SerializeField] private Image heartTemplate;

    [Header("愛心圖片（可以不填）")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("沒有圖片時用的顏色")]
    [SerializeField] private Color fullColor = new Color(0.9f, 0.15f, 0.2f, 1f);
    [SerializeField] private Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);

    // 已經產生出來的愛心
    private readonly List<Image> hearts = new List<Image>();

    private void Awake()
    {
        // 範本本身不顯示，只用來複製
        if (heartTemplate != null)
        {
            heartTemplate.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 更新血量顯示。
    /// </summary>
    /// <param name="current">目前血量</param>
    /// <param name="max">最大血量（例如買了加血的被動能力，這個數字會變大）</param>
    public void SetHealth(int current, int max)
    {
        if (heartTemplate == null)
        {
            Debug.LogWarning("HealthUI：還沒有設定 Heart Template，請在 Inspector 拖一個 Image 進去。", this);
            return;
        }

        max = Mathf.Max(0, max);
        current = Mathf.Clamp(current, 0, max);

        // 愛心不夠就再複製
        while (hearts.Count < max)
        {
            Image heart = Instantiate(heartTemplate, heartTemplate.transform.parent);
            heart.name = "Heart " + (hearts.Count + 1);
            hearts.Add(heart);
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            Image heart = hearts[i];

            // 超過最大血量的愛心先藏起來（最大血量變少時會用到）
            bool visible = i < max;
            heart.gameObject.SetActive(visible);
            if (!visible) continue;

            bool filled = i < current;
            ApplyLook(heart, filled);
        }
    }

    // 決定一顆愛心是「滿的」還是「空的」樣子
    private void ApplyLook(Image heart, bool filled)
    {
        if (filled)
        {
            if (fullHeart != null)
            {
                heart.sprite = fullHeart;
                heart.color = Color.white;
            }
            else
            {
                heart.color = fullColor;
            }
        }
        else
        {
            if (emptyHeart != null)
            {
                heart.sprite = emptyHeart;
                heart.color = Color.white;
            }
            else
            {
                // 沒有空愛心圖片：用原本的圖，改成灰色
                if (fullHeart != null) heart.sprite = fullHeart;
                heart.color = emptyColor;
            }
        }
    }
}
