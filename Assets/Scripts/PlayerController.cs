using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    //數值參考宣告
    private Rigidbody2D rb;
    private Enemycontroller enemycontroller;
    //可調動基本數值
    public float PlayerSpeed = 5f;
    public int maxHealth = 3;
    public int PlayercurrentHealth = 3;

    // 實際移動速度 = PlayerSpeed × 移動速度被動加成（組員 E 的 PlayerStats）× 地形（水會變慢）
    public float CurrentSpeed
    {
        get
        {
            float speed = PlayerSpeed;
            if (PlayerStats.Instance != null)
            {
                float baseSpeed = PlayerStats.Instance.GetBase(StatType.MoveSpeed);
                if (baseSpeed > 0f) speed *= PlayerStats.Instance.MoveSpeed / baseSpeed;
            }
            return speed * SlowZone.MultiplierFor(rb);
        }
    }

    [Tooltip("被打到之後幾秒內不會再受傷（無敵時間）")]
    public float invincibleTime = 1f;
    private float invincibleUntil;

    /// <summary>現在是不是無敵時間</summary>
    public bool IsInvincible => Time.time < invincibleUntil;

    //角色受傷參考
    public void TakeDamage_P(int damage)
    {
        // 已經死了、或剛被打過還在無敵時間，就不扣血
        // （避免被一群敵人同時碰到，一瞬間扣光）
        if (damage <= 0 || PlayercurrentHealth <= 0 || IsInvincible) return;
        invincibleUntil = Time.time + invincibleTime;

        PlayercurrentHealth -= damage;

        if (PlayercurrentHealth <= 0)
        {
            PlayercurrentHealth = 0;
            Die();
        }
    }
    void Die()
    {
        Time.timeScale = 0f;
    }

    void Start()
    {
        transform.position = Vector3.zero;
        rb = GetComponent<Rigidbody2D>();
    }

    
    void Update()
    {
        //建立一個三維向量儲存上下左右回傳值
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0f);

        //如果長度大於 1，就將其歸一化（避免斜角暴衝）
        if (movement.magnitude > 1f)
        {
            movement = movement.normalized;
        }

        //利用物理引擎線性移動
        rb.linearVelocity = movement * CurrentSpeed;

        
    }

    
}
