using System.Collections;
using UnityEngine;

public class BarrierController : MonoBehaviour
{
    [Header("バリア設定")]
    [SerializeField] private int maxHits = 3;
    [SerializeField] private float hitInterval = 0.5f; // 防いだ後の無敵時間
    public int currentHits = 0;
    private bool _isInInterval = false; // インターバル中かどうかのフラグ

    [Header("オーディオ設定")]
    [SerializeField] private AudioClip deploySound;
    [SerializeField] private AudioClip hitSound; // publicから修正（必要に応じて）
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private float soundVolume = 1.0f;

    private SpriteRenderer spriteRenderer;
    private Player_Controller controller;

    void Start()
    {
        controller = GetComponentInParent<Player_Controller>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // プレイヤーに自分を登録
        if (controller != null)
        {
            controller.RegisterBarrier(this);
        }

        if (deploySound != null)
            AudioSource.PlayClipAtPoint(deploySound, Camera.main.transform.position, soundVolume);

        UpdateAlpha();
    }

    // プレイヤーがダメージを受ける瞬間にこれを呼び出す
    public void OnShieldHit()
    {
        if(_isInInterval)
        {
            return;
        }
        currentHits++;

        if (currentHits >= maxHits)
        {
            BreakBarrier();
        }
        else
        {
            UpdateAlpha();
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position, soundVolume);
            }

            StartCoroutine(HitIntervalRoutine());
            Debug.Log($"攻撃吸収。残り耐久: {maxHits - currentHits}");
        }
    }

    private IEnumerator HitIntervalRoutine()
    {
        _isInInterval = true;

        yield return new WaitForSeconds(hitInterval);
        _isInInterval = false;
    }

    private void UpdateAlpha()
    {
        if (spriteRenderer == null) return;

        // 透明度の計算: $alpha = \frac{maxHits - currentHits}{maxHits}$
        float alpha = (float)(maxHits - currentHits) / maxHits;
        alpha = Mathf.Max(alpha, 0.3f);

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    private void BreakBarrier()
    {
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, Camera.main.transform.position, soundVolume);
        }
        Destroy(gameObject);
    }
}