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
        Enemycontroller enemy = collision.GetComponentInParent<Enemycontroller>();

        if (enemy != null)
        {
            enemy.TakeDamage_E(damage);

            Destroy(gameObject);
        } else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
