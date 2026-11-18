using UnityEngine;

public class GroundWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private WeaponData weaponData;
    private SpriteRenderer spriteRenderer;
    public WeaponData WeaponData => weaponData;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

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

        spriteRenderer.sprite = weaponData.WeaponSprite;
        
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
            spriteRenderer.sprite = weaponData.WeaponSprite;
        }
    }
}
