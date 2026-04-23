using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameFlowManager : MonoBehaviour
{
    [Header("Scene Sequence")]
    // เรียงลำดับซีน: tutorial, view, pipe, bossroom
    public List<string> sceneOrder = new List<string> { "tutorial", "view", "pipe", "bossroom" };

    [Header("Enemy Settings")]
    // ลากมอนสเตอร์ในซีนที่ "ต้องฆ่าให้หมด" มาใส่ที่นี่
    public List<GameObject> enemiesInScene = new List<GameObject>();

    [Header("UI References")]
    public GameObject loadingCanvas; // หน้าจอ Loading (สร้างเป็น Canvas/Panel)

    private bool isTransitioning = false;

    private void Start()
    {
        enemiesInScene = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
    }
    void Update()
    {
        if (isTransitioning) return;

        // เช็คว่ามอนสเตอร์ตายหมดหรือยัง
        CheckEnemies();
    }

    void CheckEnemies()
    {
        // ลบตัวที่ถูก Destroy ออกจาก List
        enemiesInScene.RemoveAll(item => item == null);

        // ถ้าหมดแล้ว ให้ไปซีนต่อไป
        if (enemiesInScene.Count == 0 && enemiesInScene != null)
        {
            StartCoroutine(LoadNextLevel());
        }
    }

    IEnumerator LoadNextLevel()
    {
        isTransitioning = true;

        // 1. เปิดหน้าจอ Loading
        if (loadingCanvas != null)
            loadingCanvas.SetActive(true);

        // 2. รอแป๊บนึงให้คนดูเห็นหน้า Load (เช่น 1.5 วินาที)
        yield return new WaitForSeconds(1.5f);

        // 3. หาชื่อซีนถัดไปตามลำดับ
        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = sceneOrder.IndexOf(currentScene);

        if (currentIndex != -1 && currentIndex < sceneOrder.Count - 1)
        {
            string nextScene = sceneOrder[currentIndex + 1];
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("ซัน! นี่คือซีนสุดท้ายแล้ว หรือชื่อซีนไม่ตรงกับใน List นะ");
        }
    }
}