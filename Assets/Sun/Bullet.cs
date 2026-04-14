
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifetime = 2f;

    [Header("References")]
    public Rigidbody2D rb;

    void OnEnable()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
    public void Launch(Vector2 moveDirection)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        rb.linearVelocity = moveDirection * speed;

        Debug.Log("💨 3. กระสุนพุ่งแล้วจ้า! (ใช้ linearVelocity)");
        Invoke("Deactivate", lifetime);
    }

    void Deactivate()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        Debug.Log("💥 4. กระสุนชนกับ: " + hit.gameObject.name);
        if (hit.CompareTag("Ground") || hit.CompareTag("Enemy"))
        {
            CancelInvoke();
            Deactivate();
        }
    }

    void OnDisable()
    {
        CancelInvoke();
    }
}