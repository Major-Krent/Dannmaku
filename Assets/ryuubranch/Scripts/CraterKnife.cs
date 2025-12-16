using UnityEngine;

public class CraterKnife : MonoBehaviour
{
    [Header("Damage")]

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false; // 默认关
    }

    public void SetDamageEnabled(bool enabled)
    {
        if (col != null) col.enabled = enabled;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // TODO：换成你项目里玩家受伤脚本
        var p = other.GetComponent<Player_Controller>(); // <- 改成你的玩家脚本名
        if (p != null)
        {
            p.TakeDamage(1);
        }
    }
}
