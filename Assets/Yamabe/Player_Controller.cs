using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [SerializeField] private float playerSpeed;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] float playerAttackRate = 0.1f; // 0.1秒に1回発射
    [SerializeField] Transform firePoint;
    private Rigidbody2D rb;
    private Collider2D col;
    private float shotTime;
    private float nextFireTime = 0.0f; 
    private Vector2 moveInput;
    // --- 画面範囲の制限用 ---
    private Camera mainCamera;
    /*private Vector2 minBounds; // 移動可能な左下の座標
    private Vector2 maxBounds; // 移動可能な右上の座標/*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        //CalculateMoveBounds();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        ShootBullet();
    }
    void LateUpdate()
    {
        Vector2 clampedPosition = transform.position;
        /*clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);*/

        transform.position = clampedPosition;
    }

    void MovePlayer()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;
        rb.linearVelocity = moveInput * playerSpeed;
    }

    void ShootBullet()
    {
        if (Input.GetMouseButton(0)&& Time.time > nextFireTime)
        {
            nextFireTime = Time.time + playerAttackRate;

            // --- ここからがマウス方向への発射ロジック ---

            // 1. マウスのスクリーン座標を取得
            Vector3 mouseScreenPos = Input.mousePosition;
            // Z座標をカメラのZ座標に合わせる (2DならこれでOK)
            mouseScreenPos.z = mainCamera.nearClipPlane;

            // 2. スクリーン座標をワールド座標に変換
            // (重要) カメラが Perspective(透視投影) の場合は Z座標の扱いが重要ですが、
            // 2Dシューティングの Orthographic(平行投影) なら Z はあまり気にせず変換できます
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

            // 3. 発射地点 (FirePoint) からマウス位置への「方向ベクトル」を計算
            // (目的地の座標) - (自分の座標)
            Vector2 direction = (new Vector2(mouseWorldPos.x, mouseWorldPos.y) - (Vector2)firePoint.position).normalized;

            // 4. (安全装置) もし方向が (0, 0) なら (＝マウスが発射地点と重なっている場合)
            if (direction == Vector2.zero)
            {
                // とりあえず上 (Y+) に飛ぶように設定
                direction = Vector2.up;
            }

            // 5. 弾が進行方向を向くように「角度」を計算 (オプション)
            // (スプライトが元々「上」を向いている前提)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // スプライトの元の向きに合わせて 90度 補正
            Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);

            // 6. 弾を生成 (Instantiate)
            // 角度(rotation)もここで指定
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
            nextFireTime = Time.time + playerAttackRate;
            //Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
    /* void CalculateMoveBounds()
     {
         if (mainCamera == null)
         {
             Debug.LogError("Main Camera is not found. Please set a MainCamera tag.");
             return;
         }

         Vector2 playerHalfSize = col.bounds.extents;

         Vector2 screenMin = mainCamera.ViewportToWorldPoint(new Vector2(0, 0));
         Vector2 screenMax = mainCamera.ViewportToWorldPoint(new Vector2(1, 1));

         minBounds = new Vector2(screenMin.x + playerHalfSize.x, screenMin.y + playerHalfSize.y);
         maxBounds = new Vector2(screenMax.x - playerHalfSize.x, screenMax.y - playerHalfSize.y);
     }*/
}