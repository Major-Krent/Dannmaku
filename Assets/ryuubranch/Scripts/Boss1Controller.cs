using UnityEngine;


[System.Serializable]
public class AttackPattern
{
    public string name;
    public float bulletSpeed;
    public int bulletCount;
    public float fanAngle;
    public float interval;
    public bool isCircle;
}
public class Boss1Controller : EnemyBase
{
    [Header("ˆÚ“®ŠÖ˜A")]
    [SerializeField] private Transform[] movePoints;
    [SerializeField] protected float MoveSpeed;
    [Header("’e–‹ŠÖ˜A")]
    [SerializeField] private GameObject bulletPrefeb;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed;

    [SerializeField] private AttackPattern[] attackPatterns;

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
