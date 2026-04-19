using UnityEngine;

public class FlyingWaterEnemy : EnemyBase
{
    [Header("Flight Settings")]
    public float flySpeed = 2f;
    public float shootingDistance = 6f;

    [Header("Ranged Attack (ปืนแรงดันน้ำ)")]
    public GameObject waterBulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float nextFireTime;

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 0f;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        if (distance > shootingDistance)
        {
            rb.linearVelocity = directionToPlayer * flySpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            if (Time.time >= nextFireTime)
            {
                ShootWaterGun(directionToPlayer);
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void ShootWaterGun(Vector2 aimDirection)
    {
        if (waterBulletPrefab != null && firePoint != null)
        {
            Debug.Log("💦 ยิงปืนแรงดันน้ำ!");
            GameObject bullet = Instantiate(waterBulletPrefab, firePoint.position, Quaternion.identity);
            bullet.GetComponent<WaterBullet>().Launch(aimDirection);
        }
    }
}