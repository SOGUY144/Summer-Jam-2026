using UnityEngine;

// --- ไฟล์นี้ชื่อ PlasmaState.cs ---
public class PlasmaState : IBossState
{
    private int count;
    private int currentCount;
    private float attackDelay = 1.5f;
    private float timer;

    public void EnterState(BossController boss)
    {
        count = Random.Range(boss.plasmaMinRepetitions, boss.plasmaMaxRepetitions + 1);
        currentCount = 0;
        timer = 0;
    }

    public void UpdateState(BossController boss)
    {
        timer -= Time.deltaTime;
        if (timer <= 0 && currentCount < count)
        {
            Attack(boss);
            currentCount++;
            timer = attackDelay;
        }
        else if (currentCount >= count)
        {
            boss.ChangeState(boss.idleState);
        }
    }

    private void Attack(BossController boss)
    {
        Animator targetHand = boss.GetNearestHandAnimator();
        GameObject handObj = boss.GetNearestHandObject();

        targetHand.SetTrigger("Plasma");

        // สุ่มวาร์ปมือ (หน้า/หลัง/ตำแหน่งผู้เล่น) ตามโจทย์
        int warpType = Random.Range(0, 3);
        Vector3 targetPos = boss.player.position;

        switch (warpType)
        {
            case 0: targetPos += boss.player.forward * 5f; break; // ด้านหน้า
            case 1: targetPos -= boss.player.forward * 5f; break; // ด้านหลัง
            case 2: targetPos += Vector3.up * 2f; break;          // เหนือหัวพอดี
        }

        handObj.transform.position = targetPos;

        // Spawn Plasma Ball
        if (boss.plasmaPrefab)
        {
            Object.Instantiate(boss.plasmaPrefab, handObj.transform.position, Quaternion.identity);
        }
    }

    public void ExitState(BossController boss) { }
}