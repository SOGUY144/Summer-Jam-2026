
using UnityEngine;

public class WeaponAim : MonoBehaviour
{
    [Header("Targets")]
    public Transform crosshair; 
    public Transform firePoint;  

    [Header("Settings")]
    public float crosshairSmoothTime = 0.1f;
    private Vector2 crosshairVelocity = Vector2.zero;
    private Camera cam;
    private Vector3 mousePosWorld;

    void Start()
    {
        cam = Camera.main;
        Cursor.visible = false;
    }

    void Update()
    {
 
        mousePosWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePosWorld.z = 0f;
        crosshair.position = Vector2.SmoothDamp(crosshair.position, mousePosWorld, ref crosshairVelocity, crosshairSmoothTime);

        Vector3 lookDir = crosshair.position - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 localScale = Vector3.one;
        if (angle > 90 || angle < -90) localScale.y = -1f;
        else localScale.y = 1f;
        transform.localScale = localScale;
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("🎯 1. คลิกซ้ายยิง!");
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject bulletObj = ObjectPooler.Instance.GetPooledObject();

        if (bulletObj != null)
        {
          
            bulletObj.transform.position = firePoint.position;
            Vector2 aimDirection = (crosshair.position - firePoint.position).normalized;
            bulletObj.SetActive(true);
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Launch(aimDirection);
                Debug.Log("🔥 2. ส่งกระสุนไปที่ทิศทาง: " + aimDirection);
            }
        }
        else
        {
            Debug.LogWarning("❌ Object Pool เต็ม! ยิงไม่ออก");
        }
    }
}