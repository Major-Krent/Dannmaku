using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] int attackDamage = 10;
    [SerializeField] float attackLifetime = 0.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, attackLifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {

        }
    }
}