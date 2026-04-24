using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour, IResettable
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    protected float currentHealth;
    protected Rigidbody2D rb;
    protected Transform player;

    [Header("Visual Effects (Flash)")]
    public Material flashMaterial;
    public float flashDuration = 0.15f;
    private Material originalMaterial;
    private Coroutine flashCoroutine;
    private string flashTextureProperty = "_MainTex";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;

    [Header("Animations")]
    public Animator animator;
    protected bool isBusy = false; // Prevents moving/attacking during animations

    protected Vector2 startPosition;
    protected Quaternion startRotation;
    protected Vector3 originalScale; // Store the initial scale from the inspector
    protected SpriteRenderer spriteRenderer;

    protected virtual void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        originalScale = transform.localScale; // Remember the size you set in the editor!
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        if (spriteRenderer != null)
            originalMaterial = spriteRenderer.sharedMaterial;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        DetectTextureProperty();
    }

    [ContextMenu("TakeDaage")]
    public virtual void TakeDamage(float damage)
    {
        if (isBusy && currentHealth <= 0) return; // Prevent double death

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        currentHealth -= damage;
        TriggerDamageFlash();

        if (currentHealth <= 0)
        {
            StartCoroutine(DieRoutine());
        }
    }

    protected virtual IEnumerator DieRoutine()
    {
        isBusy = true;
        rb.linearVelocity = Vector2.zero;

        if (animator != null) animator.SetTrigger("Dead");

        // Wait 1 second for the death animation to finish
        yield return new WaitForSeconds(0.8f);

        Destroy(gameObject);
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
            if (spriteRenderer.sprite != null)
            {
                instanceMaterial.SetTexture(flashTextureProperty, spriteRenderer.sprite.texture);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.material = originalMaterial;
        flashCoroutine = null;
    }

    private void DetectTextureProperty()
    {
        if (flashMaterial == null) return;
        if (flashMaterial.HasProperty("_MainTex")) flashTextureProperty = "_MainTex";
        else if (flashMaterial.HasProperty("_BaseMap")) flashTextureProperty = "_BaseMap";
    }

    public virtual void ResetObject()
    {
        isBusy = false;
        gameObject.SetActive(true);
        transform.position = startPosition;
        transform.rotation = startRotation;
        transform.localScale = originalScale; // Restore size
        currentHealth = maxHealth;

        if (spriteRenderer != null && originalMaterial != null)
            spriteRenderer.material = originalMaterial;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
}