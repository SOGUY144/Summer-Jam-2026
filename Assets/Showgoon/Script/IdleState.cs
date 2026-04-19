using UnityEngine;

// --- ä¿Åì¹Õéª×èÍ IdleState.cs ---
public class IdleState : IBossState
{
    private float timer;

    public void EnterState(BossController boss)
    {
        timer = 2f;
        boss.bodyAnimator.SetTrigger("Idle");
        boss.leftHandAnimator.SetTrigger("Idle");
        boss.rightHandAnimator.SetTrigger("Idle");
    }

    public void UpdateState(BossController boss)
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            int rand = Random.Range(0, 3);
            if (rand == 0) boss.ChangeState(boss.laserState);
            else if (rand == 1) boss.ChangeState(boss.smokeState);
            else boss.ChangeState(boss.plasmaState);
        }
    }

    public void ExitState(BossController boss) { }
}