using UnityEngine;

public class WaterBullet : MonoBehaviour
{
    public float speed = 10f;
    public float waterDamage = 15f;

    public void Launch(Vector2 moveDirection)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = moveDirection * speed;

        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D hit)
    {
        if (hit.CompareTag("Player"))
        {
            HydrationSystem playerHealth = hit.GetComponent<HydrationSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(waterDamage);
            }
            Destroy(gameObject);
        }
    }
}