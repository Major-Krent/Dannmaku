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
            float chaseTime = 3f;
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
                    yield return StartCoroutine(TwelveWayRotating());
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
        float delay = 0.05f;

        float spraySpeed = 4f; // 这一种攻击想用的速度

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
    private IEnumerator TwelveWayRotating()
    {
        int count = 12;
        float baseStep = 30f;
        float offsetBetweenWaves = 15f;
        float delayBetweenWaves = 0.2f;

        float multiSpeed = 8f; // 这一种攻击用的速度
        for(int i = 0; i < 12;i++)
        {

        // 第一轮
        FireMultiWay(0f, count, baseStep, multiSpeed);
        yield return new WaitForSeconds(delayBetweenWaves);

        // 第二轮（整体偏15度）
        FireMultiWay(offsetBetweenWaves, count, baseStep, multiSpeed);
        yield return new WaitForSeconds(delayBetweenWaves);
        }
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
