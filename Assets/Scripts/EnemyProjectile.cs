using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    private int damage;

    public void Launch(Vector2 direction, float speed, int shotDamage)
    {
        damage = shotDamage;
        GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * speed;
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<EnemyController>() != null)
            return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage_P(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Wall") ||
                 other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}