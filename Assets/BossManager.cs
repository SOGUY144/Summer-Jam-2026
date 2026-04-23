using UnityEngine;

public class BossManager : MonoBehaviour
{

    public GameObject bossPrefab;
    public Vector2 BossPosition;
    public void SpawnBoss()
    {
        Instantiate(bossPrefab, BossPosition, transform.rotation);
    }
}
