using UnityEngine;
using System.Collections;

public class PlayerSkills : MonoBehaviour
{
    [Header("Aether Skill (กดปุ่ม F)")]
    public GameObject platformPrefab;
    public float platformDuration = 3f;
    public float chargeTimeThreshold = 0.7f;

    // --- เพิ่มการตั้งค่าสีตอนชาร์จ ---
    [Header("Charge Visuals")]
    public Color normalColor = Color.white;
    public Color chargingColor = new Color(1f, 0.5f, 0f); // สีส้ม
    public Color readyColor = Color.red; // สีแดงเมื่อพร้อมทุบ

    private float buttonPressTime;
    private bool isCharging = false;

    [Header("Steam Jump (กดปุ่ม C)")]
    public float superJumpForce = 25f;
    private Rigidbody2D rb;

    [Header("Smoke Screen (กดปุ่ม V)")]
    public float stealthDuration = 3f;
    private SpriteRenderer sr;
    private int originalLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        originalLayer = gameObject.layer;
    }

    void Update()
    {
        // ==========================================
        // 1. Aether Skill (สร้างแพลตฟอร์ม / ชาร์จทุบ) - ใช้ปุ่ม F
        // ==========================================
        if (Input.GetKeyDown(KeyCode.F))
        {
            buttonPressTime = Time.time;
            isCharging = true;
        }

        // --- เพิ่ม Logic ตรงนี้: ทำงานตลอดเวลาที่กดปุ่ม F ค้างไว้ ---
        if (isCharging)
        {
            float holdDuration = Time.time - buttonPressTime;

            if (holdDuration >= chargeTimeThreshold)
            {
                // ชาร์จเต็มแล้ว! เปลี่ยนเป็นสีแดงเพื่อบอกผู้เล่น
                sr.color = readyColor;
            }
            else
            {
                // กำลังชาร์จ: ค่อยๆ เปลี่ยนสีจากสีปกติ ไปเป็นสีส้ม
                float lerp = holdDuration / chargeTimeThreshold;
                sr.color = Color.Lerp(normalColor, chargingColor, lerp);
            }
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            float holdDuration = Time.time - buttonPressTime;

            if (holdDuration >= chargeTimeThreshold)
            {
                ExecuteHammerSmash();
            }
            else
            {
                CreatePlatform();
            }

            // รีเซ็ตสถานะและสีกลับเป็นปกติเมื่อปล่อยปุ่ม
            isCharging = false;
            sr.color = normalColor;
        }

        // ==========================================
        // 2. Steam Super Jump (กดปุ่ม C)
        // ==========================================
        if (Input.GetKeyDown(KeyCode.C))
        {
            SteamSuperJump();
        }

        // ==========================================
        // 3. Smoke Screen (กดปุ่ม V)
        // ==========================================
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(StealthRoutine());
        }
    }

    void CreatePlatform()
    {
        if (platformPrefab != null)
        {
            // ปรับตำแหน่งให้สร้างต่ำลงมาอีกนิด (ใต้เท้าพอดี จะได้ไม่ชนตัวละครตอนเกิด)
            Vector3 spawnPos = transform.position + new Vector3(0, -1.5f, 0);
            GameObject plat = Instantiate(platformPrefab, spawnPos, Quaternion.identity);

            Destroy(plat, platformDuration);
            Debug.Log("🧱 สร้างแพลตฟอร์ม/โล่!");
        }
    }

    void ExecuteHammerSmash()
    {
        Debug.Log("🔨 พุ่งทุบพื้น/กำแพง อย่างรุนแรง!");
        // พุ่งลงพื้นอย่างแรงด้วย linearVelocity
        rb.linearVelocity = new Vector2(0, -30f);
    }

    void SteamSuperJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, superJumpForce);
        Debug.Log("💨 พุ่งทะยานด้วยแรงดันไอน้ำ!");
    }

    IEnumerator StealthRoutine()
    {
        Debug.Log("🌫️ เริ่มพรางตัว!");
        Color originalColor = sr.color;
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.4f);
        gameObject.layer = LayerMask.NameToLayer("StealthPlayer");

        yield return new WaitForSeconds(stealthDuration);

        sr.color = normalColor; // กลับมาสีปกติที่ตั้งไว้
        gameObject.layer = originalLayer;
        Debug.Log("👁️ เลิกพรางตัว!");
    }
}