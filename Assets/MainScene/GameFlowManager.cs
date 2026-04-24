using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ต้องเพิ่มตัวนี้เพื่อคุม Image
using System.Collections;
using System.Collections.Generic;

public class GameFlowManager : MonoBehaviour
{
    [Header("Scene Sequence")]
    public List<string> sceneOrder = new List<string> { "tutorial", "view", "pipe", "bossroom" };

    [Header("Enemy Settings")]
    public List<GameObject> enemiesInScene = new List<GameObject>();

    [Header("UI References (Loading Screen)")]
    public CanvasGroup loadingCanvasGroup; // ลาก loadingCanvas ที่มี Canvas Group มาใส่
    public Image loadingBackground;       // ลาก Loading_BG มาใส่
    public RectTransform gearIcon;        // ลาก Gear_Icon มาใส่

    [Header("Loading Assets")]
    public List<Sprite> loadingSprites;   // ลากรูป Thirsty ทั้ง 2 รูป (`image_24.png`, `image_25.png`) มาใส่ใน List นี้
    public float gearRotationSpeed = 200f;// ความเร็วในการหมุนฟันเฟือง
    public float fadeDuration = 1.0f;     // เวลาในการ Fade In/Out (วินาที)

    private bool isTransitioning = false;

    private void Start()
    {
        // ซ่อนหน้าจอโหลดตอนเริ่มเกม และเซ็ตค่า Alpha เป็น 0 (โปร่งใส)
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 0;
            loadingCanvasGroup.gameObject.SetActive(false);
        }

        enemiesInScene = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
    }

    void Update()
    {
        if (isTransitioning) return;
        CheckEnemies();
    }

    void CheckEnemies()
    {
        enemiesInScene.RemoveAll(item => item == null);
        if (enemiesInScene.Count == 0 && enemiesInScene != null)
        {
            StartCoroutine(TransitionToNextLevel());
        }
    }

    // --- ส่วนคุมการเปลี่ยนฉากและแอนิเมชัน ---
    IEnumerator TransitionToNextLevel()
    {
        isTransitioning = true;

        // 1. สุ่มรูปภาพพื้นหลัง
        if (loadingSprites.Count > 0 && loadingBackground != null)
        {
            int randomIndex = Random.Range(0, loadingSprites.Count);
            loadingBackground.sprite = loadingSprites[randomIndex];
        }

        // 2. ค่อยๆ ดำ (Fade Out ฉากปัจจุบัน)
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(0, 1)); // Fade จาก 0 ไป 1 (ดำสนิท)
        }

        // 3. เริ่มโหลดซีนใหม่แบบ Async (โหลดใน Background)
        string nextSceneName = GetNextSceneName();
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("ซัน! ไม่มีซีนถัดไปแล้ว หรือชื่อซีนไม่ตรงนะ");
            isTransitioning = false;
            if (loadingCanvasGroup != null) loadingCanvasGroup.gameObject.SetActive(false);
            yield break;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false; // อย่าเพิ่งเปิดซีนใหม่จนกว่าจะโหลดเสร็จ

        // 4. วนลูปตอนกำลังโหลด: หมุนฟันเฟืองไปเรื่อยๆ
        while (!operation.isDone)
        {
            // หมุนฟันเฟืองรอบแกน Z
            if (gearIcon != null)
            {
                gearIcon.Rotate(0, 0, -gearRotationSpeed * Time.deltaTime);
            }

            // ถ้าโหลดเสร็จเกือบ 100% แล้ว (Async จะค้างที่ 0.9)
            if (operation.progress >= 0.9f)
            {
                // โหลดเสร็จแล้ว! รอแป๊บนึง (เช่น 0.5 วินาที) แล้วค่อยเปิดซีนใหม่
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }
            yield return null; // รอเฟรมถัดไป
        }

        // 5. เมื่อซีนใหม่เปิดแล้ว ค่อยๆ ปรากฏฉาก (Fade In)
        if (loadingCanvasGroup != null)
        {
            yield return StartCoroutine(Fade(1, 0)); // Fade จาก 1 ไป 0 (เห็นภาพฉากใหม่)
            loadingCanvasGroup.gameObject.SetActive(false);
        }
    }

    // ฟังก์ชันช่วยทำ Fade
    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            loadingCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            yield return null;
        }
        loadingCanvasGroup.alpha = endAlpha; // เซ็ตค่าสุดท้ายให้เป๊ะ
    }

    string GetNextSceneName()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = sceneOrder.IndexOf(currentScene);

        if (currentIndex != -1 && currentIndex < sceneOrder.Count - 1)
        {
            return sceneOrder[currentIndex + 1];
        }
        return null;
    }
}