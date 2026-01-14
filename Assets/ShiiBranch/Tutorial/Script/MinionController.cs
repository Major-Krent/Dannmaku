using UnityEngine;
using System.Collections;

public class MinionController : EnemyBase
{
    [SerializeField] protected float moveSpeed = 2.0f;
    [SerializeField] private float stopDistance = 1.2f;

    private Rigidbody2D rb;
    private Animator anim; 
    private bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = false;
        }
        HP = 10f;
        currentHP = HP;
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        base.Start();

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

            Vector2 newPos = rb.position + dir * moveSpeed * Time.deltaTime;
            rb.MovePosition(newPos);
            if (anim != null) anim.SetBool("isWalking", true);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            if (anim != null) anim.SetBool("isWalking", false);
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
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        StartCoroutine(DestroyAfterDelay());

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        StartCoroutine(DestroyAfterDelay());

        base.Die();
    }
    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(3.0f);
        Destroy(gameObject);
    }
}
