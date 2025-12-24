using UnityEngine;

public class EnemyBase : MonoBehaviour
{

    [Header("Boss")]
    protected float HP;
    [SerializeField] protected float currentHP;

    [Header("PlayerŽQÆ")]
    [SerializeField] protected Transform player;






    protected virtual void Start()
    {
        currentHP = HP;
    }

    protected virtual void Update()
    {
        Move();
        UpdateFacing();
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
        if (currentHP < 0)
        {
            Die();
        }
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
