using System.Collections;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [Header("プレイヤー設定")]
    [SerializeField] private float playerSpeed;
    [SerializeField] private float playerHp;
    [SerializeField] private float playerCurrentHp;
    [SerializeField] private bool isMelee;

    [Header("遠距離攻撃設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] float playerAttackRate = 0.1f; 

    [Header("近距離攻撃設定")]
    [SerializeField] GameObject attackPrefab;
    [SerializeField] float meleeAttackCooldown = 0.5f;

    [Header("ダッシュ設定")]
    public float dashSpeed = 15.0f;      
    public float dashDuration = 0.2f;   
    public float dashCooltime = 1.0f;    
    [SerializeField] private bool _isDashing = false; 

    [SerializeField] Transform firePoint;
    private Rigidbody2D rb;
    private Collider2D col;
    private float shotTime;
    private float nextFireTime = 0.0f;
    private float nextAttackTime = 0.0f;
    private float _nextDashTime = 0.0f; 
    private Vector2 moveInput;
    private Camera mainCamera;
    void Start()
    {
        playerHp = 5;
        playerCurrentHp = playerHp;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        //CalculateMoveBounds();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();
        Dash();
        if (isMelee)
        {
            MeleeAttack();
        }
        else
        {
            ShootBullet();
        }
        
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

    private void Dash()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _nextDashTime)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        _isDashing = true;
        _nextDashTime = Time.time + dashCooltime; 

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {

            transform.Translate(moveInput * dashSpeed * Time.deltaTime, Space.World);

            yield return null;
        }

        _isDashing = false;
    }

void ShootBullet()
    {
        if (Input.GetMouseButton(0) && Time.time > nextFireTime)
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

    void MeleeAttack()
    {
        if (Input.GetMouseButton(0) && Time.time > nextAttackTime)
        {
            nextAttackTime = Time.time + meleeAttackCooldown;

            GameObject meleeAttack = Instantiate(attackPrefab, firePoint.position, firePoint.rotation, transform);

        }
    }

    public void TakeDamage(float damage)
    {
        playerCurrentHp -= damage;
        if (playerCurrentHp < 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy_Bullet")&&!_isDashing)
        {
            Bullet1 bullet = collision.GetComponent<Bullet1>();
            TakeDamage(bullet.damage);
            Destroy(collision.gameObject);
        }
    }

    private void Die()
    {
        Debug.Log("Player die");
        Destroy(gameObject);
    }
}