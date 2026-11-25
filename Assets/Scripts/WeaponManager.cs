using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private WeaponData startingWeaponData;

    [Header("References")]
    [SerializeField] private SwordController swordController;
    [SerializeField] private SpriteRenderer weaponRenderer;

    private WeaponData currentWeaponData;

    public WeaponData CurrentWeaponData => currentWeaponData;

    private void Start()
    {
        EquipWeapon(startingWeaponData);
    }

    public void EquipWeapon(WeaponData newWeaponData)
    {
        if (newWeaponData == null)
        {
            Debug.LogError("無法裝備武器：WeaponData 為空！");
            return;
        }

        if (swordController == null)
        {
            Debug.LogError("WeaponManager 找不到 SwordController！");
            return;
        }

        swordController.SetWeaponData(newWeaponData);
        weaponRenderer.sprite = newWeaponData.WeaponSprite;
        currentWeaponData = newWeaponData;

        Debug.Log("目前裝備武器：" + currentWeaponData.WeaponName);
    }
}