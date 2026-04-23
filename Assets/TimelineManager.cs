using System.Collections.Generic;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{

    public List<GameObject> enemiesInScene = new List<GameObject>();
    public GameObject Timeline2;

    public void SpawnEnemy()
    {
        foreach (GameObject enemy in enemiesInScene)
        {
            enemy.SetActive(true);
        }   
    }
    void CheckEnemies()
    {
        // ลบตัวที่ถูก Destroy ออกจาก List
        enemiesInScene.RemoveAll(item => item == null);

        // ถ้าหมดแล้ว ให้ไปซีนต่อไป
        if (enemiesInScene.Count == 0 && enemiesInScene != null)
        {
            Timeline2.SetActive(true);
        }

    }
}
