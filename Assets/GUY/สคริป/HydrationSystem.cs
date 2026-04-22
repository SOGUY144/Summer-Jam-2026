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

    [Header("UI References")]
    public Image hpFillImage;
    public Slider hydroricSlider;
    public Image hydroricFillImage;
    public GameObject shieldUI;
    public Animator shieldAnimator;

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
        UpdateUI();
    }

    private void Update()
    {
        if (PauseMenu.IsPaused) return;

        // Inputs for items/skills
        if (Input.GetKeyDown(KeyCode.E)) ConsumeSoda();
        if (Input.GetKeyDown(KeyCode.Q)) ToggleShield();

        // Environment heat logic
        if (isInHotZone) UpdateHydroric();
        else CoolDown();

        // Damage over time if overheated
        if (isOverheated) ApplyDamage();

        UpdateUI();
    }

    public void TakeDamage(float damageAmount)
    {
        // CHECK FOR DASH I-FRAMES
        if (playerController != null && playerController.IsInvincible)
        {
            Debug.Log("🛡️ Dodged! Damage ignored during Dash.");
            return;
        }

        currentHP -= damageAmount;
        if (playerController != null) playerController.TriggerDamageFlash();

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    private void ApplyDamage()
    {
        // Environmental heat damage
        currentHP -= overheatDamageRate * Time.deltaTime;

        overheatFlashTimer += Time.deltaTime;
        if (overheatFlashTimer >= overheatFlashInterval)
        {
            if (playerController != null) playerController.TriggerDamageFlash();
            overheatFlashTimer = 0f;
        }

        if (currentHP <= 0) Die();
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

    public void ToggleShield() => isShieldActive = !isShieldActive;

    public void ConsumeSoda()
    {
        if (playerController != null)
        {
            // Only consume and trigger if the player is currently Idle (not walking, jumping, or falling)
            if (!playerController.IsWalking && !playerController.IsJumping && !playerController.IsFalling)
            {
                // Trigger the 2-second movement lock and animation
                playerController.TriggerDrink(2.0f);

                // Reduce hydration only when the action successfully starts
                currentHydroric = Mathf.Max(0, currentHydroric - 50f);
                if (currentHydroric <= 0) isOverheated = false;
            }
            else
            {
                Debug.Log("Cannot drink while moving! Stop first.");
            }
        }
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
        // Disable components for respawn
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        if (playerController != null) playerController.enabled = false;

        yield return new WaitForSeconds(1.5f);

        // Move to last save point
        if (PlayerPrefs.HasKey("SafeX"))
            transform.position = new Vector2(PlayerPrefs.GetFloat("SafeX"), PlayerPrefs.GetFloat("SafeY"));

        // Reset stats
        currentHP = maxHP;
        currentHydroric = 0f;
        isOverheated = false;

        // Re-enable components
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        if (playerController != null) playerController.enabled = true;
    }
}