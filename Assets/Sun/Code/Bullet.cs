using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifetime = 2f;
    public float damage = 20f; // กำหนดดาเมจได้จากหน้า Inspector

    [Header("References")]
    public Rigidbody2D rb;
    private bool hasHit = false; // ตัวป้องกันไม่ให้กระสุนทำงานซ้ำ (กันทะลุ)

    void OnEnable()
    {
        hasHit = false; // รีเซ็ตสถานะทุกครั้งที่ถูกดึงมาใช้ใหม่ (Pool)
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void Launch(Vector2 moveDirection)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        rb.linearVelocity = moveDirection * speed;
        Invoke("Deactivate", lifetime);
    }

    void Deactivate()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        // ถ้ากระสุนเคยชนไปแล้วในเฟรมนี้ ให้หยุดทำงานทันที
        if (hasHit) return;

        if (hit.CompareTag("Enemy"))
        {
            hasHit = true; // ล็อกว่าชนแล้วนะ

            // 1. ส่งดาเมจไปที่ศัตรู
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // 2. ทำลายกระสุนทันที (ปิดการใช้งาน)
            CancelInvoke();
            Deactivate();
            Debug.Log("🎯 กระสุนโดนศัตรูและหายไปแล้ว");
        }
        else if (hit.CompareTag("Ground"))
        {
            hasHit = true;
            CancelInvoke();
            Deactivate();
        }
    }

    void OnDisable()
    {
        CancelInvoke();
    }
}