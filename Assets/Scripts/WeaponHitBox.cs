using UnityEngine;
using System.Collections.Generic;


public class WeaponHitBox : MonoBehaviour
{
    private SwordController weaponController;
    private HashSet<Enemycontroller> hitEnemies = new HashSet<Enemycontroller>();

    void Start()
    {
        weaponController = GetComponentInParent<SwordController>();
    }

    void Update()
    {

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("碰撞到Enemy");
        Enemycontroller enemy = other.GetComponent<Enemycontroller>();

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
            enemy.TakeDamage_E(weaponController.CurrentDamage);
            
        }
    }

    public void ResetHitEnemies()
    {
        hitEnemies.Clear();
    }


}
