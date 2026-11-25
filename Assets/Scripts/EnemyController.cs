using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class Enemycontroller : MonoBehaviour
{
    //角色位置抓取
    [SerializeField]private Transform player;

    public float EnemySpeed = 2f;
    //數值參考宣告
    private Rigidbody2D rb;
    private EnemyCountManager gameManager;

    private Vector2 currentDirection;

    //擊退引擎
    private bool isKnockback;
    public float knockback = 5f;
    private float KnockbackDuration = 0.5f;
    private float KnockbackTime = 0f;
    //可調整基本數值
    public float currentHealth = 3;
    public int contactDamage = 1;
    [Tooltip("死掉時掉幾枚金幣（會自動套用掠奪加成）")]
    public int goldDrop = 5;

    private bool isDead; // 避免同一幀被打兩次、死兩次（金幣拿兩次）

    public void TakeDamage_E(float damage)
    {
        if (isDead) return;

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
        gameManager = FindFirstObjectByType<EnemyCountManager>();
        //敵人數量調整(目前只能指定固定數量，請寫程式改成"隨機數量皆可"，也請配合GameManager)

        // 用程式生出來的敵人沒辦法在 Inspector 拖玩家，所以自己用 Tag 找
        if (player == null)
        {
            GameObject target = GameObject.FindWithTag("Player");
            if (target != null) player = target.transform;
        }
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

        if (player == null) return; // 找不到玩家就先不動

        //計算敵人到玩家的方向向量，用Speed的數值調整擊退距離
        Vector2 direction = (Vector2)player.position - rb.position;

        if (direction.magnitude > 1f)
        {
            direction = direction.normalized;
        }

        rb.linearVelocity = direction * EnemySpeed * SlowZone.MultiplierFor(rb); // 在水裡會變慢

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

    private void Die()
    {
        isDead = true;
        if (gameManager != null) gameManager.EnemyDefeated();

        // 掉金幣（組員 E 的 CurrencyManager）
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.AddGold(goldDrop);

        Destroy(gameObject);
    }


}
