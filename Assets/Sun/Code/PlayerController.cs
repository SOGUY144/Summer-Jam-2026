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
    public GameObject DashFx;
    public SpriteRenderer spriteRenderer;

    // Tracks current facing direction: true = facing right, false = facing left
    private bool facingRight = true;


    void Start() 
    {
        rb = GetComponent<Rigidbody2D>(); 
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize facing direction from current rotation (tolerant to small floating errors)
        facingRight = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, 0f)) < 1f;
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        IsWalking = moveInput != 0;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.SetTrigger("Dash");
            StartCoroutine(Dash());

        }
        Flip();
        UpdateAnimation();
    }

    private void Flip() // Flip by Rotation
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0f && !facingRight)
        {
            // Face right
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            facingRight = true;
        }
        else if (moveInput < 0f && facingRight)
        {
            // Face left (rotate 180 degrees on Y)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            facingRight = false;
        }
    }

    public void UpdateAnimation()
    {
        animator.SetBool("IsWalking", IsWalking);
    }
    IEnumerator Dash()
    {
        float gravity = rb.gravityScale;
        rb.gravityScale = 0;

        // Ensure DashFx rotation Y is 0 when facing right, -180 when facing left
        float dashYrotation = facingRight ? 0f : -180f;
        GameObject newObject = Instantiate(DashFx, transform.position, Quaternion.Euler(0f, dashYrotation, 0f));
        newObject.transform.parent = transform;
        rb.linearVelocity = new Vector2(Input.GetAxisRaw("Horizontal") * dashSpeed, 0);
        yield return new WaitForSeconds(0.2f);
        rb.gravityScale = gravity;
        Destroy(newObject);
    }
}