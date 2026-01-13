using UnityEngine;
using UnityEngine.EventSystems;

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] float damage = 10;
    [SerializeField] float attackLifetime = 0.2f;
    [SerializeField] float lifestealRatio = 0;

    private Player_Controller ownerPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ownerPlayer = GetComponentInParent<Player_Controller>();
        Destroy(gameObject, attackLifetime);
    }

    public void Initialize(float _damage, float lifesteal)
    {
        this.damage = _damage;
        lifestealRatio = lifesteal;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyBase enemy = collision.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            TryLifesteal();
        }
    }
    private void TryLifesteal()
    {
        if (lifestealRatio > 0 && ownerPlayer != null)
        {
            float healAmount = damage * lifestealRatio;
            ownerPlayer.Heal(healAmount); 
           Debug.Log($"‹zŒŒ: {healAmount}");
        }
    }

}