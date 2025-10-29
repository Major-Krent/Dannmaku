using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [SerializeField] private float currentSpeed;
    [SerializeField] private GameObject obj;
    private Rigidbody2D rb;
    private Collider2D col;
    private float shotTime;
    private Vector2 moveInput;
    // --- 画面範囲の制限用 ---
    private Camera mainCamera;
    private Vector2 minBounds; // 移動可能な左下の座標
    private Vector2 maxBounds; // 移動可能な右上の座標
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        mainCamera = Camera.main;
        CalculateMoveBounds();
    }

    // Update is called once per frame
    void Update()
    {

        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveY = Input.GetAxisRaw("Vertical");   
        moveInput = new Vector2(moveX, moveY).normalized;
        rb.linearVelocity = moveInput * currentSpeed;
    }
    void LateUpdate()
    {
        Vector2 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);

        transform.position = clampedPosition;
    }

    void CalculateMoveBounds()
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
    }
}