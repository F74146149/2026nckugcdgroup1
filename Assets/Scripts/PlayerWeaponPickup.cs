using UnityEngine;

public class PlayerWeaponPickup : MonoBehaviour
{
    private WeaponManager weaponManager;
    private GroundWeapon nearbyWeapon;
    

    [Header("Drop Settings")]
    [SerializeField] private GameObject groundWeaponPrefab;
    [SerializeField] private Transform dropPoint;

    private void Awake()
    {
        weaponManager = GetComponent<WeaponManager>();
    }

    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F) && nearbyWeapon != null)
        {
            PickupWeapon();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GroundWeapon groundWeapon = other.GetComponent<GroundWeapon>();

        if (groundWeapon != null)
        {
            nearbyWeapon = groundWeapon;
            Debug.Log("附近有武器：" + groundWeapon.WeaponData.WeaponName);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GroundWeapon groundWeapon = other.GetComponent<GroundWeapon>();

        if (groundWeapon == nearbyWeapon)
        {
            nearbyWeapon = null;
            Debug.Log("離開武器拾取範圍");
        }
    }

    private void PickupWeapon()
    {
        if (nearbyWeapon == null)return;

        WeaponData newWeapon = nearbyWeapon.WeaponData;
        WeaponData oldWeapon = weaponManager.CurrentWeaponData;

        if (newWeapon == null)
        {
            Debug.LogError("地面武器沒有設定 WeaponData！");
            return;
        }

        if (newWeapon == oldWeapon)
        {
            Debug.Log("已經裝備這把武器！");
            return;
        }

        // 先生成原本的武器
        DropWeapon(oldWeapon);

        // 裝備新武器
        weaponManager.EquipWeapon(newWeapon);

        // 移除原本被拾取的地面物件
        Destroy(nearbyWeapon.gameObject);

        nearbyWeapon = null;

        Debug.Log("武器交換成功！");
    }

    private void DropWeapon(WeaponData weaponData)
    {
        GameObject newObject = Instantiate(
        groundWeaponPrefab,
        dropPoint.position,
        Quaternion.identity);

        GroundWeapon groundWeapon = newObject.GetComponent<GroundWeapon>();

        if (groundWeapon == null)
        {
            Debug.LogError("Prefab 找不到 GroundWeapon！");
            Destroy(newObject);
            return;
        }

        groundWeapon.Initialize(weaponData);
    }
}
