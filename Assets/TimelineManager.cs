using System.Collections.Generic;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    public List<GameObject> enemiesInScene = new List<GameObject>();
    public GameObject Timeline2;
    public bool IsStartTimeline1 = false;

    public TutorialManager tutorialManager;

    void Start()
    {
        // กันเหนียว: สั่งปิดมอนสเตอร์ทุกตัวใน List ทันทีที่เริ่มเกม
        foreach (GameObject enemy in enemiesInScene)
        {
            if (enemy != null) enemy.SetActive(false);
        }
    }

    public void SpawnEnemy()
    {
        if (tutorialManager != null)
        {
            tutorialManager.OpenTutorial();
        }
        else
        {
            RealSpawn();
        }
    }

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