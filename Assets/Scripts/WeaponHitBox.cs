using UnityEngine;
using System.Collections.Generic;


public class WeaponHitBox : MonoBehaviour
{
    private SwordController weaponController;
    private HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();

    // ===== 讓武器圖片跟攻擊範圍一致（組員 E）=====
    // 換武器時：圖片縮放成 WeaponData 的 Weapon Length，刀柄放在玩家旁邊，
    // 攻擊範圍（Collider）改成圖片的形狀，所以「看到砍到哪裡」就是「打得到哪裡」。
    [Header("Fit hitbox to sprite")]
    [Tooltip("換武器時自動調整圖片大小和攻擊範圍")]
    [SerializeField] private bool fitToSprite = true;
    [Tooltip("刀柄離玩家中心多遠（Unity 單位）")]
    [SerializeField] private float gripDistance = 0.35f;
    [Tooltip("WeaponData 沒設定長度時用這個長度")]
    [SerializeField] private float defaultLength = 2f;

    private SpriteRenderer spriteRenderer;
    private WeaponManager weaponManager;
    private PolygonCollider2D shapeCollider;
    private Sprite fittedSprite;
    private float fittedLength = -1f;

    void Start()
    {
        weaponController = GetComponentInParent<SwordController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        weaponManager = GetComponentInParent<WeaponManager>();
        FitIfChanged();
    }

    void Update()
    {
        // 撿到新武器時圖片會換掉，這裡發現了就重新調整
        FitIfChanged();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("碰撞到Enemy");
        EnemyController enemy = other.GetComponent<EnemyController>();

        // 不是揮刀期間，就不造成擊退
        if (!weaponController.IsCurrentlyAttacking)
        {
            return;
        }

        if (enemy != null && !hitEnemies.Contains(enemy))
        {
            hitEnemies.Add(enemy);

            Debug.Log("取得EnemyController");
            enemy.Knockback();

            // 套用被動能力（攻擊力、爆擊）後的傷害（組員 E 的 DamageCalculator）
            float damage = DamageCalculator.Calculate(weaponController.CurrentDamage, out bool isCrit);
            enemy.TakeDamage_E(damage);
            HitEffects.EnemyHit(enemy.gameObject, damage, isCrit);

        }
    }

    public void ResetHitEnemies()
    {
        hitEnemies.Clear();
    }

    // ===== 讓武器圖片跟攻擊範圍一致 =====

    private void FitIfChanged()
    {
        if (!fitToSprite || spriteRenderer == null || spriteRenderer.sprite == null) return;

        float length = defaultLength;
        if (weaponManager != null && weaponManager.CurrentWeaponData != null)
        {
            length = weaponManager.CurrentWeaponData.WeaponLength;
        }

        if (spriteRenderer.sprite == fittedSprite && Mathf.Approximately(length, fittedLength)) return;
        FitToSprite(spriteRenderer.sprite, length);
    }

    /// <summary>把圖片縮放成 length 長，刀柄放在玩家旁邊，攻擊範圍改成圖片的形狀</summary>
    public void FitToSprite(Sprite sprite, float length)
    {
        fittedSprite = sprite;
        fittedLength = length;

        // 1. 圖片的實際形狀（匯入圖片時 Unity 依透明度描出來的外框）
        List<Vector2[]> paths = new List<Vector2[]>();
        List<Vector2> buffer = new List<Vector2>();
        int shapeCount = sprite.GetPhysicsShapeCount();
        for (int i = 0; i < shapeCount; i++)
        {
            buffer.Clear();
            sprite.GetPhysicsShape(i, buffer);
            if (buffer.Count >= 3) paths.Add(buffer.ToArray());
        }
        if (paths.Count == 0)
        {
            // 沒有外框資料就用整張圖的範圍
            Bounds b = sprite.bounds;
            paths.Add(new[]
            {
                new Vector2(b.min.x, b.min.y), new Vector2(b.max.x, b.min.y),
                new Vector2(b.max.x, b.max.y), new Vector2(b.min.x, b.max.y),
            });
        }

        // 2. 形狀的範圍（圖片是橫的，刀柄在左邊）
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
        foreach (Vector2[] path in paths)
        {
            foreach (Vector2 p in path)
            {
                minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
            }
        }
        float spriteLength = maxX - minX;
        if (spriteLength <= 0.0001f) return;

        // 3. 縮放成指定長度，刀柄（最左邊）放在離玩家 gripDistance 的地方，上下置中
        float scale = length / spriteLength;
        transform.localScale = new Vector3(scale, scale, 1f);
        transform.localPosition = new Vector3(
            gripDistance - minX * scale,
            -(minY + maxY) * 0.5f * scale,
            transform.localPosition.z);

        // 4. 攻擊範圍 = 圖片形狀（原本的方形 Collider 關掉）
        if (shapeCollider == null) shapeCollider = GetComponent<PolygonCollider2D>();
        if (shapeCollider == null) shapeCollider = gameObject.AddComponent<PolygonCollider2D>();
        shapeCollider.isTrigger = true;
        shapeCollider.pathCount = paths.Count;
        for (int i = 0; i < paths.Count; i++) shapeCollider.SetPath(i, paths[i]);
        shapeCollider.enabled = true;

        foreach (Collider2D other in GetComponents<Collider2D>())
        {
            if (other != shapeCollider) other.enabled = false;
        }
    }
}
