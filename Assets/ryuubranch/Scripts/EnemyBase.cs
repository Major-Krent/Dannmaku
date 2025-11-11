using UnityEngine;

public class EnemyBase : MonoBehaviour
{

    [Header("Boss")]
    [SerializeField] protected float HP;
    protected float currentHP;

    [Header("PlayeréQè∆")]
    [SerializeField] private Transform player;






    protected virtual void Start()
    {
        currentHP = HP;
    }

    protected virtual void Update()
    {
        Move();
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
    }
}
