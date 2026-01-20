using UnityEngine;

public class AbsorptionBarrier : MonoBehaviour
{
    [Header("バリア設定")]
    [Tooltip("何発まで防げるか（耐久値）")]
    [SerializeField]private int maxHits = 3;

    private int currentHits = 0;
    [Header("オーディオ設定")]
    [Tooltip("展開時の音")]
    [SerializeField] private AudioClip deploySound;
    [Tooltip("被弾時の音")] 
    public AudioClip hitSound;
    [Tooltip("破壊時の音")]
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private float soundVolume = 1.0f;
    

    private SpriteRenderer spriteRenderer;
    private Player_Controller controller;
    void Start()
    {
        controller = GetComponentInParent<Player_Controller>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        AudioSource.PlayClipAtPoint(deploySound, Camera.main.transform.position, soundVolume);
        UpdateAlpha();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // "EnemyBullet"（敵の弾）に当たった場合
        if (collision.CompareTag("Enemy_Bullet")&&!controller._isDashing)
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
        UpdateAlpha();
        if (currentHits >= maxHits)
        {
            Debug.Log("バリアが壊れました！");
            BreakBarrier();
        }
        else
        {

            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position, soundVolume);
            }

            Debug.Log($"攻撃吸収。残り耐久: {maxHits - currentHits}");
        }
    }
    private void UpdateAlpha()
    {
        if (spriteRenderer == null) return;


        float alpha = (float)(maxHits - currentHits) / maxHits;

        alpha = Mathf.Max(alpha, 0.3f); 

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
    private void BreakBarrier()
    {
        Debug.Log("バリアが壊れました！");
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position, soundVolume);
        }
        Destroy(gameObject);
    }
}