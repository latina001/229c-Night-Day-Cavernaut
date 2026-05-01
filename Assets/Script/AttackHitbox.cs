using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public float damage = 10f;

    Collider2D col;

    void Start()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false;
    }

    public void EnableHitbox()
    {
        col.enabled = true;
    }

    public void DisableHitbox()
    {
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null)
        {
            target.TakeDamage(damage, transform.root);
        }
    }
}