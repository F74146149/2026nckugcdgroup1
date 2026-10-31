using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class Enemycontroller : MonoBehaviour
{

    public Transform player;

    public float Speed = 2f;
    private Rigidbody2D rb;
    private GameManager gameManager;

    private Vector2 currentDirection;

    private bool isKnockback;
    public float knockback = 5f;
    private float KnockbackDuration = 0.5f;
    private float KnockbackTime = 0f;

    public float currentHealth = 3;
    public int contactDamage = 1;



    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindFirstObjectByType<GameManager>();
    }


    void FixedUpdate()
    {
        if (isKnockback)
        {
            KnockbackTime += Time.deltaTime;
            rb.linearVelocity = -currentDirection * knockback;

            if (KnockbackTime >= KnockbackDuration)
            {
                isKnockback = false;
                Debug.Log("Knockback finished!");
                KnockbackTime = 0f;
            }

            return;
        }

        Vector2 direction = (Vector2)player.position - rb.position;

        if (direction.magnitude > 1f)
        {
            direction = direction.normalized;
        }

        rb.linearVelocity = direction * Speed;

        currentDirection = direction;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player =
                collision.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                player.TakeDamage(contactDamage);
                Knockback();
            }
        }
    }

    public void Knockback()
    {
        isKnockback = true;
        Debug.Log("Hit!");
        KnockbackTime = 0f;

    }

    public void Die()
    {
        gameManager.EnemyDefeated();
        Debug.Log("Enemy Died");
        Destroy(gameObject);
    }


}
