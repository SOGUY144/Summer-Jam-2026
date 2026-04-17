using UnityEngine;

public class PauseController : MonoBehaviour
{
    // ต้องมี static เพื่อให้ NPC เรียกใช้ ClassName.VariableName ได้เลย
    public static bool IsGamePaused = false;

    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;
        Time.timeScale = pause ? 0 : 1; // หยุดเวลาในเกม
    }
}