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

public enum BossPhase
{
    Phase1, // 100% ~ 66%
    Phase2, // 66% ~ 33%
    Phase3  // 33% ~ 0
}


public class Boss1Controller : EnemyBase
{
    [Header("フェーズ")]
    private float phase2Threshold = 0.66f;
    private float phase3Threshold = 0.33f;
    private BossPhase currentPhase = BossPhase.Phase1;
    private Coroutine bossLoopCoroutine;

    [Header("移動関連")]
    [SerializeField] protected float MoveSpeed = 0.8f;
    [SerializeField] private float actionSpeedMultiplier = 1f;
    private float baseMoveSpeed;

    [Header("フェーズ別移動速度倍率")]
    [SerializeField] private float phase1MoveMul = 1.0f;
    [SerializeField] private float phase2MoveMul = 1.7f;
    [SerializeField] private float phase3MoveMul = 2.4f;

    [Header("フェーズ別行動速度倍率(数值越大越快)")]
    [SerializeField] private float phase1ActionMul = 1.0f;
    [SerializeField] private float phase2ActionMul = 1.3f;
    [SerializeField] private float phase3ActionMul = 1.6f;

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
    [SerializeField] private float sideBulletSpeed = 3f;     // 侧向子弹速度

    private Animator anim;
    private Rigidbody2D rb;
    bool isDashing = false;


    // 是否正在追玩家
    private bool isChasing = false;

    // 用来保证三个技能一轮都放一遍（0:Spray, 1:Fan, 2:Twelve）
    private List<int> remainingSkills = new List<int>();

