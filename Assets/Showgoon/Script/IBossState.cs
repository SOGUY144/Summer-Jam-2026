// --- ไฟล์นี้ชื่อ IBossState.cs ---
public interface IBossState
{
    void EnterState(BossController boss);
    void UpdateState(BossController boss);
    void ExitState(BossController boss);
}