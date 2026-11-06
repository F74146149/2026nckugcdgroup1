using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class Enemycontroller : MonoBehaviour
{
    //角色位置抓取
    public Transform player;

    public float Speed = 2f;
    //數值參考宣告
    private Rigidbody2D rb;
    private GameManager gameManager;

    private Vector2 currentDirection;

    //擊退引擎
    private bool isKnockback;
    public float knockback = 5f;
    private float KnockbackDuration = 0.5f;
    private float KnockbackTime = 0f;
    //可調整基本數值
    public float currentHealth = 3;
    public int contactDamage = 1;



    public void TakeDamage_E(int damage)
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
        //敵人數量調整(目前只能指定固定數量，請寫程式改成"隨機數量皆可"，也請配合GameManager)
    }


    void FixedUpdate()
    {
        //判斷擊退，利用計時器計算擊退時間
        if (isKnockback)
        {
            KnockbackTime += Time.deltaTime;
            rb.linearVelocity = -currentDirection * knockback;

            if (KnockbackTime >= KnockbackDuration)
            {
                isKnockback = false;
                KnockbackTime = 0f;
            }

            return;//擊退期間不可貼近玩家，因而打斷下面程式
        }
        //計算敵人到玩家的方向向量，用Speed的數值調整擊退距離
        Vector2 direction = (Vector2)player.position - rb.position;

        if (direction.magnitude > 1f)
        {
            direction = direction.normalized;
        }

        rb.linearVelocity = direction * Speed;

        currentDirection = direction;
    }
    //判斷敵人碰撞玩家與否，用ContactDamage調整碰撞時傷害
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player =
                collision.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                player.TakeDamage_P(contactDamage);
                Knockback();
            }
        }
    }
    //擊退函式
    public void Knockback()
    {
        isKnockback = true;
        KnockbackTime = 0f;

    }

    public void Die()
    {
        gameManager.EnemyDefeated();
        Destroy(gameObject);
    }


}
