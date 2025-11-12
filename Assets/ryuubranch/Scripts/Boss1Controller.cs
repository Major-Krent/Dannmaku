using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AttackPattern
{
    public string name;
    public float bulletSpeed;
    public int bulletCount;
    public float fanAngle;
    public float interval;
    public bool isCircle;
}

public class Boss1Controller : EnemyBase
{
    [Header("移動関連")]
    [SerializeField] protected float MoveSpeed = 3f;

    [Header("弾幕関連")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed;              // 现在暂时没用，先留着
    [SerializeField] private AttackPattern[] attackPatterns; // 以后扩展用

    [Header("ダッシュ")]
    [SerializeField] private float aimDuration = 1.2f;      // 瞄准时长
    [SerializeField] private float dashSpeed = 10f;          // 冲刺速度
    [SerializeField] private float dashDuration = 0.8f;      // 冲刺时长
    [SerializeField] private float sideFireInterval = 0.08f; // 冲刺中侧向发弹的间隔
    [SerializeField] private float sideBulletSpeed = 6f;     // 侧向子弹速度

    // 是否正在追玩家
    private bool isChasing = false;

    // 用来保证三个技能一轮都放一遍（0:Spray, 1:Fan, 2:Twelve）
    private List<int> remainingSkills = new List<int>();

    // ========== 生命周期 ==========
    protected override void Start()
    {
        base.Start();              // 初始化 HP 等
        ResetSkillCycle();         // 初始化技能轮回表 [0,1,2]
        StartCoroutine(BossLoop()); // 开始主逻辑
    }

    // 不写 Update()，让 EnemyBase.Update() 自己调用 Move()

    // ========== 移动逻辑（从 EnemyBase.Update() 调用） ==========
    protected override void Move()
    {
        if (!isChasing) return;
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)dir * MoveSpeed * Time.deltaTime;
    }

    // ========== 主循环：追玩家3秒 -> 随机技能（保证一轮三种都用一次） ==========
    private IEnumerator BossLoop()
    {
        yield return new WaitForSeconds(1f); // 出场缓冲一下

        while (true)
        {
            // 1. 朝玩家移动3秒
            isChasing = true;
            float chaseTime = 2f;
            float t = 0f;
            while (t < chaseTime)
            {
                t += Time.deltaTime;
                yield return null; // 等待下一帧
            }
            isChasing = false;

            // 2. 随机选择一个还没用过的技能
            int skillIndex = GetNextRandomSkill();

            switch (skillIndex)
            {
                case 0:
                    yield return StartCoroutine(Spray360());
                    break;
                case 1:
                    yield return StartCoroutine(FanWaveTowardsPlayer());
                    break;
                case 2:
                    yield return StartCoroutine(TwelveWayRotating(12));
                    break;
                case 3:
                    yield return StartCoroutine(AimThenDashWithSideFire());
                    break;
            }

            // 技能放完以后稍微停一下再进入下一轮
            yield return new WaitForSeconds(1f);
        }
    }

    // ========== 技能轮回管理 ==========
    private void ResetSkillCycle()
    {
        remainingSkills.Clear();
        remainingSkills.Add(0);
        remainingSkills.Add(1);
        remainingSkills.Add(2);
        remainingSkills.Add(3);
    }

    private int GetNextRandomSkill()
    {
        if (remainingSkills.Count == 0)
        {
            ResetSkillCycle();
        }

        int randomIndex = Random.Range(0, remainingSkills.Count);
        int skill = remainingSkills[randomIndex];
        remainingSkills.RemoveAt(randomIndex);
        return skill;
    }

