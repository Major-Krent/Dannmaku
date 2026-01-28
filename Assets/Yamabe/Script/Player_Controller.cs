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
    [SerializeField] public bool isDashing = false;

    [Header("被ダメージ設定")]
    [SerializeField] float damageInvincibleTime = 1.5f;
    [SerializeField] float flashInterval = 0.1f;
    [SerializeField] Color damageColor = Color.red;

    [Header("チャージ設定")]
    [SerializeField] private float maxChargeTime = 1.0f; 
    private float currentChargeTimer = 0f;
    private bool isCharging = false;

    [Header("UI設定")]
    [SerializeField] private Slider chargeSlider;

    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private Color defaultColor=Color.white;

    [SerializeField] Transform firePoint;
    private SkillManager skillManager;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private float shotTime;
    private float nextFireTime = 0.0f;
    private float nextAttackTime = 0.0f;
    private float nextDashTime = 0.0f;
    private Vector2 moveInput;
    private Camera mainCamera;
    private bool isDie=false;
    private bool isBarrierActive = false;
    private BarrierController _activeBarrier;
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
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer != null)
        {
            defaultColor=spriteRenderer.color;
        }
        if(chargeSlider != null)
        {
            chargeSlider.gameObject.SetActive(false);
        }
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
        isDie = false;
        animator.Play("Idle");
    }
    public void RegisterBarrier(BarrierController barrier)
    {
        _activeBarrier = barrier;
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
            if (!isDashing)
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
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > nextDashTime)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooltime;

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
        isDashing = false;
    }

    void ShootBullet()
    {
        if (skillManager.ChargeAttack)
        {
            if(Input.GetMouseButtonDown(0))
            {
                isCharging = true;
                currentChargeTimer = 0f;

                chargeSlider.gameObject.SetActive(true);
                chargeSlider.value = 0f;
            }

            if(Input.GetMouseButton(0))
            {
                currentChargeTimer+=Time.deltaTime;
                float ratio = Mathf.Clamp01(currentChargeTimer / maxChargeTime);
                if (chargeSlider!=null)
                {
                    chargeSlider.value=ratio;
                }
            }

            if(Input.GetMouseButtonUp(0))
            {
                float chargeRatio = Mathf.Clamp01(currentChargeTimer / maxChargeTime);

                if(chargeRatio>=0.5f)
                {
                    StartCoroutine(PerformChargedAttack(chargeRatio));
                }
                else
                {
                    StartCoroutine(PerformRangedAttack());
                }

                isCharging=false;
                currentChargeTimer=0f;
                if(chargeSlider!=null)
                {
                    chargeSlider.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            currentFireRate = playerAttackRate / Mathf.Max(skillManager.TotalAttackARateMultiplier);
            if (Input.GetMouseButton(0) && Time.time > nextFireTime)
            {
                nextFireTime = Time.time + currentFireRate;
                StartCoroutine(PerformRangedAttack());

            }
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

    public void SetBarrierActive(bool isActive)
    {
        isBarrierActive=isActive;
    }

    public void TakeDamage(float damage)
    {
        if(_activeBarrier!=null&&!isDashing)
        {
            _activeBarrier.OnShieldHit();
            return;
        }

        if (!isDie)
        {
            if (isInvincible) return;
            playerCurrentHp -= damage;
            UpdateHealthUI();
            if (playerCurrentHp < 0)
            {
                Die();
            }
            StartCoroutine(BecomeInvincible(damageInvincibleTime));
            StartCoroutine(DamageFlashRoutine());
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
        if (collision.CompareTag("Enemy_Bullet") && !isDashing)
        {
            Bullet1 bullet = collision.GetComponent<Bullet1>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
                Destroy(collision.gameObject);
            }
        }

        if (collision.CompareTag("Enemy_Bullet") && !isDashing)
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
        rb.linearVelocity = new(0f, 0f);
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
                bulletScript.Initialize(currentRangedDamage,skillManager.HomingShot);
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

    private IEnumerator PerformChargedAttack(float ratio)
    {
        animator.Play("RangeAttack");
        totalShots = 1 + skillManager.TotalExtraAttack;
        float chargeDamageMultiplier = 1f + (ratio * 2f);
        float finalDamage = (rangedDamage * skillManager.TotalDamageMultiplier) * chargeDamageMultiplier;
        for (int i = 0; i < totalShots; i++)
        {
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

            // 弾の大きさをチャージ量に合わせて大きくする
            bullet.transform.localScale *= (3f + ratio);

            BulletController bulletScript = bullet.GetComponent<BulletController>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(finalDamage, skillManager.HomingShot);
            }

            if (totalShots > 1)
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
    }

    private IEnumerator BecomeInvincible(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    private IEnumerator DamageFlashRoutine()
    {
        float timer = 0f;

        while (timer < damageInvincibleTime)
        {
            // 赤くする
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashInterval);

            // 元の色に戻す（または透明度を下げる）
            spriteRenderer.color = defaultColor;
            yield return new WaitForSeconds(flashInterval);

            timer += flashInterval * 2;
        }

        // 最後に確実に元の色に戻し、無敵を解除
        spriteRenderer.color = defaultColor;
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