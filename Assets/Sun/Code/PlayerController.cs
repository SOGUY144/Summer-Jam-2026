using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    public bool IsInvincible { get; private set; } = false;

    [Header("Action States")]
    private bool isDrinking = false;

    [Header("Visual Effects - Flash")]
    [Tooltip("A material with a solid white shader (e.g., GUI/Text Shader or a custom Flash shader)")]
    public Material flashMaterial;
    private Material originalMaterial;
    public float flashDuration = 0.15f;
    private Coroutine flashCoroutine;
    private Material instanceMaterial; // ADD THIS

    [Header("Audio")]
    public AudioSource footstepAudioSource;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip[] tileFootstepSounds;
    public AudioClip[] pipeFootstepSounds;
    public float footstepInterval = 0.35f;
    private float nextFootstepTime = 0f;

    [Header("Physics & Logic")]
    private Rigidbody2D rb;
    private bool isGrounded;
    private Collider2D currentGroundCollider;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public GameObject DashFx;

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
        {
            // Use .material (not .sharedMaterial) to get a per-instance copy
            instanceMaterial = spriteRenderer.material;
            originalMaterial = instanceMaterial;
        }

        facingRight = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, 0f)) < 1f;
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;
        if (isDashing || isDrinking) return;

        HandleMovement();
        HandleJump();
        HandleDashInput();
        HandleFootsteps();

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
        currentGroundCollider = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        isGrounded = currentGroundCollider != null;
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (footstepAudioSource != null && jumpSound != null)
            {
                footstepAudioSource.pitch = 1f;
                footstepAudioSource.PlayOneShot(jumpSound);
            }
        }

        float vy = rb.linearVelocity.y;
        IsJumping = !isGrounded && vy > verticalThreshold;
        IsFalling = !isGrounded && vy < -verticalThreshold;
    }

    private void HandleFootsteps()
    {
        if (IsWalking && isGrounded && Time.time >= nextFootstepTime)
        {
            PlayFootstepSound();
            nextFootstepTime = Time.time + footstepInterval;
        }
    }

    private void PlayFootstepSound()
    {
        if (footstepAudioSource == null) return;

        AudioClip[] currentClips = tileFootstepSounds;

        if (currentGroundCollider != null && currentGroundCollider.CompareTag("Pipe"))
        {
            currentClips = pipeFootstepSounds;
        }

        if (currentClips != null && currentClips.Length > 0)
        {
            AudioClip clip = currentClips[Random.Range(0, currentClips.Length)];
            footstepAudioSource.pitch = Random.Range(0.9f, 1.1f);
            footstepAudioSource.PlayOneShot(clip);
        }
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

        if (footstepAudioSource != null && dashSound != null)
        {
            footstepAudioSource.pitch = 1f;
            footstepAudioSource.PlayOneShot(dashSound);
        }

        float gravity = rb.gravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.gravityScale = 0;

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

        yield return new WaitForSeconds(0.2f);

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        rb.gravityScale = gravity;
        if (fx != null) Destroy(fx);

        IsInvincible = false;
        isDashing = false;
    }

    public void TriggerDrink(float duration)
    {
        if (!isDrinking && !IsWalking && !IsJumping && !IsFalling && isGrounded)
        {
            StartCoroutine(DrinkRoutine(duration));
        }
    }

    private IEnumerator DrinkRoutine(float duration)
    {
        isDrinking = true;
        rb.linearVelocity = Vector2.zero;
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

        spriteRenderer.material = flashMaterial; // instance swap, not shared

        yield return new WaitForSeconds(flashDuration);

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
        {
            spriteRenderer.material = originalMaterial; // instance, not shared
        }
    }

    private void OnDisable() => ResetMaterial();
}