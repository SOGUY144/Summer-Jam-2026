using UnityEngine;
using UnityEngine.EventSystems; // เพิ่มตรงนี้

public class PauseMenu : MonoBehaviour
{
    public GameObject container;

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
        EventSystem.current.SetSelectedGameObject(null); // เพิ่มตรงนี้
        Debug.Log("Paused");
    }

    public void ResumeButton()
    {
        container.SetActive(false);
        Time.timeScale = 1;
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