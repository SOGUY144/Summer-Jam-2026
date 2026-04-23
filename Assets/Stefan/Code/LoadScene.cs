using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class LoadNextScene : MonoBehaviour
{
    public string sceneName = "Tutorial";
    public PlayableDirector director;
    public AudioSource bgmSource; // ลาก SOundorg ใส่ช่องนี้

    public void PlayTimeline()
    {
        if (bgmSource != null)
            bgmSource.Stop();
        director.Play();
    }

    public void Load()
    {
        SceneManager.LoadScene(sceneName);
    }
}