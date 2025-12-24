using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Boss2Controller : EnemyBase
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

    [Header("レーザーPrefab")]
    [SerializeField] private GameObject warningLaserPrefab;  // 预警用
    [SerializeField] private GameObject damageLaserPrefab;   // 伤害用

    [Header("画面カバー用パラメータ")]
    [SerializeField] private int screenRayCount;        // 全屏斜线条数
    [SerializeField] private float screenHalfWidthWorld;  // 视野半宽(世界坐标)
    [SerializeField] private float screenHalfHeightWorld; // 视野半高(世界坐标)

    [Header("レーザー時間設定")]
    [SerializeField] private float laserWarningDuration; // 预警存在时间
    [SerializeField] private float laserDamageDuration;  // 伤害存在时间
    [SerializeField] private float radialSpinDuration;   // 环形激光旋转时间
    [SerializeField] private float radialSpinSpeed;       // 每秒旋转角速度（度）

    [Header("狙撃レーザー設定")]
    [SerializeField] private float aimTime;              // 瞄准时间

    // ===== 刀围成圆的沟壑技能（只在代码里调参）=====
    private float knifeSpacing = 0.6f;       // 相邻刀“沿圆周”的间隔（世界坐标弧长）
    private float craterWarningTime = 0.6f;  // 预警时间
    private float craterDamageTime = 1.2f;   // 伤害持续时间
    private bool knivesFollowBoss = false;   // true=刀环跟着Boss移动，false=生成后固定在原地

    [SerializeField] private GameObject craterKnifePrefab;

    private Animator anim;
    private Rigidbody2D rb;
    private bool isDie = false;
    private bool isDead = false;
    private bool isCasting=false;

    public Slider healthSlider;


    // 是否正在追玩家
    private bool isChasing = false;

    // 用来保证三个技能一轮都放一遍（0:Spray, 1:Fan, 2:Twelve）
    private List<int> remainingSkills = new List<int>();

    public enum BossPhase
    {
        Phase1, // 100% ~ 66%
        Phase2, // 66% ~ 33%
        Phase3  // 33% ~ 0
    }

    protected override void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        base.Start();              // 初始化 HP 等
        ResetSkillCycle();         // 初始化技能轮回表 [0,1,2]

        screenRayCount = 16;        // 全屏斜线条数
        screenHalfWidthWorld = 9f;  // 视野半宽(世界坐标)
        screenHalfHeightWorld = 5f; // 视野半高(世界坐标)

        laserWarningDuration = 0.8f; // 预警存在时间
        laserDamageDuration = 1.2f;  // 伤害存在时间
        radialSpinDuration = 2.5f;   // 环形激光旋转时间
        radialSpinSpeed = 60f;       // 每秒旋转角速度（度）

        aimTime = 1.0f;

    }

    protected void OnEnable()
    {
        bossLoopCoroutine = StartCoroutine(BossLoop()); // 开始主逻辑
        baseMoveSpeed = MoveSpeed;
        HP = 500f;
        currentHP = HP;
        healthSlider.maxValue = HP;
        healthSlider.value = currentHP;

        // 确保状态重置
        isChasing = false;
        isCasting = false;

        // 初始化移动速度
        baseMoveSpeed = MoveSpeed;

        // 重置技能循环
        ResetSkillCycle();

        // 启动主循环（防止重复启动，先停一下）
        if (bossLoopCoroutine != null) StopCoroutine(bossLoopCoroutine);
        bossLoopCoroutine = StartCoroutine(BossLoop());
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (isDead) return;
        base.Update();
        UpdatePhaseByHP();

        Animation_Control();
    }
    protected override void Move()
    {
        if (!isChasing) return;
        if (player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)dir * MoveSpeed * Time.deltaTime;
    }

    private void Animation_Control()
    {
        anim.SetBool("isChasing", isChasing);
        anim.SetBool("isCasting", isCasting);
    }

    public override void TakeDamage(float damage)
    {
        // 先用父类处理扣血 + 死亡
        if (isDie) return;
        base.TakeDamage(damage);
        healthSlider.value = currentHP;
        // 死了就不用再切阶段
        if (currentHP <= 0f) return;
    }


    private void UpdatePhaseByHP()
    {
        float hpPercent = currentHP / HP; // 0~1

        // 注意顺序：先判断第三阶段，再判断第二阶段
        if (hpPercent <= phase3Threshold && currentPhase != BossPhase.Phase3)
        {
            SkillSelectionManager.Instance.TriggerSkillSelection(3);
            EnterPhase3();
        }
        else if (hpPercent <= phase2Threshold && currentPhase == BossPhase.Phase1)
        {
            // 只允许从 1 -> 2（避免 3 再回 2）
            SkillSelectionManager.Instance.TriggerSkillSelection(3);
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
        isChasing = false;
        StartCoroutine(Wait());
        ResetSkillCycle(); // 看你要不要每次阶段重置技能轮回
    }
    private IEnumerator Wait()
    {
        anim.SetTrigger("isDie");
        isDie = true;
        if (bossLoopCoroutine != null)
        {
            StopCoroutine(bossLoopCoroutine);
        }
        yield return new WaitForSeconds(2f);
        SkillSelectionManager.Instance.TriggerSkillSelection(3);
        yield return new WaitForSeconds(1f);
        anim.SetTrigger("isFall");
        bossLoopCoroutine = StartCoroutine(BossLoop());
        isDie = false;
    }
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
                        yield return StartCoroutine(Skill_SlantLasers(45f));
                        break;
                    case 1:
                        StartCoroutine(Skill_CraterKnifeRing(2));
                        StartCoroutine(Skill_CraterKnifeRing(5));
                        yield return StartCoroutine(Skill_CraterKnifeRing(8));
                        break;
                    case 2:
                        yield return StartCoroutine(Skill_RadialSpinLaser());
                        break;
                    case 3:
                        yield return StartCoroutine(Skill_TargetLaser());
                        break;
                }
            }
            else if (currentPhase == BossPhase.Phase2)
            {
                switch (skillIndex)
                {
                    case 0:
                        StartCoroutine(Skill_SlantLasers(45f));
                        yield return new WaitForSeconds(0.8f);
                        yield return StartCoroutine(Skill_SlantLasers(135f));
                        break;
                    case 1:
                        StartCoroutine(Skill_CraterKnifeRing(3));
                        yield return new WaitForSeconds(1f);
                        yield return StartCoroutine(Skill_CraterKnifeRing(6));
                        break;
                    case 2:
                        yield return StartCoroutine(Skill_RadialSpinLaser());
                        break;
                    case 3:
                        StartCoroutine(Skill_TargetLaser());
                        yield return new WaitForSeconds(0.8f);
                        yield return StartCoroutine(Skill_TargetLaser());
                        break;
                }
            }
            else if (currentPhase == BossPhase.Phase3)
            {
                switch (skillIndex)
                {
                    case 0:
                        StartCoroutine(Skill_SlantLasers(45f));
                        yield return new WaitForSeconds(0.8f);
                        StartCoroutine(Skill_SlantLasers(90f));
                        yield return new WaitForSeconds(0.8f);
                        StartCoroutine(Skill_SlantLasers(135f));
                        yield return new WaitForSeconds(0.8f);
                        yield return StartCoroutine(Skill_SlantLasers(180f));
                        break;
                    case 1:
                        StartCoroutine(Skill_CraterKnifeRing(2));
                        yield return new WaitForSeconds(1f);
                        StartCoroutine(Skill_CraterKnifeRing(5));
                        yield return new WaitForSeconds(1f);
                        yield return StartCoroutine(Skill_CraterKnifeRing(8));
                        break;
                    case 2:
                        StartCoroutine(Skill_RadialSpinLaser());
                        yield return new WaitForSeconds(0.8f);
                        yield return StartCoroutine(Skill_RadialSpinLaser());
                        break;
                    case 3:
                        StartCoroutine(Skill_TargetLaser());
                        yield return new WaitForSeconds(0.4f);
                        yield return StartCoroutine(Skill_TargetLaser());
                        break;
                }
            }

            // 技能放完以后稍微停一下再进入下一轮
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 技能0/1：全屏斜向激光（angleDeg = 45 或 -45）
    /// </summary>
    private IEnumerator Skill_SlantLasers(float angleDeg)
    {
        isCasting=true;
        anim.SetTrigger("isCast1");
        // 射线方向（用来确定旋转角度）
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        // 垂直方向（用来沿着这个方向平移，铺满整个屏幕）
        Vector2 perp = new Vector2(-dir.y, dir.x).normalized;

        // 覆盖范围粗略估算：半宽 + 半高
        float halfLen = screenHalfWidthWorld + screenHalfHeightWorld;
        float totalLen = halfLen * 2f;
        float step = totalLen / (screenRayCount - 1);

        List<GameObject> warningList = new List<GameObject>();
        List<GameObject> damageList = new List<GameObject>();

        // 以 Boss 为中心铺满（也可以用摄像机中心）
        Vector2 center = transform.position;

        // 生成预警线
        for (int i = 0; i < screenRayCount; i++)
        {
            float offset = -halfLen + step * i;
            Vector2 pos = center + perp * offset;
            Quaternion rot = Quaternion.AngleAxis(angleDeg, Vector3.forward);
            GameObject warn = Instantiate(warningLaserPrefab, pos, rot);
            warningList.Add(warn);
        }

        // 预警时间
        yield return new WaitForSeconds(laserWarningDuration / actionSpeedMultiplier);

        // 替换成伤害激光
        foreach (GameObject warn in warningList)
        {
            if (warn == null) continue;
            Vector3 pos = warn.transform.position;
            Quaternion rot = warn.transform.rotation;
            Destroy(warn);
            GameObject dmg = Instantiate(damageLaserPrefab, pos, rot);
            damageList.Add(dmg);
        }
        warningList.Clear();

        // 伤害存在时间
        yield return new WaitForSeconds(laserDamageDuration / actionSpeedMultiplier);

        foreach (GameObject dmg in damageList)
        {
            if (dmg != null) Destroy(dmg);
        }
        damageList.Clear();
        isCasting = false;
    }

    /// <summary>
    /// 技能2：中心向外 360° 激光 + 旋转
    /// 每 30° 一道激光
    /// </summary>
    private IEnumerator Skill_RadialSpinLaser()
    {
        const int angleStep = 30;
        int count = 360 / angleStep;

        List<GameObject> warningList = new List<GameObject>();
        List<GameObject> damageList = new List<GameObject>();

        // ===== 1. 生成预警线 =====
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            GameObject warn = Instantiate(warningLaserPrefab, transform.position, rot);
            warningList.Add(warn);
        }

        yield return new WaitForSeconds(laserWarningDuration / actionSpeedMultiplier);

        // ===== 2. 预警线 → 伤害线 =====
        foreach (GameObject warn in warningList)
        {
            if (warn == null) continue;

            // 用预警线的旋转生成伤害线
            GameObject dmg = Instantiate(
                damageLaserPrefab,
                transform.position,
                warn.transform.rotation
            );

            damageList.Add(dmg);

            Destroy(warn);
        }
        warningList.Clear();

        // ===== 3. 旋转伤害线 =====
        float time = 0f;
        float duration = radialSpinDuration / actionSpeedMultiplier;

        while (time < duration)
        {
            float deltaAngle = radialSpinSpeed * Time.deltaTime * actionSpeedMultiplier;

            foreach (GameObject b in damageList)
            {
                if (b != null)
                {
                    b.transform.Rotate(0, 0, deltaAngle);
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        // ===== 4. 清理 =====
        foreach (GameObject b in damageList)
        {
            if (b != null)
                Destroy(b);
        }
        damageList.Clear();
    }

    /// <summary>
    /// 技能3：瞄准玩家一秒，然后发射激光
    /// </summary>
    private IEnumerator Skill_TargetLaser()
    {
        // 预警线，从 Boss 中心出发，朝玩家方向
        GameObject warn = Instantiate(warningLaserPrefab, transform.position, Quaternion.identity);

        float t = 0f;
        float aimDuration = aimTime / actionSpeedMultiplier;

        while (t < aimDuration)
        {
            if (player != null && warn != null)
            {
                Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                warn.transform.position = transform.position;
                warn.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            t += Time.deltaTime;
            yield return null;
        }

        // 替换成伤害激光
        GameObject dmg = null;
        if (warn != null)
        {
            Vector3 pos = warn.transform.position;
            Quaternion rot = warn.transform.rotation;
            Destroy(warn);
            dmg = Instantiate(damageLaserPrefab, pos, rot);
        }

        yield return new WaitForSeconds(laserDamageDuration / actionSpeedMultiplier);

        if (dmg != null) Destroy(dmg);
    }

    /// <summary>
    /// 技能4：cast一秒，然后knife ring
    /// </summary>
    private IEnumerator Skill_CraterKnifeRing(float radius)
    {
        isCasting = true;
        anim.SetTrigger("isCast2");
        yield return new WaitForSeconds(0.5f);
        if (craterKnifePrefab == null)
        {
            Debug.LogWarning("craterKnifePrefab is null.");
            yield break;
        }

        Vector2 center = transform.position;

        // 根据半径算周长 → 算刀数量
        float circumference = 2f * Mathf.PI * radius;
        int count = Mathf.Max(6, Mathf.RoundToInt(circumference / knifeSpacing));
        float angleStep = 360f / count;

        GameObject ringRoot = new GameObject("CraterKnifeRing");
        ringRoot.transform.position = center;
        if (knivesFollowBoss)
            ringRoot.transform.SetParent(transform);

        List<CraterKnife> knives = new List<CraterKnife>(count);

        for (int i = 0; i < count; i++)
        {
            float angleDeg = i * angleStep;
            float rad = angleDeg * Mathf.Deg2Rad;

            Vector2 pos = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            // 刀朝向圆心
            float lookToCenterAngle = angleDeg + 180f;
            Quaternion rot = Quaternion.Euler(0, 0, lookToCenterAngle);

            GameObject knifeObj = Instantiate(craterKnifePrefab, pos, rot, ringRoot.transform);

            var ck = knifeObj.GetComponent<CraterKnife>();
            if (ck != null)
            {
                ck.SetDamageEnabled(false); // 预警阶段不开伤害
                knives.Add(ck);
            }
            isCasting = false;
        }

        // 预警
        yield return new WaitForSeconds(craterWarningTime);

        // 开伤害
        foreach (var k in knives)
        {
            if (k != null) k.SetDamageEnabled(true);
        }

        // 伤害持续
        yield return new WaitForSeconds(craterDamageTime);

        Destroy(ringRoot);
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
        if (!isDead)
        {
            SkillSelectionManager.Instance.TriggerSkillSelection(3, true);
        }
        if (BossBattleManager.Instance != null)
        {
            BossBattleManager.Instance.OnBossDefeated();
        }
        isDead = true;
        Destroy(healthSlider.gameObject);
        base.Die();
        anim.SetTrigger("isDie");
        StopAllCoroutines();
        isChasing = false;
    }


}
