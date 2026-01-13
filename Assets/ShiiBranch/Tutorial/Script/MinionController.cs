using UnityEngine;
using System.Collections;

public class MinionController : EnemyBase
{
    [SerializeField] protected float moveSpeed = 2.0f;
    [SerializeField] private float stopDistance = 3.0f;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackInterval = 2.0f; 
    [SerializeField] private float bulletSpeed = 5.0f;

    private Rigidbody2D rb;
    private Animator anim; 
    private bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        HP = 30f;
        currentHP = HP;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        base.Start();

        StartCoroutine(AttackRoutine());
    }

    // Update is called once per frame
    protected override void Update()
    {

        if (isDead) return;
        base.Update();
    }
    protected override void Move()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > stopDistance)
        {
            Vector2 dir = (player.position - transform.position).normalized;

            if (dir.x > 0) transform.localScale = new Vector3(1, 1, 1);
            else if (dir.x < 0) transform.localScale = new Vector3(-1, 1, 1);

            transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;
            if (anim != null) anim.SetBool("isWalking", true);
        }
        {
            if (anim != null) anim.SetBool("isWalking", false);
        }
    }
    private IEnumerator AttackRoutine()
    {

        yield return new WaitForSeconds(1.0f);

        while (!isDead)
        {
            if (player != null && Vector2.Distance(transform.position, player.position) < 15f)
            {
                ShootAtPlayer();
            }
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void ShootAtPlayer()
    {
        if (isDead) return;
        if (bulletPrefab == null || firePoint == null || player == null) return;

        Vector2 dir = (player.position - firePoint.position).normalized;

        GameObject obj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        BulletBase bullet = obj.GetComponent<BulletBase>();
        if (bullet != null)
        {
            bullet.Init(dir, bulletSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Player_Bullet"))
        {
            BulletController bullet = collision.GetComponent<BulletController>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
            }
            Destroy(collision.gameObject); 
        }
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;
        base.TakeDamage(damage);
        if (anim != null) anim.SetTrigger("Hurt");
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        StartCoroutine(DestroyAfterDelay());

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Destroy(gameObject, 0.5f);

        base.Die();
    }
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject);
    }
}
