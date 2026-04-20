using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HydrationSystem : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 100f;
    public float currentHP;
    public float maxHydroric = 100f;
    public float currentHydroric;

    [Header("Rates (ต่อวินาที)")]
    public float heatIncreaseRate = 10f;
    public float shieldedHeatIncreaseRate = 5f;
    public float coolDownRate = 15f;
    public float overheatDamageRate = 10f;

    [Header("Items")]
    public float sodaDecreaseAmount = 50f;

    [Header("UI References")]
    public Image hpFillImage;
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
        if (Input.GetKeyDown(KeyCode.E)) ConsumeSoda();
        if (Input.GetKeyDown(KeyCode.Q)) ToggleShield();

        if (isInHotZone) UpdateHydroric();
        else CoolDown();

        if (isOverheated) ApplyDamage();

        UpdateUI();
    }

    private void UpdateHydroric()
    {
        float currentRate = isShieldActive ? shieldedHeatIncreaseRate : heatIncreaseRate;
        currentHydroric += currentRate * Time.deltaTime;
        currentHydroric = Mathf.Clamp(currentHydroric, 0f, maxHydroric);

        if (currentHydroric >= maxHydroric && !isOverheated)
        {
            isOverheated = true;
            Debug.Log("Overheated! เริ่มโดนดาเมจจากความร้อน!");
        }
    }

    private void CoolDown()
    {
        if (currentHydroric > 0)
        {
            currentHydroric -= coolDownRate * Time.deltaTime;
            currentHydroric = Mathf.Clamp(currentHydroric, 0f, maxHydroric);
        }

        if (isOverheated && currentHydroric <= 0)
        {
            isOverheated = false;
            Debug.Log("Cooled down! ความร้อนลดเหลือศูนย์ หยุดโดนดาเมจแล้ว");
        }
    }

    private void ApplyDamage()
    {
        currentHP -= overheatDamageRate * Time.deltaTime;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    private void UpdateUI()
    {
        if (hpFillImage != null) hpFillImage.fillAmount = currentHP / maxHP;
        float fillRatio = currentHydroric / maxHydroric;
        if (hydroricSlider != null) hydroricSlider.value = fillRatio;
        if (hydroricFillImage != null) hydroricFillImage.fillAmount = fillRatio;
        if (shieldUI != null) shieldUI.SetActive(isShieldActive);
        if (shieldAnimator != null) shieldAnimator.SetBool("IsActive", isShieldActive);
    }

    public void ToggleShield()
    {
        isShieldActive = !isShieldActive;
        Debug.Log("Shield toggled: " + (isShieldActive ? "เปิด" : "ปิด"));
        UpdateUI();
    }

    public void ConsumeSoda()
    {
        currentHydroric -= sodaDecreaseAmount;
        currentHydroric = Mathf.Clamp(currentHydroric, 0f, maxHydroric);
        Debug.Log("ดื่มโซดา! ลดความร้อนไป " + sodaDecreaseAmount);

        if (isOverheated && currentHydroric <= 0)
        {
            isOverheated = false;
            Debug.Log("Cooled down! โซดาลดความร้อนเหลือ 0 หยุดโดนดาเมจแล้ว");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HotZone"))
        {
            isInHotZone = true;
            Debug.Log("เข้าสู่ Hot Zone ทีมีความร้อน!");
        }
    }

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

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("💀 ผู้เล่นตายแล้ว! กำลังกลับจุดเซฟ...");
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<WeaponAim>() != null) GetComponent<WeaponAim>().enabled = false;
        if (GetComponent<PlayerController>() != null) GetComponent<PlayerController>().enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // 👈 แก้ตรงนี้: รอเวลาตายก่อน
        yield return new WaitForSeconds(1.5f);

        // 👈 แก้ตรงนี้: ย้ายผู้เล่นกลับจุดเซฟก่อนจะรีเซ็ตโลก
        if (PlayerPrefs.HasKey("SafeX"))
        {
            float x = PlayerPrefs.GetFloat("SafeX");
            float y = PlayerPrefs.GetFloat("SafeY");
            transform.position = new Vector2(x, y);
        }
        else
        {
            transform.position = new Vector2(0, 0);
        }

        // 👈 แก้ตรงนี้: ใส่คำสั่งหาตัวที่ถูกปิดไปแล้วด้วย (FindObjectsInactive.Include)
        MonoBehaviour[] allScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MonoBehaviour script in allScripts)
        {
            if (script is IResettable resettable)
            {
                resettable.ResetObject();
            }
        }

        currentHP = maxHP;
        currentHydroric = 0f;
        isOverheated = false;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        if (GetComponent<PlayerController>() != null) GetComponent<PlayerController>().enabled = true;
        if (GetComponent<WeaponAim>() != null) GetComponent<WeaponAim>().enabled = true;

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 1f;
        }

        Debug.Log("✨ คืนชีพและ Reset โลกสำเร็จ!");
    }
}