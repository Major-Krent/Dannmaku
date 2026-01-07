using UnityEngine;

public class UniquePetController : MonoBehaviour
{
    [Header("追従設定")]
    [SerializeField] private Transform playerTransform; // プレイヤーのTransform
    [SerializeField] private Vector3 offset = new Vector3(-1f, 1f, 0f); // プレイヤーとの位置関係
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float floatingAmplitude = 0.2f; // ふわふわ揺れる幅
    [SerializeField] private float floatingFrequency = 2f;  // ふわふわ揺れる速さ

    [Header("攻撃設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float fireRate = 0.8f;
    [SerializeField] private float baseDamage = 3f;

    private float _nextFireTime;
    private Vector3 _startPos;

    void Start()
    {
        // プレイヤーが未設定なら親から取得
        if (playerTransform == null) playerTransform = transform.parent;
        // 親子関係を解除して、追従ロジックで動かす（親子だと動きが硬いため）
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
        // プレイヤーの目標位置を計算
        Vector3 targetPos = playerTransform.position + offset;

        // ふわふわした動きを追加
        targetPos.y += Mathf.Sin(Time.time * floatingFrequency) * floatingAmplitude;

        // スムーズに移動
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

        // 既存の BulletController を使用
        BulletController bc = bullet.GetComponent<BulletController>();
    }

    // 索敵範囲をエディタ上で見えるようにする（デバッグ用）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}