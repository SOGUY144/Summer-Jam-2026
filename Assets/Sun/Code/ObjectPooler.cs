using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;
    public GameObject bulletPrefab;
    public int poolSize = 20;
    private List<GameObject> bulletPool = new List<GameObject>();

    void Awake() { Instance = this; }

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            bulletPool.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        foreach (GameObject obj in bulletPool)
        {
            if (!obj.activeInHierarchy) return obj;
        }
        GameObject newObj = Instantiate(bulletPrefab);
        newObj.SetActive(false);
        bulletPool.Add(newObj);
        return newObj;
    }
}