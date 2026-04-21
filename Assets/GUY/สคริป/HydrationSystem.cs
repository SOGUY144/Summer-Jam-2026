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

    [Header("Items & Shield Logic")]
    public float sodaDecreaseAmount = 50f;
    public float sodaHealAmount = 30f;
    public float sodaCooldown = 2f;
    private float nextSodaTime = 0f;
    [Range(0f, 1f)]
    public float shieldDamageReduction = 0.5f;

    [Header("UI References")]
    public Image hpFillImage;
    public Slider hydroricSlider;
    public Image hydroricFillImage;
    public GameObject shieldUI; // อันนี้คือรูปโล่บนหน้าจอ UI
    public Animator shieldAnimator;

    [Header("Player & Effects")]
    public Animator playerAnimator;
    public GameObject shieldEffectObject; // 👈 1. เพิ่มช่องนี้: สำหรับลากวัตถุโล่ (ลูกของ Player) มาใส่

    [Header("States (Read Only)")]
    public bool isInHotZone = false;
    public bool isShieldActive = false;
    public bool isOverheated = false;

    private void Start()
    {
        currentHP = maxHP;
        currentHydroric = 0f;
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();

        // 👈 2. ปิดโล่ไว้ก่อนตอนเริ่มเกมเพื่อความชัวร์
        if (shieldEffectObject != null) shieldEffectObject.SetActive(false);

        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && Time.time >= nextSodaTime)
        {
            ConsumeSoda();
            nextSodaTime = Time.time + sodaCooldown;
        }

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
            Debug.Log("Overheated!");
        }
    }

    private void CoolDown()
    {
        if (currentHydroric > 0)
        {
            currentHydroric -= coolDownRate * Time.deltaTime;
            currentHydroric = Mathf.Clamp(currentHydroric, 0f, maxHydroric);
        }
        if (isOverheated && currentHydroric <= 0) isOverheated = false;
    }

    private void ApplyDamage()
    {
        currentHP -= overheatDamageRate * Time.deltaTime;
        if (currentHP <= 0) { currentHP = 0; Die(); }
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

    // 👈 3. แก้ฟังก์ชัน ToggleShield ให้เปิด/ปิดวัตถุโล่แทนการเปลี่ยนท่าแอนิเมชันตัวละคร
    public void ToggleShield()
    {
        isShieldActive = !isShieldActive;

        if (shieldEffectObject != null)
        {
            shieldEffectObject.SetActive(isShieldActive); // เปิด/ปิด ตัวลูกที่เป็นโล่
        }

        // ถ้าอยากให้ตัวละครมีท่ากางโล่นิดๆ ด้วยก็ค้างบรรทัดนี้ไว้ได้ (แต่ถ้าไม่อยากให้ตัวละครหาย ให้เอาออก)
        // if (playerAnimator != null) playerAnimator.SetBool("IsShielding", isShieldActive);

        UpdateUI();
    }

    public void ConsumeSoda()
    {
        if (playerAnimator != null) playerAnimator.SetTrigger("DrinkSoda");

        currentHydroric -= sodaDecreaseAmount;
        currentHydroric = Mathf.Clamp(currentHydroric, 0f, maxHydroric);
        currentHP += sodaHealAmount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        if (isOverheated && currentHydroric <= 0) isOverheated = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HotZone")) isInHotZone = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("HotZone")) isInHotZone = false;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isShieldActive)
        {
            damageAmount *= shieldDamageReduction;
            Debug.Log("โล่ลดดาเมจ!");
        }

        currentHP -= damageAmount;
        if (currentHP <= 0) { currentHP = 0; Die(); }
    }

    private void Die()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // 👈 4. ปิดโล่ทันทีเมื่อตาย
        if (shieldEffectObject != null) shieldEffectObject.SetActive(false);

        if (GetComponent<PlayerController>() != null) GetComponent<PlayerController>().enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        yield return new WaitForSeconds(1.5f);

        if (PlayerPrefs.HasKey("SafeX"))
        {
            transform.position = new Vector2(PlayerPrefs.GetFloat("SafeX"), PlayerPrefs.GetFloat("SafeY"));
        }

        MonoBehaviour[] allScripts = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MonoBehaviour script in allScripts)
        {
            if (script is IResettable resettable) resettable.ResetObject();
        }

        currentHP = maxHP;
        currentHydroric = 0f;
        isOverheated = false;
        isShieldActive = false;

        yield return new WaitForEndOfFrame();

        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        if (GetComponent<PlayerController>() != null) GetComponent<PlayerController>().enabled = true;

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 1f;
        }
    }
}