using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator bodyAnimator;
    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    [Header("Sprite Renderers for Flipping")]
    public SpriteRenderer bodySprite;
    public SpriteRenderer leftHandSprite;
    public SpriteRenderer rightHandSprite;

    [Header("Parts Objects")]
    public GameObject bodyObj;
    public GameObject leftHandObj;
    public GameObject rightHandObj;

    [Header("Assign Positions (Local)")]
    [Tooltip("ตำแหน่ง Body เมื่อบสหันขวา (2D)")]
    public Vector2 bodyTargetLocalPos;
    [Tooltip("ตำแหน่งมือซ้ายเมื่อบสหันขวา (2D)")]
    public Vector2 leftHandTargetLocalPos;
    [Tooltip("ตำแหน่งมือขวาเมื่อบสหันขวา (2D)")]
    public Vector2 rightHandTargetLocalPos;

    [Header("Fire Points")]
    public Transform laserFirePoint;
    public Transform leftHandFirePoint;
    public Transform rightHandFirePoint;

    [Header("Skill Settings")]
    public LineRenderer laserRenderer;
    public GameObject smokeObject;
    public GameObject plasmaPrefab;
    public float projectileSpeed = 10f;
    public int plasmaMinRepetitions = 3;
    public int plasmaMaxRepetitions = 5;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float moveRange = 5f;
    private Vector2 startPosition; // เปลี่ยนเป็น Vector2
    private bool movingRight = true;

    private IBossState currentState;

    public IdleState idleState = new IdleState();
    public LaserState laserState = new LaserState();
    public SmokeState smokeState = new SmokeState();
    public PlasmaState plasmaState = new PlasmaState();

    void Start()
    {
        startPosition = transform.position;

        if (laserRenderer) laserRenderer.enabled = false;
        if (smokeObject) smokeObject.SetActive(false);

        ChangeState(idleState);
    }

    void Update()
    {
        if (currentState != null)
            currentState.UpdateState(this);

        HandleFlip();
        HandleMovementPattern();
    }

    private void HandleMovementPattern()
    {
        float targetX = movingRight ? startPosition.x + moveRange : startPosition.x - moveRange;
        Vector2 targetPos = new Vector2(targetX, startPosition.y);

        // ใช้ Vector2.MoveTowards สำหรับ 2D
        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            movingRight = !movingRight;
        }
    }

    private void HandleFlip()
    {
        if (player == null) return;

        bool isPlayerOnRight = player.position.x > transform.position.x;

        // 1. หันหน้า Sprite
        if (bodySprite) bodySprite.flipX = isPlayerOnRight;
        if (leftHandSprite) leftHandSprite.flipX = isPlayerOnRight;
        if (rightHandSprite) rightHandSprite.flipX = isPlayerOnRight;

        // 2. กำหนดตำแหน่งส่วนประกอบต่างๆ ตามที่ Assign ไว้ใน Inspector
        // ใช้ xMult เพื่อสลับฝั่งซ้าย-ขวาตามทิศทางที่บสหันหน้าไป
        float xMult = isPlayerOnRight ? 1f : -1f;

        if (bodyObj)
        {
            bodyObj.transform.localPosition = new Vector3(
                bodyTargetLocalPos.x * xMult,
                bodyTargetLocalPos.y,
                bodyObj.transform.localPosition.z // รักษาค่า Z ไว้เผื่อจัด Sorting Order
            );
        }

        if (leftHandObj)
        {
            leftHandObj.transform.localPosition = new Vector3(
                leftHandTargetLocalPos.x * xMult,
                leftHandTargetLocalPos.y,
                leftHandObj.transform.localPosition.z
            );
        }

        if (rightHandObj)
        {
            rightHandObj.transform.localPosition = new Vector3(
                rightHandTargetLocalPos.x * xMult,
                rightHandTargetLocalPos.y,
                rightHandObj.transform.localPosition.z
            );
        }
    }

    public void ShootPlasma(Transform firePoint)
    {
        if (plasmaPrefab && player != null && firePoint != null)
        {
            GameObject projectile = Instantiate(plasmaPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 direction = (player.position - firePoint.position).normalized;
                rb.velocity = direction * projectileSpeed;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }

    public void ChangeState(IBossState newState)
    {
        if (currentState != null) currentState.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    // อัปเดตการหาตำแหน่งให้ใช้ Vector2 เพื่อข้ามการคำนวณแกน Z (ความลึก) ที่ไม่จำเป็น
    public Animator GetNearestHandAnimator()
    {
        float distLeft = Vector2.Distance(player.position, leftHandObj.transform.position);
        float distRight = Vector2.Distance(player.position, rightHandObj.transform.position);
        return distLeft < distRight ? leftHandAnimator : rightHandAnimator;
    }

    public Transform GetNearestHandFirePoint()
    {
        float distLeft = Vector2.Distance(player.position, leftHandObj.transform.position);
        float distRight = Vector2.Distance(player.position, rightHandObj.transform.position);
        return distLeft < distRight ? leftHandFirePoint : rightHandFirePoint;
    }

    public GameObject GetNearestHandObject()
    {
        float distLeft = Vector2.Distance(player.position, leftHandObj.transform.position);
        float distRight = Vector2.Distance(player.position, rightHandObj.transform.position);
        return distLeft < distRight ? leftHandObj : rightHandObj;
    }
}