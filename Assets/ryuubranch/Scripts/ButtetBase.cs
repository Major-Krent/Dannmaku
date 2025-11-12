using UnityEngine;


[System.Serializable]
public class BulletConfig
{
    public float baseSpeed = 5f;   // 默认速度
    public float baseDamage = 10f; // 默认伤害（以后用得上）
    public float lifeTime = 5f;    // 子弹最长存在时间
}
public abstract class BulletBase : MonoBehaviour
{
    [Header("共通設定")]
    [SerializeField] protected BulletConfig config;

    // 运行时用的实际值（可以被 Init 改）
    protected Vector2 direction;
    protected float currentSpeed;
    protected float currentDamage;

    protected virtual void Awake()
    {
        // 默认用配置里的值
        currentSpeed = config.baseSpeed;
        currentDamage = config.baseDamage;
    }

    public virtual void Init(Vector2 dir, float? overrideSpeed = null, float damageMultiplier = 1f)
    {
        direction = dir.normalized;
        currentSpeed = overrideSpeed ?? config.baseSpeed;
        currentDamage = config.baseDamage * damageMultiplier;

        // 统一控制生命周期
        Destroy(gameObject, config.lifeTime);
    }

    protected virtual void Update()
    {
        Move();
    }

    protected abstract void Move();

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // 这里可以放通用的命中处理，
        // 比如检查 Tag == "Player" 时扣血，然后 Destroy 自己
        /*
        if (other.CompareTag("Player"))
        {
            // PlayerController pc = other.GetComponent<PlayerController>();
            // pc.TakeDamage(currentDamage);
            Destroy(gameObject);
        }
        */
    }
}
