using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    [Header("Target Panel")]
    public GameObject targetPanel;

    public void OpenPanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            Time.timeScale = 0;
            Debug.Log("Panel Opened / Game Paused");
        }
    }

    public void ClosePanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
            Time.timeScale = 1;
            Debug.Log("Panel Closed / Game Resumed");
        }
    }
}