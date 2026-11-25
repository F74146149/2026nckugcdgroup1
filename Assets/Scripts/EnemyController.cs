using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("追蹤")]
    [SerializeField] private Transform player;
    public float EnemySpeed = 2f;
    public float detectionRange = 5f;

    [Header("遊走")]
    public float wanderRadius = 2f;
    public float wanderSpeed = 1f;
    public float waitTime = 2f;

    [Header("近戰")]
    public int contactDamage = 1;
    public float attackRange = 1f;
    public float windupTime = 0.4f;
    public float attackCooldown = 1f;
    [SerializeField] private LayerMask wallLayers;

    [Header("提示")]
    public GameObject alert;
    public float alertDuration = 0.5f;

    [Header("生命與擊退")]
    public float currentHealth = 3f;
    public float knockback = 5f;
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private bool useContactAttack;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private EnemyCountManager gameManager;
    private Vector2 spawnPosition;
    private Vector2 wanderTarget;
    private Vector2 lastMoveDirection;
    private Vector2 knockbackDirection;
    private float waitTimer;
    private float knockbackUntil;
    private float nextAttackTime;
    private float alertUntil;
    private bool wasDetected;
    private bool isDead;
    private Coroutine attackRoutine;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            originalColor = sprite.color;

        if (alert != null)
            alert.SetActive(false);
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject target = GameObject.FindGameObjectWithTag("Player");
            if (target != null)
                player = target.transform;
        }

        gameManager = FindFirstObjectByType<EnemyCountManager>();
        spawnPosition = rb.position;
        ChooseWanderTarget();
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        if (alert != null && alert.activeSelf && Time.time >= alertUntil)
            alert.SetActive(false);

        if (Time.time < knockbackUntil)
        {
            rb.linearVelocity = knockbackDirection * knockback;
            return;
        }

        rb.linearVelocity = Vector2.zero;

        bool hasPlayer = player != null && player.gameObject.activeInHierarchy;
        float distance = hasPlayer
            ? Vector2.Distance(rb.position, player.position)
            : float.PositiveInfinity;
        bool detected = distance <= detectionRange;

        if (detected && !wasDetected && alert != null)
        {
            alertUntil = Time.time + alertDuration;
            alert.SetActive(true);
        }
        wasDetected = detected;

        if (attackRoutine != null)
            return;

        if (!detected)
        {
            Wander();
            return;
        }

        if (useContactAttack)
        {
            MoveTowards(player.position, EnemySpeed * Time.fixedDeltaTime);
            return;
        }

        if (distance <= attackRange + 0.01f)
        {
            if (Time.time >= nextAttackTime)
                attackRoutine = StartCoroutine(MeleeAttack());
            return;
        }

        float step = Mathf.Min(
            EnemySpeed * Time.fixedDeltaTime, distance - attackRange);
        MoveTowards(player.position, step);
    }

    private void Wander()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        if (Vector2.Distance(rb.position, wanderTarget) <= 0.01f)
        {
            waitTimer = waitTime;
            ChooseWanderTarget();
            return;
        }

        MoveTowards(wanderTarget, wanderSpeed * Time.fixedDeltaTime);
    }

    private void ChooseWanderTarget()
    {
        wanderTarget = spawnPosition + Random.insideUnitCircle * wanderRadius;
    }

    private void MoveTowards(Vector2 target, float step)
    {
        Vector2 nextPosition = Vector2.MoveTowards(rb.position, target, step);
        Vector2 movement = nextPosition - rb.position;
        if (movement.sqrMagnitude > 0f)
            lastMoveDirection = movement.normalized;

        rb.MovePosition(nextPosition);
    }

    private IEnumerator MeleeAttack()
    {
        if (sprite != null)
            sprite.color = Color.red;

        yield return new WaitForSeconds(windupTime);

        if (player != null && player.gameObject.activeInHierarchy &&
            Vector2.Distance(rb.position, player.position) <= attackRange + 0.01f &&
            Physics2D.Linecast(rb.position, player.position, wallLayers).collider == null &&
            player.TryGetComponent<PlayerController>(out var controller))
        {
            if (projectilePrefab != null)
            {
                Vector2 direction = (Vector2)player.position - rb.position;
                EnemyProjectile shot = Instantiate(
                    projectilePrefab, transform.position, Quaternion.identity
                );
                shot.Launch(direction, projectileSpeed, contactDamage);
            }
            else
            {
                controller.TakeDamage_P(contactDamage);
            }
        }

        if (sprite != null)
            sprite.color = originalColor;
        nextAttackTime = Time.time + attackCooldown;
        attackRoutine = null;
    }

    public void Knockback()
    {
        if (isDead)
            return;

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
            nextAttackTime = Time.time + attackCooldown;
            if (sprite != null)
                sprite.color = originalColor;
        }

        knockbackDirection = player != null
            ? (rb.position - (Vector2)player.position).normalized
            : -lastMoveDirection;
        knockbackUntil = Time.time + knockbackDuration;
    }

    public void TakeDamage_E(float damage)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        if (currentHealth > 0f)
            return;

        isDead = true;
        if (gameManager != null)
            gameManager.EnemyDefeated();

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}