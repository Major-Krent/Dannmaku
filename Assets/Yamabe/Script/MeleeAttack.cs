using UnityEngine;
using UnityEngine.EventSystems;

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] float damage = 10;
    [SerializeField] float attackLifetime = 0.2f;
    [SerializeField] float lifestealRatio = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, attackLifetime);
    }

    public void Initialize(float _damage, float lifesteal)
    {
        this.damage = _damage;
        lifestealRatio = lifesteal;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            Boss1Controller boss1Controller = collision.GetComponent<Boss1Controller>();
            boss1Controller.TakeDamage(damage);
        }
    }
}