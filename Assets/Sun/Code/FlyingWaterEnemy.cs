using UnityEngine;
using System.Collections;

public class FlyingWaterEnemy : EnemyBase
{
    [Header("Flight Settings")]
    public float flySpeed = 2f;
    public float shootingDistance = 6f;

    [Header("Ranged Attack")]
    public GameObject waterBulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float nextFireTime;

    protected override void Start()
    {
        base.Start();
        // Flying enemies shouldn't fall
        if (rb != null) rb.gravityScale = 0f;
    }

    void Update()
    {
        // Stop all logic if dead or currently performing an animation (isBusy)
        if (isBusy || player == null)
        {
            UpdateAnimation(Vector2.zero);
            return;
        }

        // Stealth Check
        if (player.gameObject.layer == LayerMask.NameToLayer("StealthPlayer"))
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        // Flip logic (preserving original scale magnitude)
        if (directionToPlayer.x != 0)
        {
            float directionX = Mathf.Sign(directionToPlayer.x);
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x) * directionX, originalScale.y, originalScale.z);
        }

        if (distance > shootingDistance)
        {
            // Chase player if too far
            rb.linearVelocity = directionToPlayer * flySpeed;
            UpdateAnimation(rb.linearVelocity);
        }
        else
        {
            // Stop and shoot if in range
            rb.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);

            if (Time.time >= nextFireTime)
            {
                StartCoroutine(AttackRoutine(directionToPlayer));
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private IEnumerator AttackRoutine(Vector2 aimDirection)
    {
        isBusy = true;

        if (animator != null) animator.SetTrigger("Attack");

        // Wait 1 second for the shooting animation to reach the "fire" frame
        yield return new WaitForSeconds(1.0f);

        // Perform the actual shot
        if (waterBulletPrefab != null && firePoint != null)
        {
            Debug.Log("💦 ยิงปืนแรงดันน้ำ!");
            GameObject bullet = Instantiate(waterBulletPrefab, firePoint.position, Quaternion.identity);

            // Assuming the bullet has a Launch method
            var bulletScript = bullet.GetComponent<WaterBullet>();
            if (bulletScript != null) bulletScript.Launch(aimDirection);
        }

        isBusy = false;
    }

    private void UpdateAnimation(Vector2 velocity)
    {
        if (animator != null)
        {
            // Use IsRunning for the flying animation state
            animator.SetBool("IsRunning", velocity.magnitude > 0.1f);
        }
    }
}