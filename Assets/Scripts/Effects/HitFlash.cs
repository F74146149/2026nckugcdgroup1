using UnityEngine;

/// <summary>
/// 讓物體（含子物件）的圖片閃一下，閃完恢復原狀（組員 E）。
/// 不用手動加：呼叫下面兩個方法會自動加上這個元件。
///
///   HitFlash.FlashWhite(物體, 多久)
///     → 整張圖一瞬間變成純白（用 Resources/HitFlashWhite 材質）
///   HitFlash.Flash(物體, 顏色, 每次多久, 閃幾下)
///     → 把圖片染成某個顏色，一亮一暗閃幾下（玩家受傷閃紅用這個）
/// </summary>
public class HitFlash : MonoBehaviour
{
    private const string WhiteMaterialPath = "HitFlashWhite";

    private static Material whiteMaterial;
    private static bool triedLoadingWhite;

    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    private Material[] originalMaterials;

    private Color flashColor;
    private Material flashMaterial; // 有的話換材質（變白），沒有的話染色
    private float interval;
    private int phasesLeft;         // 亮、暗、亮、暗…還剩幾段
    private float timer;

    public bool IsFlashing => phasesLeft > 0;

    /// <summary>一瞬間變成純白</summary>
    public static void FlashWhite(GameObject target, float duration)
    {
        if (target == null) return;

        if (!triedLoadingWhite)
        {
            triedLoadingWhite = true;
            whiteMaterial = Resources.Load<Material>(WhiteMaterialPath);
            if (whiteMaterial == null) Debug.LogWarning("HitFlash：找不到 Resources/" + WhiteMaterialPath + "，改用染色");
        }

        // 找不到白色材質時，退回染成淡色
        GetOrAdd(target).Play(new Color(1f, 0.8f, 0.8f), whiteMaterial, duration, 1);
    }

    /// <summary>染成某個顏色，一亮一暗閃 blinks 下</summary>
    public static void Flash(GameObject target, Color color, float interval, int blinks)
    {
        if (target == null) return;
        GetOrAdd(target).Play(color, null, interval, blinks);
    }

    private static HitFlash GetOrAdd(GameObject target)
    {
        HitFlash flash = target.GetComponent<HitFlash>();
        if (flash == null) flash = target.AddComponent<HitFlash>();
        return flash;
    }

    private void Play(Color color, Material material, float newInterval, int blinks)
    {
        // 正在閃就先恢復，再記住原本的樣子（不然會把閃的顏色當成原本的）
        if (IsFlashing) Restore();
        CaptureOriginals();

        flashColor = color;
        flashMaterial = material;
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

    private void CaptureOriginals()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[renderers.Length];
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
            originalMaterials[i] = renderers[i].sharedMaterial;
        }
    }

    private void Apply(bool flashOn)
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer sr = renderers[i];
            if (sr == null) continue;

            if (flashMaterial != null)
            {
                sr.sharedMaterial = flashOn ? flashMaterial : originalMaterials[i];
            }
            else
            {
                Color c = flashOn ? flashColor : originalColors[i];
                c.a = originalColors[i].a;
                sr.color = c;
            }
        }
    }

    private void Restore()
    {
        phasesLeft = 0;
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].color = originalColors[i];
            renderers[i].sharedMaterial = originalMaterials[i];
        }
    }
}
