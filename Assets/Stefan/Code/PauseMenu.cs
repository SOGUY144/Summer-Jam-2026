using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public GameObject container;
    public static bool IsPaused = false; // เพิ่มตรงนี้

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OpenPause();
        }
    }

    public void OpenPause()
    {
        container.SetActive(true);
        Time.timeScale = 0;
        IsPaused = true; // เพิ่มตรงนี้
        EventSystem.current.SetSelectedGameObject(null);
        Debug.Log("Paused");
    }

    public void ResumeButton()
    {
        container.SetActive(false);
        Time.timeScale = 1;
        IsPaused = false; // เพิ่มตรงนี้
        Debug.Log("Resumed");
    }

    public void QuitButton()
    {
        Debug.Log("Quit");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}