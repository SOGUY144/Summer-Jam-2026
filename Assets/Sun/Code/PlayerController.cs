using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Dash & I-Frames")]
    public float dashSpeed = 20f;
    public float dashCooldown = 1.2f;
    private float nextDashTime = 0f;
    private bool isDashing = false;
    // Property used by HydrationSystem to skip damage
    public bool IsInvincible { get; private set; } = false;

    [Header("Action States")]
    private bool isDrinking = false; // Locks movement during the 2s animation

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
    public SpriteRenderer spriteRenderer;
    public GameObject DashFx;

    // Movement States for Animation/Logic
    public bool IsWalking { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsFalling { get; private set; }

    private bool facingRight = true;
    private const float verticalThreshold = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalMaterial = spriteRenderer.sharedMaterial;

        facingRight = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, 0f)) < 1f;
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;

        // If we are dashing or drinking, we ignore all other movement inputs
        if (isDashing || isDrinking) return;

        HandleMovement();
        HandleJump();
        HandleDashInput();

        Flip();
        UpdateAnimationParameters();
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        IsWalking = moveInput != 0;
    }

    private void HandleJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        float vy = rb.linearVelocity.y;
        IsJumping = !isGrounded && vy > verticalThreshold;
        IsFalling = !isGrounded && vy < -verticalThreshold;
    }

    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextDashTime)
        {
            StartCoroutine(DashRoutine());
            nextDashTime = Time.time + dashCooldown;
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        IsInvincible = true;

        float gravity = rb.gravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.gravityScale = 0;

        // Visual feedback for I-Frames
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 0.5f;
            spriteRenderer.color = c;
        }

        if (animator != null) animator.SetTrigger("Dash");

        float dashYrotation = facingRight ? 0f : -180f;
        GameObject fx = Instantiate(DashFx, transform.position, Quaternion.Euler(0f, dashYrotation, 0f));
        fx.transform.parent = transform;

        float moveInput = Input.GetAxisRaw("Horizontal");
        float dashDirection = moveInput != 0 ? moveInput : (facingRight ? 1 : -1);
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0);

        yield return new WaitForSeconds(0.2f); // Dash duration

        // Reset
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        rb.gravityScale = gravity;
        Destroy(fx);

        IsInvincible = false;
        isDashing = false;
    }

    public void TriggerDrink(float duration)
    {
        // Only trigger if Idle and Grounded
        if (!isDrinking && !IsWalking && !IsJumping && !IsFalling && isGrounded)
        {
            StartCoroutine(DrinkRoutine(duration));
        }
    }

    private IEnumerator DrinkRoutine(float duration)
    {
        isDrinking = true;
        rb.linearVelocity = Vector2.zero; // Stop instantly

        if (animator != null) animator.SetTrigger("Drink");

        yield return new WaitForSeconds(duration);

        isDrinking = false;
    }

    private void Flip()
    {
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

    private void UpdateAnimationParameters()
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
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        if (spriteRenderer != null && originalMaterial != null)
            spriteRenderer.material = originalMaterial;
    }

    private void OnDisable() => ResetMaterial();
}