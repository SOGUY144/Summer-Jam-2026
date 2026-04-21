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
        spriteRenderer = GetComponent<SpriteRenderer>();

        facingRight = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, 0f)) < 1f;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
        }
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return; // หยุดรับ Input ทั้งหมดตอน Pause
        if (isDashing) return;

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

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded && !isBusy)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        float vy = rb.linearVelocity.y;
        IsJumping = !isGrounded && vy > verticalThreshold;
        IsFalling = !isGrounded && vy < -verticalThreshold;

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextDashTime && !isBusy)
        {
            if (animator != null) animator.SetTrigger("Dash");
            StartCoroutine(Dash());
            nextDashTime = Time.time + dashCooldown;
        }

        Flip();
        UpdateAnimation();
    }

    private void Flip()
    {
        if (PauseMenu.IsPaused) return; // หยุดหันซ้ายขวาตอน Pause

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

    IEnumerator Dash()
    {
        isDashing = true;
        float gravity = rb.gravityScale;
        rb.gravityScale = 0;

        float dashYrotation = facingRight ? 0f : -180f;
        GameObject newObject = Instantiate(DashFx, transform.position, Quaternion.Euler(0f, dashYrotation, 0f));
        newObject.transform.parent = transform;

        float moveInput = Input.GetAxisRaw("Horizontal");
        float dashDirection = moveInput != 0 ? moveInput : (facingRight ? 1 : -1);

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0);

        yield return new WaitForSeconds(0.2f);

        rb.gravityScale = gravity;
        Destroy(newObject);
        isDashing = false;
    }

    public void LoadLastSavePoint()
    {
        if (PlayerPrefs.HasKey("SafeX"))
        {
            float x = PlayerPrefs.GetFloat("SafeX");
            float y = PlayerPrefs.GetFloat("SafeY");
            transform.position = new Vector2(x, y);
        }
    }
}