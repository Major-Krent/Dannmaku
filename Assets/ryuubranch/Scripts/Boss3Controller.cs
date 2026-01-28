using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class Boss3Controller : EnemyBase
{
    private enum ActionType { Melee, Ranged }

    private float phase2Threshold = 0.70f;
    private float phase3Threshold = 0.45f;
    private BossPhase currentPhase = BossPhase.Phase1;
    public enum BossPhase
    {
        Phase1, // 100% ~ 66%
        Phase2, // 66% ~ 33%
        Phase3  // 33% ~ 0
    }

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    [Header("HP UI")]
    [SerializeField] private Slider healthSlider;

    [Header("無敵")]
    [SerializeField] private bool invincible;

    [Header("行動ループ(节奏)")]
    [SerializeField] private float roamTimeMin = 0.8f;
    [SerializeField] private float roamTimeMax = 1.6f;
    [SerializeField] private float restAfterAction = 0.35f;
    [SerializeField] private float actionCooldown = 0.7f; // 每次动作之间的最小间隔（防连发）

    private Coroutine loopCo;
    private float nextActionReadyTime;

    [Header("Roam 不规则高速移动")]
    [SerializeField] private float roamSpeed;
    [SerializeField] private float roamRepathInterval;
    [SerializeField] private float roamRadiusAroundPlayer;
    [SerializeField] private float roamNoiseStrength;
    private Vector2 roamTarget;
    private float nextRoamRepathTime;

    [Header("子弹判定 -> Roll")]
    [SerializeField] private string playerBulletTag = "Player_Bullet";
    [SerializeField] private Vector2 bulletSenseBoxSize = new Vector2(3.0f, 1.6f);
    [SerializeField] private Vector2 bulletSenseOffset = new Vector2(1.6f, 0f); // 面朝右为正
    [SerializeField] private float bulletSenseInterval = 0.03f;
    [SerializeField] private float rollCooldown = 0.9f;

    [Header("Roll")]
    [SerializeField] private float rollDuration = 0.32f;
    [SerializeField] private float rollSpeed = 15f;

    private float nextBulletSenseTime;
    private float nextRollReadyTime;
    private bool isRolling;
    private bool meleeAnimFinished;

    [Header("距离与选择")]
    [SerializeField] private float meleeRange;
    [SerializeField] private float rangedMinRange;
    [SerializeField] private float rangedMaxRange;
    [SerializeField, Range(0f, 1f)] private float rangedChance;

    [Header("攻击时逼近")]
    [SerializeField] private float chaseSpeedDuringAction;

    [Header("Melee")]
    [SerializeField] private float meleeApproachTimeout = 2.0f;
    [SerializeField] private float meleeStartDistance;
    [SerializeField] private float meleeRecover;
    [SerializeField] private float meleeStopDistance;
    [SerializeField] private float sideOffsetX;   // 站到玩家左右多远
    [SerializeField] private float sideAlignTolerance = 0.15f;

    [Header("Ranged(光蛋)")]
    [SerializeField] private float rangedWindup;
    [SerializeField] private float rangedRecover;
    [SerializeField] private GameObject lightEggPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float lightEggSpeed;

    [Header("脱离 Dash(攻击后)")]
    [SerializeField] private float separationDashSpeed;
    [SerializeField] private float separationDashTime;

    [Header("Phase Change Energy Orbs")]
    [SerializeField] private GameObject energyOrbPrefab;
    [SerializeField] private int energyOrbCount = 8;
    [SerializeField] private float orbScatterSpeed = 4f;
    [SerializeField] private float orbScatterDuration = 0.4f;
    [SerializeField] private float orbSeekSpeed = 7f;

    public Boss3Melee boss3melee;
    Collider2D meleeCollider;

    private bool isActing =false;
    protected override void Start()
    {
        base.Start();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!anim) anim = GetComponent<Animator>();
        meleeCollider = boss3melee.GetComponent<Collider2D>();
        PickNewRoamTarget();
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

        SpawnEnergyOrbsToPlayer();
        Debug.Log("Enter Phase 2");
    }
    private void EnterPhase3()
    {
        currentPhase = BossPhase.Phase3;

        SpawnEnergyOrbsToPlayer();
        Debug.Log("Enter Phase 3");
    }

    private void OnEnable()
    {
        if (meleeCollider == null)
        {
            Debug.Log("meleenull");
            meleeCollider = GetComponentInChildren<Collider2D>();
        }
        sideOffsetX = 0.5f;

        roamSpeed = 9f;
        roamRepathInterval = 1.5f;
        roamRadiusAroundPlayer = 12f;
        roamNoiseStrength = 1;

        meleeRange = 6f;
        meleeStartDistance = 2f;
        rangedMinRange = 3.5f;
        rangedMaxRange = 9f;
        rangedChance = 0.6f;

        chaseSpeedDuringAction = 10.5f;

        meleeRecover = 0.20f;
        meleeStopDistance = 1.2f;
        

        rangedWindup = 0.33f;
        rangedRecover = 0.22f;
        lightEggSpeed = 20f;

        separationDashSpeed = 12f;
        separationDashTime = 0.18f;

        // 你要在这里设HP也行（沿用你Boss2习惯）
        HP = 500f;
        currentHP = HP;

        if (healthSlider)
        {
            healthSlider.maxValue = HP;
            healthSlider.value = currentHP;
        }

        invincible = false;
        isRolling = false;
        nextActionReadyTime = 0f;

        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = StartCoroutine(BossLoop());

        if (meleeCollider) meleeCollider.enabled = false;
    }

    protected override void Update()
    {
        base.Update();
        UpdatePhaseByHP();
        if (healthSlider) healthSlider.value = currentHP;
    }

    protected override void Move()
    {
        if (player == null || rb == null) return;

        // 任意时刻检测子弹
        TickBulletSense();
    }

    // ===================== Loop=====================
    private IEnumerator BossLoop()
    {
        yield return new WaitForSeconds(0.8f);

        while (true)
        {
            // 1) Roam 一段时间（期间持续检测子弹，可能被打断 Roll）
            float roamTime = Random.Range(roamTimeMin, roamTimeMax);
            float t = 0f;

            while (t < roamTime)
            {
                // 如果正在翻滚，就不要Roam推速度，避免抢控制
                if (!isRolling)
                    TickRoam();

                t += Time.deltaTime;
                yield return null;
            }

            // 2) 全局动作CD：没到就继续小Roam一会儿
            if (Time.time < nextActionReadyTime)
            {
                yield return new WaitForSeconds(1.5f);
                continue;
            }

            // 3) 选动作（按距离+概率）
            ActionType action = ChooseAction();

            // 4) 执行动作（动作过程中也会检测子弹，但一般不建议打断动作；这里保持不打断）
            if (action == ActionType.Melee)
                yield return StartCoroutine(CoMelee());
            else {
                yield return StartCoroutine(CoRanged());
                yield return StartCoroutine(CoRanged());
                yield return StartCoroutine(CoRanged());
            }


            nextActionReadyTime = Time.time + actionCooldown;

            // 5) 休息一下再进入下一轮
            yield return new WaitForSeconds(restAfterAction);
        }
    }

    private ActionType ChooseAction()
    {
        float d = Vector2.Distance(rb.position, player.position);

        if (d <= meleeRange)
            return ActionType.Melee;

        if(d >= rangedMinRange && d <= rangedMaxRange && Random.value < rangedChance)
            return ActionType.Ranged;

        //// 默认：如果不满足远程条件，就尽量近战（让战斗更紧凑）
        return ActionType.Melee;
    }

    // ===================== Bullet -> Roll =====================
    private void TickBulletSense()
    {
        if (isActing) return; // ✅ 攻击中不允许翻滚打断

        if (Time.time < nextBulletSenseTime) return;
        nextBulletSenseTime = Time.time + bulletSenseInterval;

        if (Time.time < nextRollReadyTime) return;
        if (isRolling) return;

        int facingSign = transform.localScale.x >= 0 ? 1 : -1;
        Vector2 origin = (Vector2)transform.position + new Vector2(bulletSenseOffset.x * facingSign, bulletSenseOffset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(origin, bulletSenseBoxSize, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && hits[i].CompareTag(playerBulletTag))
            {
                StartCoroutine(CoRollPerpendicularRandom());
                break;
            }
        }
    }


    private bool IsInAction()
    {
        // 你后面如果要扩展动作类型，这里也方便改
        // 目前：我们不想让翻滚打断动作，所以动作期返回true
        return false;
    }

    private IEnumerator CoRollPerpendicularRandom()
    {
        isRolling = true;
        nextRollReadyTime = Time.time + rollCooldown;

        Vector2 baseDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        if (baseDir.sqrMagnitude < 0.001f) baseDir = Vector2.right;

        Vector2 perp = new Vector2(-baseDir.y, baseDir.x);
        int sign = Random.value < 0.5f ? 1 : -1;
        Vector2 rollDir = (perp * sign).normalized;

        SetInvincible(true);
        anim?.SetTrigger("Roll");

        float end = Time.time + rollDuration;
        while (Time.time < end)
        {
            rb.linearVelocity = rollDir * rollSpeed;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        SetInvincible(false);

        isRolling = false;
    }

    // ===================== Roam =====================
    private void TickRoam()
    {
        if (Time.time >= nextRoamRepathTime)
        {
            nextRoamRepathTime = Time.time + roamRepathInterval;
            PickNewRoamTarget();
        }

        Vector2 pos = rb.position;
        Vector2 toTarget = roamTarget - pos;
        if (toTarget.sqrMagnitude <= 0.25f) // 0.5m以内算“到点”，你可以调
        {
            PickNewRoamTarget();
            toTarget = roamTarget - pos;
        }
        Vector2 dir = toTarget.normalized;
        float tt = Time.time * 2.5f;
        Vector2 noise = new Vector2(Mathf.PerlinNoise(tt, 0.2f) - 0.5f, Mathf.PerlinNoise(0.2f, tt) - 0.5f);
        Vector2 finalDir = (dir + noise * roamNoiseStrength).normalized;

        rb.linearVelocity = finalDir * roamSpeed;
    }

    private void PickNewRoamTarget()
    {
        Vector2 center = (Vector2)player.position;
        roamTarget = center + Random.insideUnitCircle * roamRadiusAroundPlayer;
    }

    private void MoveTowardPlayer(float speed)
    {
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    // ===================== Melee =====================
    private IEnumerator CoMelee()
    {
        if (player == null) yield break;
        isActing = true;     // ✅ 上锁
        meleeAnimFinished = false;
        meleeAnimFinished = false;

        int side = (rb.position.x < player.position.x) ? -1 : 1;
        Vector2 targetPos = new Vector2(
            player.position.x + side * sideOffsetX,
            player.position.y
        );

        float start = Time.time;
        float dy = targetPos.y - rb.position.y;
        // 先跑到 targetPos（世界坐标左/右点）
        while (Time.time - start < 3)
        {
            Vector2 toTarget = targetPos - rb.position;
            float dist = toTarget.magnitude;

            // 到位
            if ((dy>=-0.4||dy <= 0.4)&& dist<1)
                break;

            rb.linearVelocity = (toTarget / dist) * chaseSpeedDuringAction* 1.6f;
            anim?.SetTrigger("Melee");
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // 走不到就别挥空
        if (Vector2.Distance(rb.position, targetPos) > meleeStartDistance)
            yield break;

        // 到位后再出刀

        yield return new WaitUntil(() => meleeAnimFinished);
        yield return new WaitForSeconds(meleeRecover);
        isActing = false;     // ✅ 上锁

    }


    public void OnMeleeAnimFinished()
    {
        meleeAnimFinished = true;
    }

    public void OpenHitBox()
    {
        Debug.Log("open");
        if (meleeCollider) meleeCollider.enabled = true;
    }

    public void CloseHitBox()
    {
        if (meleeCollider) meleeCollider.enabled = false;
    }

    // ===================== Ranged =====================
    private IEnumerator CoRanged()
    {
        anim?.SetTrigger("Ranged");

        float end = Time.time + rangedWindup;
        while (Time.time < end)
        {
            // 前摇可以轻追，也可以不追
            MoveTowardPlayer(chaseSpeedDuringAction * 0.6f);
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        ShootLightEgg();

        yield return new WaitForSeconds(rangedRecover);
    }

    private void ShootLightEgg()
    {
        if (!lightEggPrefab || !shootPoint) return;

        Vector2 dir = ((Vector2)player.position - (Vector2)shootPoint.position).normalized;
        GameObject egg = Instantiate(lightEggPrefab, shootPoint.position, Quaternion.identity);

        var proj = egg.GetComponent<LightEgg>();
        if (proj != null) proj.Launch(dir, lightEggSpeed);
        else
        {
            var prb = egg.GetComponent<Rigidbody2D>();
            if (prb) prb.linearVelocity = dir * lightEggSpeed;
        }
    }

    // ===================== Separation Dash =====================
    private IEnumerator CoSeparationDash()
    {
        if (meleeCollider) meleeCollider.enabled = false;
        // 侧跳：Boss->Player 的垂直方向随机左右
        Vector2 baseDir = ((Vector2)player.position - rb.position).normalized;
        if (baseDir.sqrMagnitude < 0.001f) baseDir = Vector2.right;

        Vector2 perp = new Vector2(-baseDir.y, baseDir.x);
        int sign = Random.value < 0.5f ? 1 : -1;
        Vector2 dashDir = (perp * sign).normalized;

        float end = Time.time + separationDashTime;
        while (Time.time < end)
        {
            rb.linearVelocity = dashDir * separationDashSpeed;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
    }

    // ===================== Damage / Die =====================
    private void SetInvincible(bool on) => invincible = on;

    public override void TakeDamage(float damage)
    {
        if (invincible) return;
        base.TakeDamage(damage);
        if (healthSlider) healthSlider.value = currentHP;
    }

    protected override void Die()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        rb.linearVelocity = Vector2.zero;
        // 1️⃣ 停止一切行为
        StopAllCoroutines();

        // 3️⃣ 关闭碰撞 & 刚体
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        anim?.SetTrigger("isDie");
        base.Die();
    }

    protected void DestroyBoss()
    {
        Destroy(gameObject);
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

    private void SpawnEnergyOrbsToPlayer()
    {
        StartCoroutine(Wait());
        if (energyOrbPrefab == null || player == null)
            return;

        for (int i = 0; i < energyOrbCount; i++)
        {
            // ⭐ 注意：生成点 = Boss 本体
            GameObject orb = Instantiate(
                energyOrbPrefab,
                transform.position,
                Quaternion.identity
            );

            EnergyOrb energyOrb = orb.GetComponent<EnergyOrb>();
            if (energyOrb != null)
            {
                energyOrb.Init(
                    player,
                    orbScatterSpeed,
                    orbSeekSpeed,
                    orbScatterDuration
                );
            }
        }
    }

    private IEnumerator Wait()
    {
        isActing = true;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(2f);
        SkillSelectionManager.Instance.TriggerSkillSelection(3);
        yield return new WaitForSeconds(1f);
        isActing = false;

        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = StartCoroutine(BossLoop());
    }

}
