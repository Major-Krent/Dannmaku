using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Tooltip("弾の移動速度")]
    public float speed = 10.0f;

    [Tooltip("弾が消えるまでの時間（秒）")]
    public float lifetime = 3.0f;

    public int damage = 1;

    void Start()
    {
        // 弾が生成されてから 'lifetime' 秒後に自動的に消滅(Destroy)する
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(transform.up * speed * Time.deltaTime, Space.World);
    }

    // --- 発展：弾が何かに当たった時の処理 ---

    // この機能を使うには、弾のプレハブに
    // 1. Rigidbody 2D (Is Kinematic = true)
    // 2. Collider 2D (Is Trigger = true)
    // がアタッチされている必要があります。
    //
    // 敵にも Collider 2D が必要です。

    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // "Enemy" タグを持つオブジェクトに当たった場合
        if (collision.CompareTag("Enemy"))
        {
            // 敵にダメージを与える処理などをここに追加
            // (例: collision.GetComponent<EnemyHealth>().TakeDamage(10);)

            // 弾を消滅させる
            Destroy(gameObject);
        }
    }
    */
}