using UnityEngine;

/// <summary>
/// 讓物體（含子物件）的圖片閃一下顏色，閃完恢復原本的顏色（組員 E）。
/// 不用手動加：HitFlash.Flash(物體, 顏色, 每次多久, 閃幾下) 會自動加上這個元件。
/// </summary>
public class HitFlash : MonoBehaviour
{
    private SpriteRenderer[] renderers;
    private Color[] originalColors;

    private Color flashColor;
    private float interval;
    private int phasesLeft;   // 亮、暗、亮、暗…還剩幾段
    private float timer;

    public bool IsFlashing => phasesLeft > 0;

    public static void Flash(GameObject target, Color color, float interval, int blinks)
    {
        if (target == null) return;

        HitFlash flash = target.GetComponent<HitFlash>();
        if (flash == null) flash = target.AddComponent<HitFlash>();
        flash.Play(color, interval, blinks);
    }

    private void Play(Color color, float newInterval, int blinks)
    {
        // 沒在閃的時候才記住原本的顏色（正在閃時記到的會是閃的顏色）
        if (!IsFlashing) CaptureOriginalColors();

        flashColor = color;
        interval = Mathf.Max(0.01f, newInterval);
        phasesLeft = Mathf.Max(1, blinks) * 2 - 1; // 最後一段是「亮」，結束後恢復
        timer = interval;
        Apply(true);
    }

    private void Update()
    {
        if (!IsFlashing) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        phasesLeft--;
        timer = interval;

        if (phasesLeft <= 0) Restore();
        else Apply(phasesLeft % 2 == 1);
    }

    private void OnDisable()
    {
        if (IsFlashing) Restore();
    }

    private void CaptureOriginalColors()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
        }
    }

    private void Apply(bool flashOn)
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Color c = flashOn ? flashColor : originalColors[i];
            c.a = originalColors[i].a;
            renderers[i].color = c;
        }
    }

    private void Restore()
    {
        phasesLeft = 0;
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null) renderers[i].color = originalColors[i];
        }
    }
}
