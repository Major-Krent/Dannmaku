using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player_Controller : MonoBehaviour
{
    [Header("プレイヤー設定")]
    [SerializeField] private float playerSpeed;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float playerHp;
    [SerializeField] private float playerCurrentHp;
    [SerializeField] float currentFireRate;

    [SerializeField] private bool isMelee;
    public bool IsMelee => isMelee;

    [Header("遠距離攻撃設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] float playerAttackRate = 0.1f;
    [SerializeField] private float totalShots;
    [SerializeField] private float rangedDamage;
    [SerializeField] private float currentRangedDamage;

    [Header("近距離攻撃設定")]
    [SerializeField] GameObject attackPrefab;
    [SerializeField] float meleeAttackCooldown = 0.5f;
    [SerializeField] float meleeDamage;
    [SerializeField] float currentMeleeDamage;

    [Header("共通攻撃設定")]

    [Header("ダッシュ設定")]
    [SerializeField] float dashSpeed = 15.0f;
    [SerializeField] float dashDuration = 0.2f;
    [SerializeField] float dashCooltime = 1.0f;
    [SerializeField] private bool _isDashing = false;
    [Header("無敵時間設定")]
    [SerializeField] float damageInvincibleTime = 1.5f;
    private bool _isInvincible = false;

    [SerializeField] Transform firePoint;
    private SkillManager skillManager;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private float shotTime;
    private float nextFireTime = 0.0f;
    private float nextAttackTime = 0.0f;
    private float _nextDashTime = 0.0f;
    private Vector2 moveInput;
    private Camera mainCamera;
    private bool isDie=false;
    [SerializeField] private Slider hpBarSlider;
    void Start()
    {
        playerCurrentHp = playerHp;
        UpdateHealthUI();
        skillManager = GetComponent<SkillManager>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();
        //CalculateMoveBounds();
        //---------------------------------------------
        BindCamera();
        BindUI();
        UpdateHealthUI();
        SceneManager.sceneLoaded += OnSceneLoaded;
        //---------------------------------------------
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        playerCurrentHp = playerHp;
        BindCamera();
        BindUI();
        UpdateHealthUI();
    }
    private void BindUI()
    {
        
        if (hpBarSlider == null)
        {
            
            GameObject uiObj = GameObject.Find("PlayerHpBar");
            if (uiObj != null)
            {
                hpBarSlider = uiObj.GetComponent<Slider>();
            }
            else
            {
               
                Debug.LogWarning("PlayerHpBarを見つからない");
            }
        }
    }
    private void BindCamera()
    {
        mainCamera = Camera.main;

        var vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = this.transform;
            Debug.Log("Cinemachine Camera re-bound to Player.");
        }
    }
    private void UpdateHealthUI()
    {
        if (hpBarSlider != null)
        {
            hpBarSlider.value = Mathf.Clamp01(playerCurrentHp / playerHp);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!isDie)
        {
            HandleFlip();
            if (!_isDashing)
            {
                MovePlayer();
            }
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
        currentSpeed = playerSpeed * skillManager.TotalMoveSpeedMultiplier;
        rb.linearVelocity = moveInput * currentSpeed;
        if(moveInput.magnitude>0.1f)
        {
            animator.SetBool("Walk", true);
        }
        else
        {
            animator.SetBool("Walk", false);
        }
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
        Vector2 dashDirection = moveInput;
        if (dashDirection == Vector2.zero)
        {
            dashDirection = Vector2.zero;
        }
        while (Time.time < startTime + dashDuration)
        {

            rb.linearVelocity = dashDirection.normalized * dashSpeed;

            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
        _isDashing = false;
    }

    void ShootBullet()
    {
        currentFireRate = playerAttackRate / Mathf.Max(skillManager.TotalAttackARateMultiplier);
        if (Input.GetMouseButton(0) && Time.time > nextFireTime)
        {
            nextFireTime = Time.time + currentFireRate;
            StartCoroutine(PerformRangedAttack());

        }
    }

    void MeleeAttack()
    {
        
        currentFireRate = meleeAttackCooldown / Mathf.Max(skillManager.TotalAttackARateMultiplier);
        if (Input.GetMouseButton(0) && Time.time > nextAttackTime)
        {
            
            Debug.Log("埼玉！！");
            nextAttackTime = Time.time + currentFireRate;
            StartCoroutine(PerformMeleeAttack());
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isDie)
        {
            animator.Play("TakeDamage");
            if (_isInvincible) return;
            playerCurrentHp -= damage;
            UpdateHealthUI();
            if (playerCurrentHp < 0)
            {
                Die();
            }
            StartCoroutine(BecomeInvincible(damageInvincibleTime));
        }
    }
    public void Heal(float amount)
    {
        playerCurrentHp += amount;

        if (playerCurrentHp > playerHp)
        {
            playerCurrentHp = playerHp;
        }
        UpdateHealthUI();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy_Bullet") && !_isDashing)
        {
            Bullet1 bullet = collision.GetComponent<Bullet1>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
                Destroy(collision.gameObject);
            }
        }

        if (collision.CompareTag("Enemy_Bullet") && !_isDashing)
        {

            Lazer1 lazer = collision.GetComponent<Lazer1>();
            if (lazer != null)
            {
                TakeDamage(lazer.damage);
            }
        }
    }
    //HPを初期化
    public void Revive()
    {
        playerCurrentHp = playerHp;
        //animator
    }
    private void Die()
    {
        animator.Play("Die");
        Debug.Log("Player die");
        isDie = true;
        //Destroy(gameObject);

        //ゲームオーバーUIを呼び出す
        GameOverManager gm = FindFirstObjectByType<GameOverManager>();
        if (gm != null)
        {
            gm.ShowGameOver();
        }
    }

    private IEnumerator PerformRangedAttack()
    {
        animator.Play("RangeAttack");
        currentRangedDamage = rangedDamage * skillManager.TotalDamageMultiplier;
        totalShots = 1 + skillManager.TotalExtraAttack;

        for (int i = 0; i < totalShots; i++)
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            if (mainCamera == null) yield break;

            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = mainCamera.nearClipPlane;
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            Vector2 direction = (new Vector2(mouseWorldPos.x, mouseWorldPos.y) - (Vector2)firePoint.position).normalized;

            if (direction == Vector2.zero)
            {
                direction = Vector2.up;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle - 90f);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
            BulletController bulletScript = bullet.GetComponent<BulletController>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(currentRangedDamage);
            }
            if (totalShots > 1)
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private IEnumerator PerformMeleeAttack()
    {
        float lifeSteel = skillManager.TotalLifestealRatio;
        currentMeleeDamage = meleeDamage * skillManager.TotalDamageMultiplier;
        totalShots = 1 + skillManager.TotalExtraAttack;

        for (int i = 0; i < totalShots; i++)
        {
            animator.Play("MeleeAttack");
            GameObject meleeAttack = Instantiate(attackPrefab, firePoint.position, firePoint.rotation, transform);
            MeleeAttack meleeScript = meleeAttack.GetComponent<MeleeAttack>();
            if (meleeScript != null)
            {
                meleeScript.Initialize(currentMeleeDamage, lifeSteel);
            }
            if (totalShots > 1)
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private IEnumerator BecomeInvincible(float duration)
    {
        _isInvincible = true;
        yield return new WaitForSeconds(duration);
        _isInvincible = false;
    }

    void HandleFlip()
    {
        // マウスのワールド座標を取得
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // プレイヤーの右側にマウスがあるかチェック
        if (mousePos.x > transform.position.x)
        {
            // 右向き（元のサイズ）
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            // 左向き（Xをマイナスにして反転）
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}