using UnityEngine;

public class Bullet1 : BulletBase
{

    public int damage = 1;
    void Start()
    {
        
    }

    protected override void Move()
    {
        // 很纯粹的直线前进
        transform.position += (Vector3)direction * currentSpeed * Time.deltaTime;
    }
}
