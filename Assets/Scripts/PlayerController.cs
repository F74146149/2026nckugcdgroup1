using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float hp = 3;
    public float Speed = 5f;
    private Rigidbody2D rb;
    private Enemycontroller enemycontroller;
    public int maxHealth = 3;
    public int currentHealth = 3;
    

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Died");
        Time.timeScale = 0f;
    }

    void Start()
    {
        transform.position = Vector3.zero;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0f);

        // 2. 核心修正：如果長度大於 1，就將其歸一化（避免斜角暴衝）
        if (movement.magnitude > 1f)
        {
            movement = movement.normalized;
        }

        // 3. 執行移動
        rb.linearVelocity = movement * Speed;

        
    }

    
}
