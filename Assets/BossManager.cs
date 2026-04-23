using UnityEngine;

public class BossManager : MonoBehaviour
{

    public GameObject boss;
    public Vector2 BossPosition;
    public void SpawnBoss()
    {
        boss.SetActive(true);
    }
}
