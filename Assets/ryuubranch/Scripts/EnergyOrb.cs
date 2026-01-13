using UnityEngine;

public class EnergyOrb : MonoBehaviour
{
    private Transform target;

    // ===== 参数 =====
    private float scatterSpeed;
    private float seekSpeed;
    private float scatterDuration;

    // ===== 内部状态 =====
    private Vector2 scatterDir;
    private float timer;
    private bool isSeeking = false;

    public void Init(
        Transform target_,
        float scatterSpeed_,
        float seekSpeed_,
        float scatterDuration_
    )
    {
        target = target_;
        scatterSpeed = scatterSpeed_;
        seekSpeed = seekSpeed_;
        scatterDuration = scatterDuration_;

        // 随机散开方向
        scatterDir = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        if (!isSeeking)
        {
            // ===== 第一阶段：从 Boss 身上散开 =====
            timer += Time.deltaTime;
            transform.position += (Vector3)(scatterDir * scatterSpeed * Time.deltaTime);

            if (timer >= scatterDuration)
            {
                isSeeking = true;
            }
        }
        else
        {
            // ===== 第二阶段：飞向玩家 =====
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 dir = (target.position - transform.position).normalized;
            transform.position += dir * seekSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, target.position) < 0.25f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isSeeking && collision.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
