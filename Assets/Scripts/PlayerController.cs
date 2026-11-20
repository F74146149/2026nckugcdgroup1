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
    
    //角色受傷參考
    public void TakeDamage_P(int damage)
    {
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
        rb.linearVelocity = movement * PlayerSpeed;

        
    }

    
}
