using UnityEngine;
using UnityEngine.UI;

public class HydrationSystem : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 100f;
    public float currentHP;

    public float maxHydroric = 100f; // ความร้อนสูงสุด
    public float currentHydroric;

    [Header("Rates (ต่อวินาที)")]
    public float heatIncreaseRate = 10f; // อัตราส่วนเพิ่มความร้อนปกติ
    public float shieldedHeatIncreaseRate = 5f; // อัตราส่วนเพิ่มความร้อนตอนเปิดโล่
    public float coolDownRate = 15f; // อัตราส่วนลดความร้อนเมื่ออยู่นอกพื้นที่
    public float overheatDamageRate = 10f; // ดาเมจต่อวินาทีเมื่อความร้อนเต็ม

    [Header("Items")]
    public float sodaDecreaseAmount = 50f; // ลดความร้อนทันทีที่ดื่มโซดา

    [Header("UI References")]
    [Tooltip("ลาก Slider หรือ Image มาใส่ (สามารถใช้ตัวใดตัวหนึ่ง หรือทั้งสองตัวก็ได้)")]
    public Slider hydroricSlider;
    public Image hydroricFillImage;
    [Tooltip("ใส่ GameObject เพื่อเปิด-ปิดโล่แบบธรรมดา (ใช้แบบเปิดปิดภาพ)")]
    public GameObject shieldUI;
    [Tooltip("ใส่ Animator เพื่อเล่นแอนิเมชันโล่ (ตั้งพารามิเตอร์ชื่อ IsActive เป็น bool)")]
    public Animator shieldAnimator;

    [Header("States (Read Only)")]
    public bool isInHotZone = false;
    public bool isShieldActive = false;
    public bool isOverheated = false;

    private void Start()
    {
        currentHP = maxHP;
        currentHydroric = 0f;
        UpdateUI();
    }

    private void Update()
    {
        // รับค่าปุ่มคีย์บอร์ด (ตัวอย่าง: กด E ดื่มโซดา / กด Q เปิดปิดโล่)
        if (Input.GetKeyDown(KeyCode.E))
        {
            ConsumeSoda();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleShield();
        }

        // อัปเดตความร้อนตามพื้นที่
        if (isInHotZone)
        {
            UpdateHydroric();
        }
        else
        {
            CoolDown();
        }

        // โดนดาเมจถ้าอยู่ในสถานะ Overheat
        if (isOverheated)
        {
            ApplyDamage();
        }

        // อัปเดต UI ทุกเฟรมเพื่อให้การแสดงผลลื่นไหล
        UpdateUI();
    }

    // ฟังก์ชันเพิ่มความร้อนเมื่ออยู่ใน Hot Zone
    private void UpdateHydroric()
    {
        // เช็คว่าเปิดโล่อยู่หรือไม่ เพื่อคำนวณ rate
        float currentRate = isShieldActive ? shieldedHeatIncreaseRate : heatIncreaseRate;

        currentHydroric += currentRate * Time.deltaTime;
        currentHydroric = Mathf.Clamp(currentHydroric, 0f, maxHydroric);

        // ถ้าความร้อนเต็ม จะเข้าสู่สถานะ Overheated
        if (currentHydroric >= maxHydroric && !isOverheated)
        {
            isOverheated = true;
            Debug.Log("Overheated! เริ่มโดนดาเมจจากความร้อน!");
        }
    }

    // ฟังก์ชันลดความร้อนเมื่ออยู่นอก Hot Zone
    private void CoolDown()
    {
        if (currentHydroric > 0)
        {
            currentHydroric -= coolDownRate * Time.deltaTime;
            currentHydroric = Mathf.Clamp(currentHydroric, 0f, maxHydroric);
        }

        // ผู้เล่นจะหลุดจากสถานะ Overheated ก็ต่อเมื่อความร้อนลดลงเหลือ 0 เท่านั้น
        if (isOverheated && currentHydroric <= 0)
        {
            isOverheated = false;
            Debug.Log("Cooled down! ความร้อนลดเหลือศูนย์ หยุดโดนดาเมจแล้ว");
        }
    }

    // ฟังก์ชันสุ่มดาเมจเมื่อ Overheat
    private void ApplyDamage()
    {
        currentHP -= overheatDamageRate * Time.deltaTime;

        // 👇 เพิ่มเช็คตรงนี้ด้วยครับ ไม่งั้นตายเพราะความร้อนจะไม่หายไป
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    // ฟังก์ชันอัปเดตแถบ UI และโชว์โล่
    private void UpdateUI()
    {
        float fillRatio = currentHydroric / maxHydroric;

        if (hydroricSlider != null)
        {
            // ทำงานได้ดีถ้าตั้งค่า Slider จาก 0 ถึง 1 ใน Editor หรือจะใช้ value ตรงๆ ก็ได้
            hydroricSlider.value = fillRatio;
        }

        if (hydroricFillImage != null)
        {
            // รองรับ Image แบบ Filled
            hydroricFillImage.fillAmount = fillRatio;
        }

        if (shieldUI != null)
        {
            // เปิด-ปิด UI ของโล่แบบซ่อน/แสดง
            shieldUI.SetActive(isShieldActive);
        }

        if (shieldAnimator != null)
        {
            // ส่งค่า isShieldActive ไปให้ Animator Play แอนิเมชัน
            shieldAnimator.SetBool("IsActive", isShieldActive);
        }
    }

    // เปิด-ปิด โล่
    public void ToggleShield()
    {
        isShieldActive = !isShieldActive;
        Debug.Log("Shield toggled: " + (isShieldActive ? "เปิด" : "ปิด"));
        UpdateUI();
    }

    // ดื่มโซดาเพื่อลดความร้อนทันที
    public void ConsumeSoda()
    {
        currentHydroric -= sodaDecreaseAmount;
        currentHydroric = Mathf.Clamp(currentHydroric, 0f, maxHydroric);
        Debug.Log("ดื่มโซดา! ลดความร้อนไป " + sodaDecreaseAmount);

        // ตรวจสอบเผื่อว่าโซดาลดความร้อนจนถึง 0 ให้หลุดจากสถานะ Overheat เลย
        if (isOverheated && currentHydroric <= 0)
        {
            isOverheated = false;
            Debug.Log("Cooled down! โซดาลดความร้อนเหลือ 0 หยุดโดนดาเมจแล้ว");
        }
    }

    // ตรวจจับเมื่อผู้เล่นเดินเข้า Hot Zone
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HotZone"))
        {
            isInHotZone = true;
            Debug.Log("เข้าสู่ Hot Zone ทีมีความร้อน!");
        }
    }

    // ตรวจจับเมื่อผู้เล่นเดินออก Hot Zone
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("HotZone"))
        {
            isInHotZone = false;
            Debug.Log("ออกจาก Hot Zone แล้ว!");
        }
    }
    public void TakeDamage(float damageAmount)
    {
        currentHP -= damageAmount;
        Debug.Log("💥 ผู้เล่นโดนโจมตี! เสียเลือด: " + damageAmount + " | เลือดเหลือ: " + currentHP);

        // เช็คการตาย
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }
    private void Die()
    {
        if (!gameObject.activeSelf) return; // ถ้าหายไปแล้วไม่ต้องรันซ้ำ

        Debug.Log("💀 ผู้เล่นตายแล้ว!");

        // ปิดการมองเห็น
        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().enabled = false;

        // ปิดการชน (กันศัตรูมาชนซ้ำ)
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;

        // หรือจะใช้คำสั่งเดิมที่ซันต้องการ (หายไปทั้ง Object)
        gameObject.SetActive(false);
    }
}