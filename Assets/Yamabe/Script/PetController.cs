using UnityEngine;

public class PetController : MonoBehaviour
{
    [Header("í«è]ê›íË")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 offset = new Vector3(-1f, 1f, 0f);
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float floatingAmplitude = 0.2f;
    [SerializeField] private float floatingFrequency = 2f;

    [Header("çUåÇê›íË")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float fireRate = 0.8f;
    [SerializeField] private float baseDamage = 3f;

    private float _nextFireTime;
    private Vector3 _startPos;

    void Start()
    {
        if (playerTransform == null) playerTransform = transform.parent;
        transform.SetParent(null);
    }

    void Update()
    {
        if (playerTransform == null) return;

        FollowPlayer();
        AutoAttack();
    }

    private void FollowPlayer()
    {
        Vector3 targetPos = playerTransform.position + offset;

        targetPos.y += Mathf.Sin(Time.time * floatingFrequency) * floatingAmplitude;

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    private void AutoAttack()
    {
        if (Time.time < _nextFireTime) return;

        Transform target = FindClosestEnemy();
        if (target != null)
        {
            Shoot(target);
            _nextFireTime = Time.time + fireRate;
        }
    }

    private Transform FindClosestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = enemy.transform;
                }
            }
        }
        return closest;
    }

    private void Shoot(Transform target)
    {
        Vector2 direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.Euler(0, 0, angle));

        BulletController bc = bullet.GetComponent<BulletController>();
    }
}