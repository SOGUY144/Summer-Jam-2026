using UnityEngine;
using System.Collections;

public class PlayerSkills : MonoBehaviour
{
    [Header("Aether Skill (Button F)")]
    public GameObject platformPrefab;
    public float platformDuration = 3f;
    public float chargeTimeThreshold = 0.5f;
    public float hammerPower = -30f;
    public float hammerPreDelay = 0.05f;
    public float aetherCooldown = 2f;
    private float nextAetherTime = 0f;

    [Header("Charge Visuals")]
    public Color normalColor = Color.white;
    public Color chargingColor = new Color(1f, 0.5f, 0f);
    public Color readyColor = Color.red;

    [Header("Steam Jump (Button C)")]
    public float superJumpForce = 25f;
    public float jumpCooldown = 3f;
    private float nextJumpTime = 0f;

    [Header("Smoke Screen (Button V)")]
    public float stealthDuration = 3f;
    public float stealthCooldown = 5f;
    private float nextStealthTime = 0f;

    [Header("Animation Parameters")]
    public Animator animator;
    public string animChargeBool = "IsCharging";
    public string animAttackTrigger = "HammerAttack";
    public string animStealthTrigger = "Stealth";

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private int originalLayer;
    private float buttonPressTime;

    private bool isCharging = false;
    private bool isPerformingSmash = false;

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

        if (IsBusy) return;

        if (Input.GetKeyDown(KeyCode.C) && Time.time >= nextJumpTime)
        {
            SteamSuperJump();
            nextJumpTime = Time.time + jumpCooldown;
        }

        if (Input.GetKeyDown(KeyCode.V) && Time.time >= nextStealthTime)
        {
            StartCoroutine(StealthRoutine());
            nextStealthTime = Time.time + stealthCooldown;
        }
    }

    private void HandleAetherSkill()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isPerformingSmash && Time.time >= nextAetherTime)
        {
            buttonPressTime = Time.time;
            isCharging = true;
            if (animator) animator.SetBool(animChargeBool, true);
        }

        if (isCharging)
        {
            // ล็อคความเร็วแกน X เป็น 0 ขณะชาร์จ
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

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

        if (Input.GetKeyUp(KeyCode.F) && isCharging)
        {
            float holdDuration = Time.time - buttonPressTime;
            if (animator) animator.SetBool(animChargeBool, false);

            isCharging = false;

            if (holdDuration >= chargeTimeThreshold)
            {
                StartCoroutine(HammerSmashSequence());
                nextAetherTime = Time.time + aetherCooldown;
            }
            else
            {
                CreatePlatform();
                sr.color = normalColor;
                nextAetherTime = Time.time + aetherCooldown;
            }
        }
    }

    private IEnumerator HammerSmashSequence()
    {
        isPerformingSmash = true;
        if (animator) animator.SetTrigger(animAttackTrigger);

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        }

        yield return new WaitForSeconds(hammerPreDelay);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, hammerPower);

        float damage = 500f;
        float hitRadius = 2.5f;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, hitRadius);
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(0.2f);

        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }

        sr.color = normalColor;
        isPerformingSmash = false;
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
        gameObject.layer = LayerMask.NameToLayer("StealthPlayer");
        yield return new WaitForSeconds(0.5f);
        sr.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0.4f);
        yield return new WaitForSeconds(stealthDuration);
        sr.color = normalColor;
        gameObject.layer = originalLayer;
    }
}