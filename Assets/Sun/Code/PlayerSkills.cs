using UnityEngine;
using System.Collections;

public class PlayerSkills : MonoBehaviour
{
    [Header("Aether Skill (Button F)")]
    public GameObject platformPrefab;
    public float platformDuration = 3f;
    public float chargeTimeThreshold = 1.0f;
    public float hammerPower = -30f;
    public float hammerPreDelay = 0.05f;

    [Header("Charge Visuals")]
    public Color normalColor = Color.white;
    public Color chargingColor = new Color(1f, 0.5f, 0f);
    public Color readyColor = Color.red;

    [Header("Steam Jump (Button C)")]
    public float superJumpForce = 25f;

    [Header("Smoke Screen (Button V)")]
    public float stealthDuration = 3f;

    [Header("Animation Parameters")]
    public Animator animator;
    public string animChargeBool = "IsCharging";
    public string animAttackTrigger = "HammerAttack";
    public string animStealthTrigger = "Stealth";

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private int originalLayer;
    private float buttonPressTime;

    // Logic flags
    private bool isCharging = false;
    private bool isPerformingSmash = false;

    // The PlayerController checks this to see if it should disable movement
    public bool IsBusy => isCharging || isPerformingSmash;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        originalLayer = gameObject.layer;
    }

    void Update()
    {
        HandleAetherSkill();

        // Prevent other skills while busy
        if (IsBusy) return;

        if (Input.GetKeyDown(KeyCode.C)) SteamSuperJump();
        if (Input.GetKeyDown(KeyCode.V)) StartCoroutine(StealthRoutine());
    }

    private void HandleAetherSkill()
    {
        // Start Charging
        if (Input.GetKeyDown(KeyCode.F) && !isPerformingSmash)
        {
            buttonPressTime = Time.time;
            isCharging = true;
            if (animator) animator.SetBool(animChargeBool, true);
        }

        // While Charging
        if (isCharging)
        {
            float holdDuration = Time.time - buttonPressTime;
            if (holdDuration >= chargeTimeThreshold)
            {
                sr.color = readyColor;
            }
            else
            {
                float lerp = holdDuration / chargeTimeThreshold;
                sr.color = Color.Lerp(normalColor, chargingColor, lerp);
            }
        }

        // Release Key
        if (Input.GetKeyUp(KeyCode.F) && isCharging)
        {
            float holdDuration = Time.time - buttonPressTime;
            if (animator) animator.SetBool(animChargeBool, false);

            isCharging = false; // Stop charging state immediately

            if (holdDuration >= chargeTimeThreshold)
            {
                // Full charge -> Perform Smash (locks movement via isPerformingSmash)
                StartCoroutine(HammerSmashSequence());
            }
            else
            {
                // Partial charge -> Just create platform and reset
                CreatePlatform();
                sr.color = normalColor;
            }
        }
    }

    private IEnumerator HammerSmashSequence()
    {
        isPerformingSmash = true; // LOCK MOVEMENT during the trigger/pre-delay phase

        if (animator) animator.SetTrigger(animAttackTrigger);

        // Wait for the animation "wind up"
        yield return new WaitForSeconds(hammerPreDelay);

        // Execute physics
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, hammerPower);

        // Keep the lock for a tiny bit longer so the smash animation can play
        yield return new WaitForSeconds(0.15f);

        sr.color = normalColor;
        isPerformingSmash = false; // RESTORE MOVEMENT
    }

    private void CreatePlatform()
    {
        if (platformPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, -1.5f, 0);
            GameObject plat = Instantiate(platformPrefab, spawnPos, Quaternion.identity);
            Destroy(plat, platformDuration);
        }
    }

    private void SteamSuperJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, superJumpForce);
    }

    private IEnumerator StealthRoutine()
    {
        if (animator) animator.SetTrigger(animStealthTrigger);
        yield return new WaitForSeconds(0.5f);

        sr.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0.4f);
        gameObject.layer = LayerMask.NameToLayer("StealthPlayer");

        yield return new WaitForSeconds(stealthDuration);

        sr.color = normalColor;
        gameObject.layer = originalLayer;
    }
}