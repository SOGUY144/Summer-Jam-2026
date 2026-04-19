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

    private BossState currentState;
    private bool isAttacking;

    void Start()
    {
        laser.enabled = false;
        smokeObject.SetActive(false);
        StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        while (true)
        {
            if (!isAttacking)
            {
                ChangeState(BossState.ChooseSkill);
            }

            yield return new WaitForSeconds(attackInterval);
        }
    }

    void ChangeState(BossState newState)
    {
        currentState = newState;
        Debug.Log("Boss State ? " + newState);

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

    // ================= SWEEP LASER =================
    IEnumerator LaserState()
    {
        isAttacking = true;

        Debug.Log("Laser: Charging...");
        bodyAnim.SetTrigger("BossCharge");

        yield return new WaitForSeconds(laserChargeTime);

        Debug.Log("Laser: SWEEP FIRE!");
        laser.enabled = true;

        float timer = 0f;

        while (timer < laserDuration)
        {
            float t = timer / laserDuration;

            // ?? คำนวณมุมจาก start ? end
            float angle = Mathf.Lerp(sweepStartAngle, sweepEndAngle, t);

            // ?? แปลงเป็น direction
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            Vector3 start = laserPoint.position;
            Vector3 end = start + (Vector3)dir * laserDistance;

            laser.SetPosition(0, start);
            laser.SetPosition(1, end);

            timer += Time.deltaTime;
            yield return null;
        }

        laser.enabled = false;
        Debug.Log("Laser: End");

        isAttacking = false;
    }

    // ================= SMOKE =================
    IEnumerator SmokeState()
    {
        isAttacking = true;

        Animator hand = GetNearestHand();
        hand.SetTrigger("Smoke_Relese");

        smokeObject.SetActive(true);

        yield return new WaitForSeconds(smokeDuration);

        smokeObject.SetActive(false);

        isAttacking = false;
    }

    // ================= PLASMA =================
    IEnumerator PlasmaState()
    {
        isAttacking = true;

        int repeat = Random.Range(plasmaMin, plasmaMax + 1);

        for (int i = 0; i < repeat; i++)
        {
            Animator hand = GetNearestHand();

            yield return new WaitForSeconds(plasmaChargeTime);

            hand.SetTrigger("Plasma");

            Transform firePoint = (hand == leftHandAnim) ? leftFirePoint : rightFirePoint;

            GameObject plasma = Instantiate(plasmaPrefab, firePoint.position, Quaternion.identity);

            Vector2 targetPos = player.position + new Vector3(0, 0.5f, 0);
            Vector2 dir = (targetPos - (Vector2)firePoint.position).normalized;

            Rigidbody2D rb = plasma.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = dir * plasmaSpeed;

            yield return new WaitForSeconds(0.5f);
        }

        isAttacking = false;
    }

    // ================= HAND SELECT =================
    Animator GetNearestHand()
    {
        float distL = Vector2.Distance(player.position, leftHandAnim.transform.position);
        float distR = Vector2.Distance(player.position, rightHandAnim.transform.position);

        return (distL < distR) ? leftHandAnim : rightHandAnim;
    }
}