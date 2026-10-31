using UnityEngine;

public class WeaponHitTest : MonoBehaviour
{
    private WeaponController weaponController;
    public int hitDamage = 1;
    void Start()
    {
        weaponController = GetComponentInParent<WeaponController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("武器碰到：" + other.name);

        if (weaponController == null)
        {
            Debug.LogError("找不到 WeaponController！");
            return;
        }

        // 不是揮刀期間，就不造成擊退
        if (!weaponController.IsCurrentlyAttacking)
        {
            return;
        }

        Enemycontroller enemy = other.GetComponent<Enemycontroller>();

        if (enemy != null)
        {
            Debug.Log("取得EnemyController");
            enemy.Knockback();
            enemy.TakeDamage(hitDamage);
            
        }
    }
    

}
