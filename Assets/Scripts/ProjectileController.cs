using UnityEngine;

public class ProjectilContoller : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField]private int damage = 3;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    
    void Update()
    {
        
    }

    public void Launch(Vector2 direction,float speed)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyController enemy = collision.GetComponentInParent<EnemyController>();

        if (enemy != null)
        {
            // 套用被動能力（攻擊力、爆擊）後的傷害（組員 E 的 DamageCalculator）
            float dealt = DamageCalculator.Calculate(damage, out bool isCrit);
            enemy.TakeDamage_E(dealt);
            HitEffects.EnemyHit(enemy.gameObject, dealt, isCrit);

            Destroy(gameObject);
        } else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
