using UnityEngine;
using UnityEngine.SceneManagement; // สำคัญ: ต้องมีเพื่อใช้เปลี่ยนซีน
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Settings")]
    public string nextSceneName; // ตั้งชื่อซีนต่อไปใน Inspector ได้เลย

    // รายชื่อศัตรูที่ต้องกำจัดให้หมดก่อนถึงจะเปลี่ยนซีน
    public List<GameObject> targetEnemies = new List<GameObject>();

    private void Update()
    {
        // เช็คตลอดเวลาว่าศัตรูใน List เหลืออยู่ไหม
        CheckEnemies();
    }

    void CheckEnemies()
    {
        // วนลูปเช็คใน List ว่าตัวไหนตาย (เป็น null) ให้ลบออก
        for (int i = targetEnemies.Count - 1; i >= 0; i--)
        {
            if (targetEnemies[i] == null)
            {
                targetEnemies.RemoveAt(i);
            }
        }

        // ถ้าศัตรูใน List หมดเกลี้ยงแล้ว ให้ไปซีนต่อไป
        if (targetEnemies.Count == 0)
        {
            GoToNextScene();
        }
    }

    void GoToNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("ซัน! ลืมใส่ชื่อซีนต่อไปใน Inspector หรือเปล่า?");
        }
    }
}