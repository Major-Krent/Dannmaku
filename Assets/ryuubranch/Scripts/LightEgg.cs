using UnityEngine;

public class LightEgg : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float lifeTime = 4f;
    public int damage = 1;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    public void Launch(Vector2 dir, float speed)
    {
        if (rb) rb.linearVelocity = dir.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

    }
}
