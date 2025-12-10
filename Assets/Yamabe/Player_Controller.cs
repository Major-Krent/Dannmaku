using System.Collections;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [Header("プレイヤー設定")]
    [SerializeField] private float playerSpeed;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float playerHp;
    [SerializeField] private float playerCurrentHp;
    [SerializeField] float currentFireRate;

    [SerializeField] private bool isMelee;

    [Header("遠距離攻撃設定")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] float playerAttackRate = 0.1f;
    [SerializeField] private float totalShots;

    [Header("近距離攻撃設定")]
    [SerializeField] GameObject attackPrefab;
    [SerializeField] float meleeAttackCooldown = 0.5f;

    [Header("共通攻撃設定")]

    [Header("ダッシュ設定")]
    public float dashSpeed = 15.0f;
    public float dashDuration = 0.2f;
    public float dashCooltime = 1.0f;
    [SerializeField] private bool _isDashing = false;

    [SerializeField] Transform firePoint;
    private SkillManager skillManager;
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
        skillManager = GetComponent<SkillManager>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        //CalculateMoveBounds();
    }

    // Update is called once per frame
    void Update()
    {
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
        currentFireRate = playerAttackRate / Mathf.Max(skillManager.TotalAttackARateMultiplier);
        if (Input.GetMouseButton(0) && Time.time > nextFireTime)
        {
            nextFireTime = Time.time + currentFireRate;
            StartCoroutine(PerformAttack());

        }
    }

    void MeleeAttack()
    {
        if (Input.GetMouseButton(0) && Time.time > nextAttackTime)
        {
            Debug.Log("埼玉！！");
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
        if (collision.CompareTag("Enemy_Bullet") && !_isDashing)
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

    private IEnumerator PerformAttack()
    {
        totalShots = 1 + skillManager.TotalExtraAttack;
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
            if (totalShots > 1)
            {
                yield return new WaitForSeconds(0.05f);
            }
        }
    } 
}