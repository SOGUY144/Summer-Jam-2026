using UnityEngine;

public class WalkingHeatEnemy : EnemyBase
{
    [Header("Movement")]
    public float walkSpeed = 3f;

    [Header("Melee Attack (สนับมือความร้อน)")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float heatDamage = 20f;
    private float nextAttackTime;

    protected override void Start()
    {
        base.Start();
        rb.gravityScale = 3f;
    }

    void Update()
    {
        if (player == null || player.gameObject.layer == LayerMask.NameToLayer("StealthPlayer"))
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // หยุดเดิน
            return; // 👈 ข้ามคำสั่งตามล่าด้านล่างไปเลย
        }
        if (player == null) return;
        
            float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (Time.time >= nextAttackTime)
            {
                MeleeAttack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            float directionX = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(directionX * walkSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(directionX, 1, 1);
        }
    }

    void MeleeAttack()
    {
        Debug.Log("🔥 ต่อยด้วยสนับมือความร้อน! ทำดาเมจ: " + heatDamage);

        // ดึงสคริปต์เลือดของ Player มาใช้
        HydrationSystem playerHealth = player.GetComponent<HydrationSystem>();

        if (playerHealth != null)
        {
            // ทำดาเมจเข้า HP โดยตรง (เดี๋ยวเราไปสร้างฟังก์ชัน TakeDamage ให้เพื่อนในข้อ 2)
            playerHealth.TakeDamage(heatDamage);

            // [ออปชันเสริม] ถ้าอยากให้โดนต่อยแล้ว "ความร้อนขึ้น" ด้วย ให้เปิดคอมเมนต์บรรทัดล่างครับ
            // playerHealth.currentHydroric += 15f; 
        }
    }
}