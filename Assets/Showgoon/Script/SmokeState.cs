using UnityEngine;

// --- ä¿Åì¹Õéª×èÍ SmokeState.cs ---
public class SmokeState : IBossState
{
    private float duration = 60f;

    public void EnterState(BossController boss)
    {
        duration = 60f;
        boss.leftHandAnimator.SetTrigger("Smoke_Relese");
        boss.rightHandAnimator.SetTrigger("Smoke_Relese");
        boss.smokeObject.SetActive(true);
    }

    public void UpdateState(BossController boss)
    {
        duration -= Time.deltaTime;
        if (duration <= 0) boss.ChangeState(boss.idleState);
    }

    public void ExitState(BossController boss)
    {
        boss.smokeObject.SetActive(false);
    }
}