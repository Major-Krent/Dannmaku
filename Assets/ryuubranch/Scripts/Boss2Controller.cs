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

    private Animator anim;
    private Rigidbody2D rb;
    bool isDashing = false;
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
        bossLoopCoroutine = StartCoroutine(BossLoop()); // 开始主逻辑
        baseMoveSpeed = MoveSpeed;
        HP = 500f;
        currentHP = HP;
        healthSlider.maxValue = HP;
        healthSlider.value = currentHP;

    }

    // Update is called once per frame
    void Update()
    {
        UpdatePhaseByHP();
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
        if (bossLoopCoroutine != null)
        {
            StopCoroutine(bossLoopCoroutine);
        }
        ResetSkillCycle(); // 看你要不要每次阶段重置技能轮回
        bossLoopCoroutine = StartCoroutine(BossLoop());
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

                        break;
                    case 1:

                        break;
                    case 2:

                        break;
                    case 3:

                        break;
                }
            }
            else if (currentPhase == BossPhase.Phase2)
            {
                switch (skillIndex)
                {
                    case 0:

                        break;
                    case 1:

                        break;
                    case 2:

                        break;
                    case 3:

                        break;
                }
            }
            else if (currentPhase == BossPhase.Phase3)
            {
                switch (skillIndex)
                {
                    case 0:

                        break;
                    case 1:

                        break;
                    case 2:

                        break;
                    case 3:

                        break;
                }
            }

            // 技能放完以后稍微停一下再进入下一轮
            yield return new WaitForSeconds(1f);
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
        SkillSelectionManager.Instance.TriggerSkillSelection(3, true);
        Debug.Log("Boss1 Died");
        Destroy(healthSlider.gameObject);
        Destroy(gameObject);
        base.Die();
    }


}
