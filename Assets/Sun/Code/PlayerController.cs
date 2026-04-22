using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashCooldown = 1.2f;
    private float nextDashTime = 0f;
    private bool isDashing = false;
    private bool isDrinking = false; // New state to lock movement

    // This property allows HydrationSystem to check for I-frames
    public bool IsInvincible { get; private set; } = false;

    [Header("Visual Effects - Flash")]
    public Material flashMaterial;
    private Material originalMaterial;
    public float flashDuration = 0.15f;
    private Coroutine flashCoroutine;
    public string flashTextureProperty = "_MainTex";

    [Header("Physics & Logic")]
    private Rigidbody2D rb;
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Animator animator;
    public bool IsWalking = false;
    public bool IsJumping = false;
    public bool IsFalling = false;
    public GameObject DashFx;
    public SpriteRenderer spriteRenderer;

    private bool facingRight = true;
    private const float verticalThreshold = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalMaterial = spriteRenderer.sharedMaterial;

        // Check initial facing direction based on rotation
        facingRight = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, 0f)) < 1f;
    }

    void Update()
    {
        // Basic checks for Pause, Dash, and Drinking state
        if (PauseMenu.IsPaused) return;
        if (isDashing || isDrinking) return; // Lock input if drinking

        // Integration with PlayerSkills (if exists)
        PlayerSkills skills = GetComponent<PlayerSkills>();
        bool isBusy = (skills != null && skills.IsBusy);

        float moveInput = Input.GetAxisRaw("Horizontal");

        if (isBusy)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            IsWalking = false;
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            IsWalking = moveInput != 0;
        }

        // Ground check for jumping
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded && !isBusy)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        // State detection for animations
        float vy = rb.linearVelocity.y;
        IsJumping = !isGrounded && vy > verticalThreshold;
        IsFalling = !isGrounded && vy < -verticalThreshold;

        // Dash Input
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextDashTime && !isBusy)
        {
            if (animator != null) animator.SetTrigger("Dash");
            StartCoroutine(Dash());
            nextDashTime = Time.time + dashCooldown;
        }

        Flip();
        UpdateAnimation();
    }

    IEnumerator Dash()
    {
        isDashing = true;
        IsInvincible = true; // Start I-Frames

        float gravity = rb.gravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.gravityScale = 0;

        // Visual feedback for I-Frames: transparency
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 0.5f;
            spriteRenderer.color = c;
        }

        float dashYrotation = facingRight ? 0f : -180f;
        GameObject fx = Instantiate(DashFx, transform.position, Quaternion.Euler(0f, dashYrotation, 0f));
        fx.transform.parent = transform;

        float moveInput = Input.GetAxisRaw("Horizontal");
        float dashDirection = moveInput != 0 ? moveInput : (facingRight ? 1 : -1);

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0);

        yield return new WaitForSeconds(0.2f); // Dash duration

        // Reset state and visuals
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        rb.gravityScale = gravity;
        Destroy(fx);

        IsInvincible = false; // End I-Frames
        isDashing = false;
    }

    private void Flip()
    {
        if (PauseMenu.IsPaused) return;
        float moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput > 0f && !facingRight)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            facingRight = true;
        }
        else if (moveInput < 0f && facingRight)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            facingRight = false;
        }
    }

    public void UpdateAnimation()
    {
        if (animator == null) return;
        animator.SetBool("IsWalking", IsWalking);
        animator.SetBool("IsJumping", IsJumping);
        animator.SetBool("IsFalling", IsFalling);
    }

    public void TriggerDamageFlash()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer == null || flashMaterial == null) yield break;
        spriteRenderer.material = flashMaterial;
        Material instanceMaterial = spriteRenderer.material;

        float timer = 0f;
        while (timer < flashDuration)
        {
            if (spriteRenderer.sprite != null && spriteRenderer.sprite.texture != null)
            {
                string propName = string.IsNullOrEmpty(flashTextureProperty) ? "_MainTex" : flashTextureProperty;
                instanceMaterial.SetTexture(propName, spriteRenderer.sprite.texture);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        ResetMaterial();
    }

    public void ResetMaterial()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        if (spriteRenderer != null && originalMaterial != null)
            spriteRenderer.material = originalMaterial;
    }

    private void OnDisable() => ResetMaterial();

    public void TriggerDrink(float duration)
    {
        // Only allow drinking if not already drinking, and not moving/in air
        if (!isDrinking && !IsWalking && !IsJumping && !IsFalling)
        {
            StartCoroutine(DrinkRoutine(duration));
        }
    }

    private IEnumerator DrinkRoutine(float duration)
    {
        isDrinking = true;

        // Stop movement immediately
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        IsWalking = false;
        UpdateAnimation();

        if (animator != null) animator.SetTrigger("Drink");

        yield return new WaitForSeconds(duration);

        isDrinking = false;
    }
}