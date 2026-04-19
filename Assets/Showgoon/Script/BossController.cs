using System.Collections;
using UnityEngine;

public class BossStateMachine : MonoBehaviour
{
    public enum BossState
    {
        ChooseSkill,
        Laser,
        Smoke,
        Plasma
    }

    [Header("References")]
    public Animator bodyAnim;
    public Animator leftHandAnim;
    public Animator rightHandAnim;
    public Transform player;

    [Header("Laser")]
    public Transform laserPoint;
    public LineRenderer laser;

    [Header("Sweep Laser Setting")]
    public float laserChargeTime = 1f;
    public float laserDuration = 3f;
    public float laserDistance = 15f;
    public float sweepStartAngle = -60f;
    public float sweepEndAngle = 60f;

    [Header("Smoke")]
    public GameObject smokeObject;
    public float smokeDuration = 5f;

    [Header("Plasma")]
    public GameObject plasmaPrefab;
    public Transform leftFirePoint;
    public Transform rightFirePoint;
    public float plasmaSpeed = 10f;
    public float plasmaChargeTime = 0.35f;
    public int plasmaMin = 3;
    public int plasmaMax = 5;

    [Header("Timing")]
    public float attackInterval = 2f;

    [Header("Roaming (Area setup)")]
    [Tooltip("Assign a Collider2D (Box, Polygon, Circle) with 'Is Trigger' enabled. Boss will stay inside this shape.")]
    public Collider2D roamArea;
    public bool enableRoam = true;
    public float moveSpeed = 2f;
    public float idleBetweenMoves = 1.5f;
    public float arriveThreshold = 0.2f;

    private BossState currentState;
    private bool isAttacking;

    // Internal Roaming State
    private Vector2 roamTarget;
    private bool hasRoamTarget;

    void Start()
    {
        // Initial setup
        if (laser != null) laser.enabled = false;
        if (smokeObject != null) smokeObject.SetActive(false);

        // Safety check for roamArea
        if (roamArea != null && !roamArea.isTrigger)
        {
            Debug.LogWarning("BossStateMachine: roamArea collider should be set to 'Is Trigger' to avoid physics collisions.");
        }

        // Run logic loops
        StartCoroutine(AttackLoop());
        StartCoroutine(RoamLoop());
    }

    IEnumerator AttackLoop()
    {
        // Wait a small delay so the boss starts moving before attacking
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (!isAttacking)
            {
                ChangeState(BossState.ChooseSkill);
            }

            // Interval between the END of an attack and the NEXT skill choice
            yield return new WaitUntil(() => !isAttacking);
            yield return new WaitForSeconds(attackInterval);
        }
    }

    void ChangeState(BossState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case BossState.ChooseSkill:
                ChooseSkill();
                break;
            case BossState.Laser:
                StartCoroutine(LaserState());
                break;
            case BossState.Smoke:
                StartCoroutine(SmokeState());
                break;
            case BossState.Plasma:
                StartCoroutine(PlasmaState());
                break;
        }
    }

    void ChooseSkill()
    {
        int rand = Random.Range(0, 3);
        if (rand == 0) ChangeState(BossState.Laser);
        else if (rand == 1) ChangeState(BossState.Smoke);
        else ChangeState(BossState.Plasma);
    }

    // ================= MOVEMENT LOGIC =================

    IEnumerator RoamLoop()
    {
        while (true)
        {
            if (!enableRoam || roamArea == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // If we are currently attacking, wait until we finish
            if (isAttacking)
            {
                hasRoamTarget = false;
                yield return new WaitUntil(() => !isAttacking);
            }

            // 1. Pick a destination
            roamTarget = GetRandomPointInArea(roamArea);
            hasRoamTarget = true;

            // 2. Move to destination
            while (!isAttacking && Vector2.Distance(transform.position, roamTarget) > arriveThreshold)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    (Vector3)roamTarget,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            hasRoamTarget = false;

            // 3. Idle at the spot for a duration (unless interrupted by attack)
            float waitTimer = idleBetweenMoves;
            while (waitTimer > 0 && !isAttacking)
            {
                waitTimer -= Time.deltaTime;
                yield return null;
            }
        }
    }

    Vector2 GetRandomPointInArea(Collider2D area)
    {
        Bounds b = area.bounds;
        Vector2 randomPoint = b.center;

        // Try 20 times to find a point actually inside the trigger shape (useful for Polygons/Circles)
        for (int i = 0; i < 20; i++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float y = Random.Range(b.min.y, b.max.y);
            Vector2 potentialPoint = new Vector2(x, y);

            if (area.OverlapPoint(potentialPoint))
            {
                return potentialPoint;
            }
        }

        return randomPoint; // Fallback to center if sampling fails
    }

    // ================= SKILL COROUTINES =================

    IEnumerator LaserState()
    {
        isAttacking = true;
        if (bodyAnim) bodyAnim.SetTrigger("BossCharge");
        yield return new WaitForSeconds(laserChargeTime);

        laser.enabled = true;
        float timer = 0f;
        while (timer < laserDuration)
        {
            float t = timer / laserDuration;
            float angle = Mathf.Lerp(sweepStartAngle, sweepEndAngle, t);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            Vector3 start = laserPoint.position;
            Vector3 end = start + (Vector3)dir * laserDistance;

            laser.SetPosition(0, start);
            laser.SetPosition(1, end);

            timer += Time.deltaTime;
            yield return null;
        }

        laser.enabled = false;
        isAttacking = false;
    }

    IEnumerator SmokeState()
    {
        isAttacking = true;
        Animator hand = GetNearestHand();
        if (hand) hand.SetTrigger("Smoke_Relese");

        smokeObject.SetActive(true);
        yield return new WaitForSeconds(smokeDuration);
        smokeObject.SetActive(false);

        isAttacking = false;
    }

    IEnumerator PlasmaState()
    {
        isAttacking = true;
        int repeat = Random.Range(plasmaMin, plasmaMax + 1);

        for (int i = 0; i < repeat; i++)
        {
            Animator hand = GetNearestHand();
            yield return new WaitForSeconds(plasmaChargeTime);

            if (hand) hand.SetTrigger("Plasma");

            Transform firePoint = (hand == leftHandAnim) ? leftFirePoint : rightFirePoint;
            GameObject plasma = Instantiate(plasmaPrefab, firePoint.position, Quaternion.identity);

            Vector2 targetPos = (Vector2)player.position + new Vector2(0, 0.5f);
            Vector2 dir = (targetPos - (Vector2)firePoint.position).normalized;

            Rigidbody2D rb = plasma.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = dir * plasmaSpeed;

            yield return new WaitForSeconds(0.5f);
        }

        isAttacking = false;
    }

    Animator GetNearestHand()
    {
        if (!leftHandAnim || !rightHandAnim) return leftHandAnim ?? rightHandAnim;
        float distL = Vector2.Distance(player.position, leftHandAnim.transform.position);
        float distR = Vector2.Distance(player.position, rightHandAnim.transform.position);
        return (distL < distR) ? leftHandAnim : rightHandAnim;
    }

    // ================= DEBUGGING =================

    void OnDrawGizmosSelected()
    {
        if (roamArea != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Bounds b = roamArea.bounds;
            Gizmos.DrawCube(b.center, b.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(b.center, b.size);
        }

        if (hasRoamTarget)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, roamTarget);
            Gizmos.DrawSphere(roamTarget, 0.2f);
        }
    }
}