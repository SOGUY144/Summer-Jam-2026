using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private float lifeTime = 0.15f;
    private float timer;
    private bool initialized = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
        }
    }

    public void Setup(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null) return;
        }

        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        Color c = lineRenderer.startColor;
        if (c.a <= 0f) c.a = 1f;
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;

        timer = lifeTime;
        initialized = true;
    }

    void Update()
    {
        if (!initialized || lineRenderer == null) return;

        timer -= Time.deltaTime;

        float alpha = Mathf.Clamp01(timer / lifeTime);
        Color c = lineRenderer.startColor;
        c.a = alpha;

        lineRenderer.startColor = c;
        lineRenderer.endColor = c;

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}