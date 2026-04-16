using UnityEngine;

public class TestMove : MonoBehaviour
{
    void Update()
    {
        transform.Translate(Vector2.right * 5f * Time.deltaTime);
    }
}