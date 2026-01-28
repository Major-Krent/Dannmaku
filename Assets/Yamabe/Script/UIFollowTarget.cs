using UnityEngine;
using UnityEngine.UI;

public class UIFollowTarget : MonoBehaviour
{
    [Header("追尾対象")]
    [SerializeField] private Transform target; // プレイヤーのTransform
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0); // 頭上の位置調整

    private RectTransform _rectTransform;
    private Camera _mainCamera;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. プレイヤーのワールド座標にオフセットを足す
        Vector3 worldPos = target.position + offset;

        // 2. ワールド座標をスクリーン座標（画面上の位置）に変換
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

        // 3. スライダーの位置を更新
        transform.position = screenPos;
    }

    // プレイヤー側からターゲットを指定する場合用（生成時にセットするなど）
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}