    // ========== 工具函数：角度 <-> 方向 ==========
    private Vector2 AngleToDir(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private float DirToAngle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    // ========== 技能1：360度喷水（一发一发转圈） ==========
    private IEnumerator Spray360()
    {
        int shots = 60;
        float totalAngle = 360f;
        float step = totalAngle / shots;
        float delay = 0.005f;

        float spraySpeed = 2f; // 这一种攻击想用的速度

        for (int i = 0; i < shots; i++)
        {
            float angle = step * i;
            Vector2 dir = AngleToDir(angle);
            SpawnBullet(dir, spraySpeed);
            yield return new WaitForSeconds(delay);
        }
    }

    // ========== 技能2：朝玩家方向的扇形地震波 ==========
    private IEnumerator FanWaveTowardsPlayer()
    {
        if (player == null || firePoint == null) yield break;

        int rings = 5;
        int bulletsPerRing = 10;
        float fanAngle = 60f;
        float intervalBetweenRing = 0.2f;

        float fanSpeed = 6f; // 这一种攻击用的速度

        Vector2 toPlayer = (player.position - firePoint.position).normalized;
        float centerAngle = DirToAngle(toPlayer);

        for (int ring = 0; ring < rings; ring++)
        {
            float startAngle = centerAngle - fanAngle * 0.5f;
            float step = (bulletsPerRing <= 1) ? 0f : fanAngle / (bulletsPerRing - 1);

            for (int i = 0; i < bulletsPerRing; i++)
            {
                float angle = startAngle + step * i;
                Vector2 dir = AngleToDir(angle);
                SpawnBullet(dir, fanSpeed);
            }

            yield return new WaitForSeconds(intervalBetweenRing);
        }
    }

    // ========== 技能3：12方向两轮，第二轮整体偏15度 ==========
    private IEnumerator TwelveWayRotating(int times_)
    {
        int count = 12;
        float baseStep = 30f;
        float offsetBetweenWaves = 15f;
        float delayBetweenWaves = 0.2f;
        int fireTimes=times_;

        float multiSpeed = 2f; // 这一种攻击用的速度
        for(int i = 0; i < fireTimes; i++)
        {

        // 第一轮
        FireMultiWay(0f, count, baseStep, multiSpeed);
        yield return new WaitForSeconds(delayBetweenWaves);

        // 第二轮（整体偏15度）
        FireMultiWay(offsetBetweenWaves, count, baseStep, multiSpeed);
        yield return new WaitForSeconds(delayBetweenWaves);
        }
        StartCoroutine(Spray360());
    }

    private void FireMultiWay(float startAngle, int count, float step, float speed)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;
            Vector2 dir = AngleToDir(angle);
            SpawnBullet(dir, speed);
        }
    }

    // ========== 技能4：冲刺并发射侧向弹幕 ==========
    private IEnumerator AimThenDashWithSideFire()
    {
        if (firePoint == null)
            yield break;

        // 1) 瞄准阶段：持续面向玩家并更新“最后瞄准方向”
        Vector2 lastAimDir = Vector2.right; // 默认给个方向，防止玩家为 null
        float t = 0f;
        while (t < aimDuration)
        {
            t += Time.deltaTime;

            if (player != null)
                lastAimDir = (player.position - firePoint.position).normalized;

            // 若需要朝向对齐（2D转Z朝向），可加：transform.right = lastAimDir;
            yield return null;
        }

        // 2) 冲刺阶段：沿最后瞄准方向位移，同时侧向发弹
        bool isDashing = true;
        // 启动一个并行协程进行侧向发弹
        IEnumerator sideFire = SideFireRoutine(lastAimDir, () => isDashing);
        StartCoroutine(sideFire);

        float dashTime = 0f;
        while (dashTime < dashDuration)
        {
            dashTime += Time.deltaTime;

            // 用 transform 直接位移（若用刚体可改成 rb.velocity = ...）
            transform.position += (Vector3)(lastAimDir * dashSpeed * Time.deltaTime);
            yield return null;
        }

        // 3) 冲刺结束，停止发弹
        isDashing = false;

        StartCoroutine(TwelveWayRotating(2));
    }
    private IEnumerator SideFireRoutine(Vector2 dashDir, System.Func<bool> isDashingGetter)
    {
        // dashDir 的法线向量
        Vector2 perpRight = new Vector2(-dashDir.y, dashDir.x);
        Vector2 perpLeft = -perpRight;

        // 按间隔持续发射，直到外部把 isDashing 置为 false
        while (isDashingGetter())
        {
            // 右侧一发
            SpawnBullet(perpRight, sideBulletSpeed);
            // 左侧一发
            SpawnBullet(perpLeft, sideBulletSpeed);

            yield return new WaitForSeconds(sideFireInterval);
        }
    }

    // ========== 生成子弹 ==========
    private void SpawnBullet(Vector2 dir, float speed)
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject obj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        BulletBase bullet = obj.GetComponent<BulletBase>();  // 例如 StraightBullet : BulletBase
        if (bullet != null)
        {
            bullet.Init(dir, speed);
        }
    }

    protected override void Die()
    {
        Debug.Log("Boss1 Died");
        base.Die();
    }
}
