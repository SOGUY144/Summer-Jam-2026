using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HydrationSystem : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 100f;
    public float currentHP;
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

    private PlayerController playerController;
    private float overheatFlashTimer = 0f;
    private float overheatFlashInterval = 0.2f;

    [Header("States")]
    public bool isInHotZone = false;
    public bool isShieldActive = false;
    public bool isOverheated = false;

    private void Start()
    {
        currentHP = maxHP;
        currentHydroric = 0f;
        playerController = GetComponent<PlayerController>();

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
        // CRITICAL: Check PlayerController for Dash I-Frames
        if (playerController != null && playerController.IsInvincible)
        {
            Debug.Log("🛡️ Dodged! No damage taken during Dash.");
            return;
        }

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

        // Ensure player is standing still on ground
        if (!playerController.IsWalking && !playerController.IsJumping && !playerController.IsFalling)
        {
            // Lock movement for 2 seconds
            playerController.TriggerDrink(2.0f);

            // Apply stats
            currentHydroric = Mathf.Max(0, currentHydroric - sodaDecreaseAmount);
            currentHP = Mathf.Min(maxHP, currentHP + sodaHealAmount);

            if (currentHydroric <= 0) isOverheated = false;
        }
        else
        {
            Debug.Log("Cannot drink while moving! Stop first.");
        }
    }

    private void UpdateHydroric()
    {
        float currentRate = isShieldActive ? shieldedHeatIncreaseRate : heatIncreaseRate;
        currentHydroric += currentRate * Time.deltaTime;
        currentHydroric = Mathf.Clamp(currentHydroric, 0f, maxHydroric);
        if (currentHydroric >= maxHydroric && !isOverheated) isOverheated = true;
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

    private void OnTriggerEnter2D(Collider2D collision) { if (collision.CompareTag("HotZone")) isInHotZone = true; }
    private void OnTriggerExit2D(Collider2D collision) { if (collision.CompareTag("HotZone")) isInHotZone = false; }

    private void Die()
    {
        if (playerController != null) playerController.ResetMaterial();
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        if (playerController != null) playerController.enabled = false;

        yield return new WaitForSeconds(1.5f);

        if (PlayerPrefs.HasKey("SafeX"))
            transform.position = new Vector2(PlayerPrefs.GetFloat("SafeX"), PlayerPrefs.GetFloat("SafeY"));

        currentHP = maxHP;
        currentHydroric = 0f;
        isOverheated = false;

        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        if (playerController != null) playerController.enabled = true;
    }
}