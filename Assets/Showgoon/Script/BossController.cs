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

    [Header("Laser Settings")]
    public Transform laserPoint;
    public LineRenderer laser;
    public float laserDamageAmount = 30f;
    public float laserDamageCooldown = 1.0f;
    public float laserThickness = 0.5f; // เพิ่มความกว้างขึ้นเล็กน้อยเพื่อให้โดนง่ายขึ้น
    public LayerMask playerLayer; // ตั้งค่าให้ตรวจจับเฉพาะ Layer ของ Player

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
    public Collider2D roamArea;
    public bool enableRoam = true;
    public float moveSpeed = 2f;
    public float idleBetweenMoves = 1.5f;
    public float arriveThreshold = 0.2f;

    private BossState currentState;
    private bool isAttacking;
    private float nextLaserDamageTime;

    // Internal Roaming State
    private Vector2 roamTarget;
    private bool hasRoamTarget;

    void Start()
    {
        if (laser != null) laser.enabled = false;
        if (smokeObject != null) smokeObject.SetActive(false);

        if (roamArea != null && !roamArea.isTrigger)
        {
            Debug.LogWarning("BossStateMachine: roamArea collider should be set to 'Is Trigger'.");
        }

        StartCoroutine(AttackLoop());
        StartCoroutine(RoamLoop());
    }

    IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(1f);
        while (true)
        {
            if (!isAttacking)
            {
                ChangeState(BossState.ChooseSkill);
            }
            yield return new WaitUntil(() => !isAttacking);
            yield return new WaitForSeconds(attackInterval);
        }
    }

    void ChangeState(BossState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case BossState.ChooseSkill: ChooseSkill(); break;
            case BossState.Laser: StartCoroutine(LaserState()); break;
            case BossState.Smoke: StartCoroutine(SmokeState()); break;
            case BossState.Plasma: StartCoroutine(PlasmaState()); break;
        }
    }

    void ChooseSkill()
    {
        int rand = Random.Range(0, 3);
        if (rand == 0) ChangeState(BossState.Laser);
        else if (rand == 1) ChangeState(BossState.Smoke);
        else ChangeState(BossState.Plasma);
    }

    IEnumerator RoamLoop()
    {
        while (true)
        {
            if (!enableRoam || roamArea == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            if (isAttacking)
            {
                hasRoamTarget = false;
                yield return new WaitUntil(() => !isAttacking);
            }

            roamTarget = GetRandomPointInArea(roamArea);
            hasRoamTarget = true;

            while (!isAttacking && Vector2.Distance(transform.position, roamTarget) > arriveThreshold)
            {
                transform.position = Vector3.MoveTowards(transform.position, (Vector3)roamTarget, moveSpeed * Time.deltaTime);
                yield return null;
            }

            hasRoamTarget = false;
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
        for (int i = 0; i < 20; i++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float y = Random.Range(b.min.y, b.max.y);
            Vector2 potentialPoint = new Vector2(x, y);
            if (area.OverlapPoint(potentialPoint)) return potentialPoint;
        }
        return b.center;
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

            // Calculate direction based on angle
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 start = laserPoint.position;
            Vector3 end = start + (Vector3)dir * laserDistance;

            // Visuals
            laser.SetPosition(0, start);
            laser.SetPosition(1, end);

            // DAMAGE CHECK: CircleCast works like a thick raycast
            // ใช้ LayerMask เพื่อความแม่นยำและประสิทธิภาพ
            RaycastHit2D hit = Physics2D.CircleCast(start, laserThickness, dir, laserDistance, playerLayer);

            // Debug Line ในหน้า Scene (จะเห็นเป็นสีแดงเมื่อเลเซอร์ทำงาน)
            Debug.DrawRay(start, dir * laserDistance, Color.red);

            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    if (Time.time >= nextLaserDamageTime)
                    {
                        HydrationSystem health = hit.collider.GetComponent<HydrationSystem>();
                        if (health != null)
                        {
                            health.TakeDamage(laserDamageAmount);
                            nextLaserDamageTime = Time.time + laserDamageCooldown;
                            Debug.Log("Laser Damaged Player!");
                        }
                    }
                }
            }

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
            if (rb != null) rb.linearVelocity = dir * plasmaSpeed;

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

    void OnDrawGizmosSelected()
    {
        if (roamArea != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Bounds b = roamArea.bounds;
            Gizmos.DrawCube(b.center, b.size);
        }
    }
}