using UnityEngine;

// เติม IResettable เข้าไปตรงนี้
public abstract class EnemyBase : MonoBehaviour, IResettable
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    protected float currentHealth;
    protected Rigidbody2D rb;
    protected Transform player;

    // 👇 ตัวแปรสำหรับจำค่าเริ่มต้น
    protected Vector2 startPosition;
    protected Quaternion startRotation;

    protected virtual void Start()
    {
        // 1. จำตำแหน่งและค่าต่างๆ ไว้ตั้งแต่เริ่มเกม
        startPosition = transform.position;
        startRotation = transform.rotation;
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 👇 เปลี่ยนจาก Destroy เป็นแค่ปิดตัวตนไว้ก่อน เพื่อให้ Reset ได้
    protected virtual void Die()
    {
        Debug.Log("💀 " + gameObject.name + " ถูกกำจัด!");
        gameObject.SetActive(false);
    }

    // 👇 ฟังก์ชัน Reset ที่จะถูกเรียกจาก Player
    public virtual void ResetObject()
    {
        gameObject.SetActive(true); // ปลุกให้ตื่น
        transform.position = startPosition; // วาร์ปกลับที่เดิม
        transform.rotation = startRotation;
        currentHealth = maxHealth; // เลือดเต็ม

        if (rb != null) rb.linearVelocity = Vector2.zero; // หยุดความเร็วค้าง
        Debug.Log(gameObject.name + " Reset เรียบร้อย!");
    }
}