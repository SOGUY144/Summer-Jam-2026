using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HydrationSystem : MonoBehaviour
{
    // ตัวแปรพิเศษ: จะคงค่าอยู่แม้เปลี่ยน Scene แต่จะรีเซ็ตเมื่อปิดเกม/เปิดใหม่
    public static bool hasDiedOnce = false;

    [Header("Stats")]
    public float maxHP = 100f;
    public float currentHP = 20f;
    public float maxHydroric = 100f;
    public float currentHydroric;

    [Header("Rates")]
    public float heatIncreaseRate = 10f;
    public float shieldedHeatIncreaseRate = 5f;
    public float coolDownRate = 15f;
    public float overheatDamageRate = 10f;

    [Header("Items")]
    public float sodaDecreaseAmount = 50f;
    public float sodaHealAmount = 20f;

    [Header("UI References")]
    public Image hpFillImage;
    public Slider hydroricSlider;
    public Image hydroricFillImage;
    public GameObject shieldUI;
    public Animator shieldAnimator;

    [Header("Player Visuals")]
    public GameObject shieldEffectObject;

    [Header("Settings")]
    public LayerMask sunLayer;

    private PlayerController playerController;
    private Rigidbody2D rb;
    private float overheatFlashTimer = 0f;
    private float overheatFlashInterval = 0.2f;

    [Header("States")]
    public bool isInHotZone = false;
    public bool isShieldActive = false;
    public bool isOverheated = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        currentHP = maxHP;
        currentHydroric = 0f;

        // แก้ปัญหาการวาร์ป: จะวาร์ปไปจุด Safe ก็ต่อเมื่อ "เคยตายมาก่อนในรอบการเล่นนี้" เท่านั้น
        if (PlayerPrefs.HasKey("SafeX") && hasDiedOnce)
        {
            transform.position = new Vector2(PlayerPrefs.GetFloat("SafeX"), PlayerPrefs.GetFloat("SafeY"));
        }

        if (shieldEffectObject != null) shieldEffectObject.SetActive(false);
        UpdateUI();
    }

    private void Update()
    {
        if (PauseMenu.IsPaused) return;

        if (Input.GetKeyDown(KeyCode.E)) AttemptConsumeSoda();
        if (Input.GetKeyDown(KeyCode.Q)) ToggleShield();

        if (isInHotZone) UpdateHydroric();
        else CoolDown();

        if (isOverheated) ApplyOverheatDamage();

        UpdateUI();
    }

    public void TakeDamage(float damageAmount)
    {
        if (playerController != null && playerController.IsInvincible) return;

        currentHP -= damageAmount;
        if (playerController != null) playerController.TriggerDamageFlash();

        if (currentHP <= 0) Die();
    }

    private void ApplyOverheatDamage()
    {
        currentHP -= overheatDamageRate * Time.deltaTime;

        overheatFlashTimer += Time.deltaTime;
        if (overheatFlashTimer >= overheatFlashInterval)
        {
            if (playerController != null) playerController.TriggerDamageFlash();
            overheatFlashTimer = 0f;
        }

        if (currentHP <= 0) Die();
    }

    private void AttemptConsumeSoda()
    {
        if (playerController == null) return;

        if (!playerController.IsWalking && !playerController.IsJumping && !playerController.IsFalling)
        {
            playerController.TriggerDrink(2.0f);
            currentHydroric = Mathf.Max(0, currentHydroric - sodaDecreaseAmount);
            currentHP = Mathf.Min(maxHP, currentHP + sodaHealAmount);

            if (currentHydroric <= 0) isOverheated = false;
        }
    }

    private void UpdateHydroric()
    {
        float currentRate = isShieldActive ? shieldedHeatIncreaseRate : heatIncreaseRate;
        currentHydroric += currentRate * Time.deltaTime;
        currentHydroric = Mathf.Clamp(currentHydroric, 0f, maxHydroric);

        if (currentHydroric >= maxHydroric) isOverheated = true;
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

    private void UpdateUI()
    {
        if (hpFillImage != null) hpFillImage.fillAmount = currentHP / maxHP;
        if (hydroricSlider != null) hydroricSlider.value = currentHydroric / maxHydroric;
        if (hydroricFillImage != null) hydroricFillImage.fillAmount = currentHydroric / maxHydroric;
        if (shieldUI != null) shieldUI.SetActive(isShieldActive);
        if (shieldAnimator != null) shieldAnimator.SetBool("IsActive", isShieldActive);
    }

    public void ToggleShield()
    {
        isShieldActive = !isShieldActive;
        if (shieldEffectObject != null) shieldEffectObject.SetActive(isShieldActive);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // เช็คทั้ง Tag และ Layer ตามที่เพื่อนทำไว้
        if (collision.CompareTag("HotZone") || ((1 << collision.gameObject.layer) & sunLayer) != 0)
        {
            isInHotZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("HotZone") || ((1 << collision.gameObject.layer) & sunLayer) != 0)
        {
            isInHotZone = false;
        }
    }

    private void Die()
    {
        if (playerController != null) playerController.ResetMaterial();
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        // บอกระบบว่ามีการตายเกิดขึ้นแล้ว รอบหน้าให้วาร์ปได้
        hasDiedOnce = true;

        // หยุดฟิสิกส์กันร่วงทะลุแมพ
        if (rb != null) rb.simulated = false;

        GetComponent<SpriteRenderer>().enabled = false;
        if (playerController != null) playerController.enabled = false;

        yield return new WaitForSeconds(1.5f);

        // รีโหลดซีนเพื่อให้ศัตรูและวัตถุทั้งหมดรีเซ็ต
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}