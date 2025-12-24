using UnityEngine;

public class AbsorptionBarrier : MonoBehaviour
{
    [Header("バリア設定")]
    [Tooltip("何発まで防げるか（耐久値）")]
    public int maxHits = 3;

    private int currentHits = 0;

    void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // "EnemyBullet"（敵の弾）に当たった場合
        if (collision.CompareTag("Enemy_Bullet"))
        {
            AbsorbAttack(collision.gameObject);
        }
    }

    private void AbsorbAttack(GameObject enemyBullet)
    {
        // 1. 敵の弾を破壊する
        Destroy(enemyBullet);

        // 2. エフェクトやログ
        Debug.Log("敵の攻撃を吸収しました！");

        currentHits++;
        if (currentHits >= maxHits)
        {
            Debug.Log("バリアが壊れました！");
            Destroy(gameObject);
        }
    }
}