using TMPro;
using UnityEngine;

/// <summary>
/// 跳出來的傷害數字：往上飄、慢慢變透明，然後消失（組員 E）。
/// 用 DamagePopup.Spawn(位置, 傷害, 是否爆擊) 產生。
/// </summary>
public class DamagePopup : MonoBehaviour
{
    private const float Lifetime = 0.7f;
    private const float RiseSpeed = 1.6f;

    private static Material outlineMaterial;
    private static bool triedLoadingMaterial;

    private TextMeshPro text;
    private Color baseColor;
    private float age;
    private bool isCrit;

    public static DamagePopup Spawn(Vector3 position, float damage, bool isCrit)
    {
        GameObject go = new GameObject("DamagePopup");
        position.x += Random.Range(-0.2f, 0.2f);
        position.z = 0f;
        go.transform.position = position;

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.text = FormatDamage(damage);
        tmp.fontSize = isCrit ? 6f : 4.5f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.rectTransform.sizeDelta = new Vector2(3f, 1f);
        tmp.color = isCrit ? new Color(1f, 0.85f, 0.2f) : Color.white;
        tmp.sortingOrder = 500; // 蓋在地圖和角色上面

        // 黑色外框比較看得清楚（TextMesh Pro 內建的材質）
        if (!triedLoadingMaterial)
        {
            triedLoadingMaterial = true;
            outlineMaterial = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Outline");
        }
        if (outlineMaterial != null) tmp.fontSharedMaterial = outlineMaterial;

        DamagePopup popup = go.AddComponent<DamagePopup>();
        popup.text = tmp;
        popup.baseColor = tmp.color;
        popup.isCrit = isCrit;
        return popup;
    }

    private static string FormatDamage(float damage)
    {
        // 整數就不顯示小數點；有小數就顯示一位，例如 1.2
        return Mathf.Approximately(damage, Mathf.Round(damage)) ? Mathf.RoundToInt(damage).ToString() : damage.ToString("0.#");
    }

    private void Update()
    {
        age += Time.deltaTime;
        float t = age / Lifetime;

        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // 往上飄，越來越慢
        transform.position += Vector3.up * RiseSpeed * (1f - t) * Time.deltaTime;

        // 爆擊一開始放大再縮回來
        float scale = isCrit ? Mathf.Lerp(1.5f, 1f, Mathf.Clamp01(t * 4f)) : 1f;
        transform.localScale = Vector3.one * scale;

        // 後半段淡出
        Color c = baseColor;
        c.a = t < 0.5f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f);
        text.color = c;
    }
}
