using UnityEngine;

public class HomingBulletController : MonoBehaviour
{
    [Header("弾の基本設定")]
    public float speed = 10.0f;
    public float lifetime = 3.0f;

    [Header("誘導（ホーミング）設定")]
    public bool enableHoming = true;

    [Tooltip("旋回性能")]
    public float rotateSpeed = 200.0f;

    [Tooltip("敵を探知する半径")]
    public float detectionRadius = 10.0f;

    [Tooltip("誘導する視野角（度）。これより外側に敵がいると誘導をやめます")]
    [Range(0, 360)]
    public float homingAngle = 90.0f; // ⬅️ 追加: 前方90度（左右45度）以内なら反応

    // 内部変数
    private Transform target;
    private Vector2 _moveDirection;
    private float _damage;
    private float _lifestealRatio;

    public void Initialize(Vector2 dir, float damage, float lifesteal)
    {
        _moveDirection = dir;
        _damage = damage;
        _lifestealRatio = lifesteal;
        if (_moveDirection == Vector2.zero) _moveDirection = transform.up;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);

        if (enableHoming)
        {
            FindClosestTargetInSight(); // ⬅️ 視野内の敵を探すメソッドに変更
        }
    }

    void Update()
    {
        if (enableHoming && target != null)
        {
            // ターゲットがまだ存在し、かつ「前方（視野角内）」にいるか確認
            if (IsTargetInSight(target))
            {
                HomingMovement();
            }
            else
            {
                // ターゲットが視野外（真横や後ろ）に行った場合
                // ここで target = null; にするとロックオンが外れます（今回は外さないまま直進させます）
            }
        }

        // 常に自分の向いている方向に進む
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    /// <summary>
    /// 半径内 かつ 視野角内 の一番近い敵を探す
    /// </summary>
    private void FindClosestTargetInSight()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                // ⬅️ 追加: 視野角のチェック
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

    /// <summary>
    /// 対象が視野角（homingAngle）の中にいるか判定する
    /// </summary>
    private bool IsTargetInSight(Transform targetTransform)
    {
        if (targetTransform == null) return false;

        // 自分から敵への方向ベクトル
        Vector2 directionToTarget = (targetTransform.position - transform.position).normalized;

        // 自分の正面（transform.up）と、敵への方向との角度差（0～180度）を取得
        float angle = Vector2.Angle(transform.up, directionToTarget);

        // 角度差が「視野角の半分」以下なら、視野に入っているとみなす
        // 例: homingAngleが90度なら、左右45度以内ならOK
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