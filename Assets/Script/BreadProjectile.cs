using UnityEngine;

/// <summary>
/// ติดกับ Prefab ขนมปัง — บอสจะ Instantiate แล้วยิงไปหาผู้เล่น
/// </summary>
public class BreadProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float damage = 1f;          // ลด 1 หัวใจ
    public float lifetime = 4f;

    Vector2 dir;

    public void Init(Vector2 direction)
    {
        dir = direction.normalized;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IDamageable dmg = other.GetComponent<IDamageable>();
            if (dmg != null)
                dmg.TakeDamage(damage, transform);

            Destroy(gameObject);
        }

        // ชนพื้น/กำแพงหายด้วย
        if (other.CompareTag("Ground"))
            Destroy(gameObject);
    }
}
