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

    void Start() { rb = GetComponent<Rigidbody2D>(); }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) StartCoroutine(Dash());
    }

    IEnumerator Dash()
    {
        float gravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(Input.GetAxisRaw("Horizontal") * dashSpeed, 0);
        yield return new WaitForSeconds(0.2f);
        rb.gravityScale = gravity;
    }
}