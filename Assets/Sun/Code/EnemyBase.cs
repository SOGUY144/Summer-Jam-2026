using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    protected float currentHealth;
    protected Rigidbody2D rb;
    protected Transform player;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    protected virtual void Die()
    {
        Debug.Log("💀 " + gameObject.name + " ถูกกำจัด!");
        Destroy(gameObject);
    }
}