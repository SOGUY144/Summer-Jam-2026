using UnityEngine;

// --- ไฟล์นี้ชื่อ LaserState.cs ---
public class LaserState : IBossState
{
    private float chargeTimer = 1f;
    private float activeTimer = 5f;
    private bool isFiring = false;

    public void EnterState(BossController boss)
    {
        chargeTimer = 1f;
        activeTimer = 5f;
        isFiring = false;
        boss.bodyAnimator.SetTrigger("BossCharge");
    }

    public void UpdateState(BossController boss)
    {
        if (!isFiring)
        {
            chargeTimer -= Time.deltaTime;
            if (chargeTimer <= 0)
            {
                isFiring = true;
                boss.laserRenderer.enabled = true;
            }
        }
        else
        {
            activeTimer -= Time.deltaTime;
            boss.laserRenderer.SetPosition(0, boss.bodyObj.transform.position);

            // ส่ายเลเซอร์กวาดพื้น
            float angle = Mathf.Sin(Time.time * 2f) * 45f;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            boss.laserRenderer.SetPosition(1, boss.bodyObj.transform.position + dir * 50f);

            if (activeTimer <= 0) boss.ChangeState(boss.idleState);
        }
    }

    public void ExitState(BossController boss)
    {
        boss.laserRenderer.enabled = false;
    }
}