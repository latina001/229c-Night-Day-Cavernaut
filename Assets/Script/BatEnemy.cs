using UnityEngine;
using System.Collections;

public class BatEnemy : MonoBehaviour, IDamageable
{
    [Header("Target")]
    public Transform player;

    [Header("Area")]
    public Transform centerPoint;     // จุดกลางโซนบิน
    public float roamRadius = 4f;     // รัศมีบินวน

    [Header("Stats")]
    public float health = 30f;

    [Header("Move")]
    public float speed = 3f;
    public float chaseRange = 8f;
    public float stopDistance = 1.5f;
    public float roamSpeed = 2f;

    [Header("Attack")]
    public float damage = 10f;
    public float cooldown = 1f;
    float lastAttack;

    [Header("Knockback")]
    public float knockback = 6f;
    public float hitStun = 0.3f;

    [Header("Death")]
    public float deathTime = 1.5f;
    public float fallGravity = 3f;
    public float deathPushForce = 5f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Collider2D col;

    bool isDead;
    bool isHit;

    Vector2 roamTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        rb.gravityScale = 0;

        PickNewRoamPoint();
    }

    void Update()
    {
        if (isDead || isHit) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= chaseRange)
        {
            ChasePlayer(distToPlayer);
        }
        else
        {
            Roam();
        }
    }

    // ================= CHASE =================
    void ChasePlayer(float dist)
    {
        Vector2 dir = (player.position - transform.position).normalized;

        rb.linearVelocity = dir * speed;

        sr.flipX = dir.x < 0;

        if (dist <= stopDistance)
            Attack();
    }

    // ================= ROAM (บินในพื้นที่) =================
    void Roam()
    {
        if (Vector2.Distance(transform.position, roamTarget) < 0.3f)
        {
            PickNewRoamPoint();
        }

        Vector2 dir = (roamTarget - (Vector2)transform.position).normalized;

        rb.linearVelocity = dir * roamSpeed;

        sr.flipX = dir.x < 0;
    }

    void PickNewRoamPoint()
    {
        Vector2 random = Random.insideUnitCircle * roamRadius;
        roamTarget = (Vector2)centerPoint.position + random;
    }

    // ================= ATTACK =================
    void Attack()
    {
        if (Time.time < lastAttack + cooldown) return;

        lastAttack = Time.time;

        anim.SetTrigger("Attack");

        IDamageable p = player.GetComponent<IDamageable>();
        if (p != null)
        {
            p.TakeDamage(damage, transform);
        }
    }

    // ================= DAMAGE =================
    public void TakeDamage(float damage, Transform attacker)
    {
        if (isDead) return;

        health -= damage;

        Vector2 dir = (transform.position - attacker.position).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * knockback, ForceMode2D.Impulse);

        StartCoroutine(Hit());

        if (health <= 0)
        {
            StartCoroutine(DeathRoutine(attacker));
        }
    }

    IEnumerator Hit()
    {
        isHit = true;

        anim.SetTrigger("Hit");

        yield return new WaitForSeconds(hitStun);

        isHit = false;
    }

    // ================= DEATH =================
    IEnumerator DeathRoutine(Transform attacker)
    {
        isDead = true;

        StopAllCoroutines();

        rb.linearVelocity = Vector2.zero;

        Vector2 dir = (transform.position - attacker.position).normalized;
        rb.AddForce(dir * deathPushForce, ForceMode2D.Impulse);

        rb.gravityScale = fallGravity;

        if (col) col.enabled = false;

        anim.ResetTrigger("Hit");
        anim.ResetTrigger("Attack");

        anim.SetTrigger("Death");

        yield return new WaitForSeconds(deathTime);

        Destroy(gameObject);
    }

    // ================= DEBUG AREA =================
    void OnDrawGizmosSelected()
    {
        if (centerPoint == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(centerPoint.position, roamRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}