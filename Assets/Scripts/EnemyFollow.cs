using System.Collections;
using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("追蹤")]
    public float speed = 2f;
    public float detectionRange = 5f;

    [Header("遊走")]
    public float wanderRadius = 2f;
    public float wanderSpeed = 1f;
    public float waitTime = 2f;

    public float attackRange = 1f;
    public float windupTime = 0.4f;
    public float attackCooldown = 1f;

    private bool isAttacking;
    private float nextAttackTime;

    [Header("提示")]
    public GameObject alert;
    public float alertDuration = 0.5f;

    private Transform player;

    private Rigidbody2D rb;
    private bool wasDetected;

    private Vector2 spawnPosition;
    private Vector2 wanderTarget;
    private float waitTimer;

    [SerializeField] private LayerMask wallLayers;

    void Start()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");

        if (target != null)
            player = target.transform;

        if (alert != null)
            alert.SetActive(false);

        spawnPosition = transform.position;
        ChooseWanderTarget();
    }

    void FixedUpdate()
    {
        bool detected = false;

        if (player != null)
        {
            float distance = Vector2.Distance(
                transform.position,
                player.position
            );

            detected = distance <= detectionRange;

            if (detected && !wasDetected && alert != null)
                StartCoroutine(ShowAlert());

            if (detected && !isAttacking)
            {
                if (distance <= attackRange)
                {
                    if (Time.time >= nextAttackTime)
                        StartCoroutine(MeleeAttack());
                }
                else
                {
                    float step = Mathf.Min(
                        speed * Time.fixedDeltaTime,
                        distance - attackRange
                    );

                    rb.MovePosition(Vector2.MoveTowards(
                        rb.position, player.position, step
                    ));
                }
            }
        }

        if(!detected && !isAttacking)
            Wander();

        wasDetected = detected;
    }

    void Wander()
    {
        if (waitTimer > 0)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        rb.MovePosition(Vector2.MoveTowards(
        rb.position, wanderTarget, wanderSpeed * Time.fixedDeltaTime
        ));

        if (Vector2.Distance(transform.position, wanderTarget) < 0.01f)
        {
            waitTimer = waitTime;
            ChooseWanderTarget();
        }
    }

    void ChooseWanderTarget()
    {
        wanderTarget = spawnPosition + Random.insideUnitCircle * wanderRadius;
    }

    IEnumerator ShowAlert()
    {
        alert.SetActive(true);
        yield return new WaitForSeconds(alertDuration);
        alert.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    IEnumerator MeleeAttack()
    {
        isAttacking = true;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        sprite.color = Color.red;

        yield return new WaitForSeconds(windupTime);

        if (player != null &&
        Vector2.Distance(rb.position, player.position) <= attackRange &&
        Physics2D.Linecast(rb.position, player.position, wallLayers).collider == null)
        {
            Debug.Log("命中玩家");
        }
        else
        {
            Debug.Log("揮空");
        }

        sprite.color = originalColor;
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }
}