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


}
