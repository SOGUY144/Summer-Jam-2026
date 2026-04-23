using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] pages;
    public Button nextBtn, prevBtn, fightBtn;
    public TimelineManager timeline;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip openTutorialSound; // เสียงตอน Tutorial ขึ้นมา
    public AudioClip changePageSound;   // เสียงตอนกด Next/Prev

    private int currentIndex = 0;

    void Awake()
    {
        // สั่งซ่อนตัวเองทันทีที่เกมเริ่ม เผื่อซันลืมปิดใน Inspector
        gameObject.SetActive(false);
    }

    public void OpenTutorial()
    {
        gameObject.SetActive(true);
        currentIndex = 0;
        Time.timeScale = 0; // หยุดโลก

        // เล่นเสียงตอนเปิด Tutorial
        if (audioSource != null && openTutorialSound != null)
            audioSource.PlayOneShot(openTutorialSound);

        UpdateUI();
    }

    public void NextPage()
    {
        if (currentIndex < pages.Length - 1)
        {
            currentIndex++;
            PlayChangePageSound();
            UpdateUI();
        }
    }

    public void PrevPage()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            PlayChangePageSound();
            UpdateUI();
        }
    }

    public void FinishTutorial()
    {
        Time.timeScale = 1; // เดินเวลาต่อ
        PlayChangePageSound();
        if (timeline != null) timeline.RealSpawn();
        gameObject.SetActive(false); // ซ่อนตัวเองเมื่อเริ่มสู้
    }

    void UpdateUI()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentIndex);
        }

        // คุมปุ่ม: หน้าแรกซ่อนปุ่มซ้าย / หน้าสุดท้ายซ่อนปุ่มขวาแต่โชว์ปุ่มสู้
        prevBtn.gameObject.SetActive(currentIndex > 0);
        nextBtn.gameObject.SetActive(currentIndex < pages.Length - 1);
        fightBtn.gameObject.SetActive(currentIndex == pages.Length - 1);
    }

    void PlayChangePageSound()
    {
        if (audioSource != null && changePageSound != null)
            audioSource.PlayOneShot(changePageSound);
    }
}