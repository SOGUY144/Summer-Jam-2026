using UnityEngine;
using System.Collections;

public class WalkingHeatEnemy : EnemyBase
{
    [Header("Movement")]
    public float walkSpeed = 3f;

    [Header("Melee Attack")]
    public float attackRange = 1.5f;
    public float attackCooldown = 2.0f;
    public float heatDamage = 20f;
    private float nextAttackTime;

    protected override void Start()
    {
        base.Start();
        if (rb != null) rb.gravityScale = 3f;
    }

    void Update()
    {
        // Don't do anything if dying, attacking, or player is missing
        if (isBusy || player == null)
        {
            UpdateAnimation(0);
            return;
        }

        // Stealth Check
        if (player.gameObject.layer == LayerMask.NameToLayer("StealthPlayer"))
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            UpdateAnimation(0);
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            UpdateAnimation(0);

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            float directionX = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(directionX * walkSpeed, rb.linearVelocity.y);

            // Flip sprite WITHOUT resetting scale magnitude
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x) * directionX, originalScale.y, originalScale.z);

            UpdateAnimation(rb.linearVelocity.x);
        }
    }

    private IEnumerator AttackRoutine()
    {
        isBusy = true;

        if (animator != null) animator.SetTrigger("Attack");

        // Wait for 1 second for the attack animation to reach its 'impact'
        yield return new WaitForSeconds(0.5f);
    
        // Re-check distance: Did the player move away during the 1s wind-up?
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            HydrationSystem playerHealth = player.GetComponent<HydrationSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(heatDamage);
                Debug.Log("🔥 Enemy hit player for " + heatDamage);
            }
        }

        isBusy = false;
    }

    private void UpdateAnimation(float velocityX)
    {
        if (animator != null)
        {
            // Use IsRunning parameter
            animator.SetBool("IsRunning", Mathf.Abs(velocityX) > 0.1f);
        }
    }
}