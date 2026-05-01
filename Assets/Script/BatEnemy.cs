using UnityEngine;
using System.Collections;

public class BatEnemy : MonoBehaviour, IDamageable
{
    [Header("Target")]
    public Transform player;

    [Header("Stats")]
    public float health = 30f;

    [Header("Move")]
    public float speed = 3f;
    public float chaseRange = 8f;
    public float stopDistance = 1.5f;

    [Header("Attack")]
    public float damage = 10f;
    public float cooldown = 1f;
    float lastAttack;

    [Header("Knockback")]
    public float knockback = 6f;
    public float hitStun = 0.3f;

    [Header("Death")]
    public float deathTime = 1.5f;     // ⏱ เวลารอก่อนหาย
    public float fallGravity = 3f;     // 🌍 แรงตกตอนตาย
    public float deathPushForce = 5f;  // 💥 แรงกระเด็นตอนตาย

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;
    Collider2D col;

    bool isDead;
    bool isHit;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // 🦇 ปกติบิน = ไม่มีแรงโน้มถ่วง
        rb.gravityScale = 0;
    }

    void Update()
    {
        if (isDead || isHit) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= chaseRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;

            rb.linearVelocity = dir * speed;

            sr.flipX = dir.x < 0;

            if (dist <= stopDistance)
                Attack();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ---------------- ATTACK ----------------
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

    // ---------------- TAKE DAMAGE ----------------
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

    // ---------------- HIT ----------------
    IEnumerator Hit()
    {
        isHit = true;

        anim.SetTrigger("Hit");

        yield return new WaitForSeconds(hitStun);

        isHit = false;
    }

    // ---------------- DIE (ตกพื้นจริง) ----------------
    IEnumerator DeathRoutine(Transform attacker)
    {
        isDead = true;

        StopAllCoroutines();

        // 🔥 หยุดก่อน
        rb.linearVelocity = Vector2.zero;

        // 💥 กระเด็นออกจาก attacker
        Vector2 dir = (transform.position - attacker.position).normalized;
        rb.AddForce(dir * deathPushForce, ForceMode2D.Impulse);

        // 🌍 เปิดแรงโน้มถ่วงให้ตก
        rb.gravityScale = fallGravity;

        // ❗ ไม่ปิด simulated (จะได้ตกได้จริง)

        // 🔥 ปิด collider กันชนมั่ว
        if (col != null)
            col.enabled = false;

        // 🔥 กัน animation ตีกัน
        anim.ResetTrigger("Hit");
        anim.ResetTrigger("Attack");

        // 💀 เล่น animation ตาย
        anim.SetTrigger("Death");

        // ⏱ รอจนเล่นจบ
        yield return new WaitForSeconds(deathTime);

        Destroy(gameObject);
    }
}