using UnityEngine;
using System.Collections;

public class EnemyBase : MonoBehaviour
{

    [Header("Boss")]
    protected float HP;
    [SerializeField] protected float currentHP;

    [Header("PlayeréQè∆")]
    [SerializeField] protected Transform player;

    protected SpriteRenderer sprite;
    [SerializeField] protected Color hitColor = Color.red;
    [SerializeField] protected float flashDuration = 0.1f;
    [SerializeField] protected int flashCount = 2;

    private Color originalColor;
    private Coroutine hitFlashCo;




    protected virtual void Start()
    {
        currentHP = HP;

        if (sprite == null)
            sprite = GetComponentInChildren<SpriteRenderer>();

        if (sprite != null)
            originalColor = sprite.color;
    }

    private void OnEnable()
    {
        FindPlayer();
    }

    protected virtual void Update()
    {
        Move();
        UpdateFacing();
        if (player == null)
        {
            FindPlayer();
            return;
        }
    }

    private void FindPlayer()   
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    private void UpdateFacing()
    {
        if (player == null) return;

        float dirX = player.position.x - transform.position.x;

        if (dirX > 0.01f)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (dirX < -0.01f)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    protected virtual void Move()
    {

    }

    public virtual void TakeDamage(float damage)
    {
        currentHP -= damage;
        PlayHitFlash();

        if (currentHP < 0)
        {
            Die();
        }
    }

    protected void PlayHitFlash()
    {
        if (sprite == null) return;

        if (hitFlashCo != null)
            StopCoroutine(hitFlashCo);

        hitFlashCo = StartCoroutine(HitFlashCoroutine());
    }

    private IEnumerator HitFlashCoroutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            sprite.color = hitColor;
            yield return new WaitForSeconds(flashDuration);

            sprite.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }

        sprite.color = originalColor;
        hitFlashCo = null;
    }

    protected virtual void Die()
    {
        Debug.Log("BossDied");
        LevelTimer timer = FindFirstObjectByType<LevelTimer>();
        if (timer != null)
        {
            timer.StopAndRecord();
        }
        LevelEnvironment env = FindFirstObjectByType<LevelEnvironment>();
        if (env != null)
        {
            env.OpenPath();
        }
    }
}
