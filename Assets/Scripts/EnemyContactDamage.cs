using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private int damage = 5;
    [SerializeField] private float cooldown = 1f;

    private float nextHitTime;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextHitTime)
            return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
            return;

        nextHitTime = Time.time + cooldown;
        player.TakeDamage_P(damage);
    }
}