    // ========== 生命周期 ==========
    protected override void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        base.Start();              // 初始化 HP 等
        ResetSkillCycle();         // 初始化技能轮回表 [0,1,2]
        bossLoopCoroutine = StartCoroutine(BossLoop()); // 开始主逻辑
        baseMoveSpeed = MoveSpeed;
        HP = 500f;
        currentHP = HP;

    }

    // 不写 Update()，让 EnemyBase.Update() 自己调用 Move()
    protected override void Update()
    {
        base.Update();
        anim.SetBool("isDashing", isDashing);
        UpdatePhaseByHP();
    }

    // ========== 移动逻辑（从 EnemyBase.Update() 调用） ==========
    protected override void Move()
    {
        if (!isChasing) return;
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)dir * MoveSpeed * Time.deltaTime;

    }

    // ========== フェーズ判定 ==========
    public override void TakeDamage(float damage)
    {
        // 先用父类处理扣血 + 死亡
        base.TakeDamage(damage);

        // 死了就不用再切阶段
        if (currentHP <= 0f) return;


    }

    private void UpdatePhaseByHP()
    {
        float hpPercent = currentHP / HP; // 0~1

        // 注意顺序：先判断第三阶段，再判断第二阶段
        if (hpPercent <= phase3Threshold && currentPhase != BossPhase.Phase3)
        {
            EnterPhase3();
        }
        else if (hpPercent <= phase2Threshold && currentPhase == BossPhase.Phase1)
        {
            // 只允许从 1 -> 2（避免 3 再回 2）
            EnterPhase2();
        }
    }

    private void EnterPhase2()
    {
        currentPhase = BossPhase.Phase2;

        // 调整移动速度 & 行动加速倍率
        MoveSpeed = baseMoveSpeed * phase2MoveMul;
        actionSpeedMultiplier = phase2ActionMul;

        Debug.Log("Enter Phase 2");

        // 如果你想 2 阶段用完全不同的技能循环，可以重启 Loop
        RestartBossLoop();
    }

    private void EnterPhase3()
    {
        currentPhase = BossPhase.Phase3;

        MoveSpeed = baseMoveSpeed * phase3MoveMul;
        actionSpeedMultiplier = phase3ActionMul;

        Debug.Log("Enter Phase 3");

        RestartBossLoop();
    }

    private void RestartBossLoop()
    {
        if (bossLoopCoroutine != null)
        {
            StopCoroutine(bossLoopCoroutine);
        }
        ResetSkillCycle(); // 看你要不要每次阶段重置技能轮回
        bossLoopCoroutine = StartCoroutine(BossLoop());
    }

    // ========== 主循环：追玩家3秒 -> 随机技能（保证一轮三种都用一次） ==========
    private IEnumerator BossLoop()
    {
        yield return new WaitForSeconds(1f); // 出场缓冲一下

        while (true)
        {
            // 1. 朝玩家移动3秒
            isChasing = true;
            float chaseTime = 3f / actionSpeedMultiplier;
            float t = 0f;
            while (t < chaseTime)
            {
                t += Time.deltaTime;
                yield return null; // 等待下一帧
            }
            isChasing = false;



            // 2. 随机选择一个还没用过的技能
            int skillIndex = GetNextRandomSkill();

            if (currentPhase == BossPhase.Phase1)
            {
                switch (skillIndex)
                {
                    case 0:
                        yield return StartCoroutine(Spray360());
                        break;
                    case 1:
                        yield return StartCoroutine(FanWaveTowardsPlayer(1, 0.9f));
                        break;
                    case 2:
                        yield return StartCoroutine(TwelveWayRotating(12));
                        break;
                    case 3:
                        yield return StartCoroutine(FanWaveTowardsPlayer(1, 0.9f));
                        break;
                }
            }
            else if (currentPhase == BossPhase.Phase2)
            {
                switch (skillIndex)
                {
                    case 0:
                        yield return StartCoroutine(Spray360());
                        yield return StartCoroutine(FanWaveTowardsPlayer(2, 0.4f));
                        break;
                    case 1:
                        yield return StartCoroutine(FanWaveTowardsPlayer(2, 0.4f));
                        break;
                    case 2:
                        yield return StartCoroutine(TwelveWayRotating(12));
                        break;
                    case 3:
                        yield return StartCoroutine(AimThenDashWithSideFire());
                        break;
                }
            }
            else if (currentPhase == BossPhase.Phase3)
            {
                switch (skillIndex)
                {
                    case 0:
                        yield return StartCoroutine(Spray360());
                        yield return StartCoroutine(FanWaveTowardsPlayer(3, 0.3f));
                        break;
                    case 1:
                        yield return StartCoroutine(FanWaveTowardsPlayer(3, 0.3f));
                        yield return StartCoroutine(Spray360());
                        break;
                    case 2:
                        yield return StartCoroutine(TwelveWayRotating(12));
                        break;
                    case 3:
                        yield return StartCoroutine(AimThenDashWithSideFire());
                        yield return StartCoroutine(AimThenDashWithSideFire());
                        yield return StartCoroutine(AimThenDashWithSideFire());
                        yield return StartCoroutine(Spray360());
                        break;
                }
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

        float spraySpeed = 2f * actionSpeedMultiplier; // 这一种攻击想用的速度

        for (int i = 0; i < shots; i++)
        {
            float angle = step * i;
            Vector2 dir = AngleToDir(angle);
            SpawnBullet(dir, spraySpeed);
            yield return new WaitForSeconds(delay);
        }
    }

    // ========== 技能2：朝玩家方向的扇形地震波 ==========
    private IEnumerator FanWaveTowardsPlayer(int rings_, float lag)
    {
        if (player == null || firePoint == null) yield break;

        int rings = rings_;
        int bulletsPerRing = 6;
        float fanAngle = 30f;
        float intervalBetweenRing = lag;

        float fanSpeed = 5f; // 这一种攻击用的速度

        yield return new WaitForSeconds(0.5f);

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
        float offsetBetweenWaves = 6f;
        float delayBetweenWaves = 0.5f;
        int fireTimes = (int)(times_ * actionSpeedMultiplier);

        float multiSpeed = 2f; // 这一种攻击用的速度
        for (int i = 0; i < fireTimes; i++)
        {

            // 第一轮
            FireMultiWay(offsetBetweenWaves * i, count, baseStep, multiSpeed);
            yield return new WaitForSeconds(delayBetweenWaves / actionSpeedMultiplier);
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
        isDashing = true;

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
        anim.SetTrigger("isFall");

        // 3) 冲刺结束，停止发弹
        isDashing = false;

    }

    private IEnumerator FireAfterCharge()
    {
        // 第一轮
        FireMultiWay(0, 12, 30, 2);
        yield return new WaitForSeconds(0.2f);
        // 第二轮（整体偏15度）
        FireMultiWay(15f, 12, 30, 2);
        yield return new WaitForSeconds(0.2f);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player_Bullet"))
        {
            BulletController bullet = collision.GetComponent<BulletController>();
            TakeDamage(bullet.damage);
            Destroy(collision.gameObject);
        }
    }



    protected override void Die()
    {
        Debug.Log("Boss1 Died");
        base.Die();
    }
}
