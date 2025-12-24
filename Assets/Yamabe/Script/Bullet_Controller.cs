using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("弾の基本設定")]
    [SerializeField] float speed = 10.0f;
    [SerializeField] float lifetime = 3.0f;
    public float damage = 5;

    [Header("誘導設定")]
    [SerializeField] bool enableHoming = true;

    [Tooltip("旋回性能")]
    [SerializeField] float rotateSpeed = 200.0f;

    [Tooltip("敵を探知する半径")]
    [SerializeField] float detectionRadius = 10.0f;

    [Tooltip("誘導する視野角")]
    [Range(0, 360)]
    [SerializeField] float homingAngle = 90.0f;

    // 内部変数
    private Transform target;
    private Vector2 _moveDirection;
    private float _lifestealRatio;

    public void Initialize(float _damage)
    {
        this.damage = _damage;
        _moveDirection = transform.up;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);

        if (enableHoming)
        {
            FindClosestTargetInSight();
        }
    }

    void Update()
    {
        if (enableHoming && target != null)
        {
            if (IsTargetInSight(target))
            {
                HomingMovement();
            }
        }

        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void FindClosestTargetInSight()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                if (IsTargetInSight(hitCollider.transform))
                {
                    float distanceToEnemy = Vector2.Distance(transform.position, hitCollider.transform.position);

                    if (distanceToEnemy < shortestDistance)
                    {
                        shortestDistance = distanceToEnemy;
                        nearestEnemy = hitCollider.transform;
                    }
                }
            }
        }

        target = nearestEnemy;
    }

    private bool IsTargetInSight(Transform targetTransform)
    {
        if (targetTransform == null) return false;

        Vector2 directionToTarget = (targetTransform.position - transform.position).normalized;
        float angle = Vector2.Angle(transform.up, directionToTarget);

        return angle <= homingAngle / 2.0f;
    }

    private void HomingMovement()
    {
        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        direction.Normalize();

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}