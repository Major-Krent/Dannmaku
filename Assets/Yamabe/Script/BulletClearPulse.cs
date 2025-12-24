using UnityEngine;

public class BulletClearPulse : MonoBehaviour
{
    [Header("消去設定")]
    public float radius = 100.0f;       // 消去する範囲
    public string bulletTag = "Enemy_Bullet"; // 消したい弾のタグ

    void Start()
    {
        ClearBullets();

        // エフェクトが終わる頃にオブジェクトを消す
        Destroy(gameObject, 1.0f);
    }

    private void ClearBullets()
    {
        // 1. 中心（プレイヤー）から一定範囲内のコライダーをすべて取得
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hitColliders)
        {
            // 2. タグが敵の弾なら破壊する
            if (hit.CompareTag(bulletTag))
            {
                // パーティクルなどを出したい場合はここで生成
                Destroy(hit.gameObject);
            }
        }

        Debug.Log($"{radius} 範囲内の弾を消去しました。");
    }

    // Unityエディタ上で範囲を見やすくするためのデバッグ表示
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}