using UnityEngine;
using UnityEngine.SceneManagement;


public class BossManager : MonoBehaviour
{

    public GameObject boss;
    public Vector2 BossPosition;
    public void SpawnBoss()
    {
        SceneManager.LoadScene("End");
    }
}
