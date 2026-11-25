using UnityEngine;

public class GroundWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private WeaponData weaponData;
    [Tooltip("地上的武器圖片縮放成 WeaponData 的 Weapon Length，跟拿在手上一樣大（撿取範圍不變）")]
    [SerializeField] private bool matchHeldSize = true;

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D pickupRange;
    private float pickupRadius = -1f;   // Prefab 上原本設定的撿取半徑

    public WeaponData WeaponData => weaponData;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pickupRange = GetComponent<CircleCollider2D>();
        if (pickupRange != null) pickupRadius = pickupRange.radius;

        if (weaponData == null)
        {
            Debug.LogError("GroundWeapon 尚未設定 WeaponData！");
            return;
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("GroundWeapon 找不到 SpriteRenderer！");
            return;
        }

        ApplyWeapon();
    }

    public void Initialize(WeaponData newWeaponData)
    {
        weaponData = newWeaponData;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (weaponData != null && spriteRenderer != null)
        {
            ApplyWeapon();
        }
    }

    private void ApplyWeapon()
    {
        spriteRenderer.sprite = weaponData.WeaponSprite;
        if (!matchHeldSize || spriteRenderer.sprite == null) return;

        // 圖片縮放成武器長度
        float width = spriteRenderer.sprite.bounds.size.x;
        if (width <= 0.0001f) return;
        float scale = weaponData.WeaponLength / width;
        transform.localScale = new Vector3(scale, scale, 1f);

        // 物體縮小時 Collider 也會跟著縮小，把撿取半徑放大回原本的大小
        if (pickupRange != null && pickupRadius > 0f) pickupRange.radius = pickupRadius / scale;
    }
}
