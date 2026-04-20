using UnityEngine;

public class PlasmaBall : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // ทำดาเมจให้ผู้เล่นที่ชนกับ Plasma Ball
            HydrationSystem playerHealth = collision.GetComponent<HydrationSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10); // ปรับค่าดาเมจตามต้องการ
            }
            // ทำลาย Plasma Ball หลังจากชน
            Destroy(gameObject);
        }
    }
}
