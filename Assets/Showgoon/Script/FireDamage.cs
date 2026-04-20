using UnityEngine;

public class FireDamage : MonoBehaviour
{
    [Header("Settings")]
    public float damageAmount = 10f;
    public float damageInterval = 0.5f; // ทำดาเมจทุกๆ 0.5 วินาที

    private float nextDamageTime;

    private void OnTriggerStay2D(Collider2D collision)
    {
        // ตรวจสอบว่าสิ่งที่อยู่ในไฟคือ Player หรือไม่
        if (collision.CompareTag("Player"))
        {
            // ตรวจสอบว่าถึงเวลาทำดาเมจรอบถัดไปหรือยัง
            if (Time.time >= nextDamageTime)
            {
                // เรียกใช้ฟังก์ชัน TakeDamage จาก script HydrationSystem ที่ติดอยู่กับ Player
                HydrationSystem playerHealth = collision.GetComponent<HydrationSystem>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount);

                    // ตั้งเวลาสำหรับการโดนดาเมจครั้งต่อไป
                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }
    }
}