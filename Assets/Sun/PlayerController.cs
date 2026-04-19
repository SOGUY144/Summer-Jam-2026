using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float dashSpeed = 20f;
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
    private PlayerSkills playerSkills;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerSkills = GetComponent<PlayerSkills>();

        facingRight = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, 0f)) < 1f;
    }

    void Update()
    {
        // USE IsBusy flag which includes both Charging and the Smash animation window
        if (playerSkills != null && playerSkills.IsBusy)
        {
            StopMovementDuringAction();
            return;
        }

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        IsWalking = moveInput != 0;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        float vy = rb.linearVelocity.y;
        IsJumping = !isGrounded && vy > 0.1f;
        IsFalling = !isGrounded && vy < -0.1f;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.SetTrigger("Dash");
            StartCoroutine(Dash());
        }

        Flip();
        UpdateAnimation();
    }

    private void StopMovementDuringAction()
    {
        // Hold current X velocity (0) but keep falling velocity for the smash to work
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        IsWalking = false;

        // We do NOT call UpdateAnimation() here because we want the 
        // Skills script to control the animation during this time.
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
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

    public void UpdateAnimation()
    {
        if (animator == null) return;
        animator.SetBool("IsWalking", IsWalking);
        animator.SetBool("IsJumping", IsJumping);
        animator.SetBool("IsFalling", IsFalling);
    }

    IEnumerator Dash()
    {
        float gravity = rb.gravityScale;
        rb.gravityScale = 0;
        float dashYrotation = facingRight ? 0f : -180f;
        GameObject newObject = Instantiate(DashFx, transform.position, Quaternion.Euler(0f, dashYrotation, 0f));
        newObject.transform.parent = transform;
        float dashDir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0);
        yield return new WaitForSeconds(0.2f);
        rb.gravityScale = gravity;
        Destroy(newObject);
    }
}