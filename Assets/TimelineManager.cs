using System.Collections.Generic;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    public List<GameObject> enemiesInScene = new List<GameObject>();
    public GameObject Timeline2;
    public bool IsStartTimeline1 = false;

    public TutorialManager tutorialManager; // ลาก Tutorial_System มาใส่

    // ตัวที่ NPC หรือ Timeline เรียกตอนแรก
    public void SpawnEnemy()
    {
        if (tutorialManager != null)
        {
            tutorialManager.OpenTutorial(); // ให้ Tutorial เด้งก่อน
        }
        else
        {
            RealSpawn(); // ถ้าไม่มี Tutorial ให้เกิดเลย
        }
    }

    // ตัวที่ Tutorial จะเรียกกลับมาเมื่ออ่านจบ
    public void RealSpawn()
    {
        foreach (GameObject enemy in enemiesInScene)
        {
            if (enemy != null) enemy.SetActive(true);
        }
        IsStartTimeline1 = true;
    }

    void Update()
    {
        if (IsStartTimeline1)
        {
            enemiesInScene.RemoveAll(item => item == null);
            if (enemiesInScene.Count == 0)
            {
                if (Timeline2 != null) Timeline2.SetActive(true);
                IsStartTimeline1 = false;
            }
        }
    }
}