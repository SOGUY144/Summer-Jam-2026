using UnityEngine;

/// <summary>
/// Handles weapon aiming by positioning a crosshair at the mouse cursor
/// and calculating firing directions toward that point.
/// </summary>
public class WeaponAim : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("The transform representing the visual crosshair in the world.")]
    public Transform crosshair;
    [Tooltip("The point from which the bullets are spawned.")]
    public Transform firePoint;

    [Header("Settings")]
    [Tooltip("How smoothly the crosshair follows the mouse.")]
    public float crosshairSmoothTime = 0.05f;

    private Vector2 _crosshairVelocity = Vector2.zero;
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;

        // Ensure the cursor is visible, though the crosshair transform will likely be the primary visual aid.
        Cursor.visible = true;

        if (crosshair == null)
        {
            Debug.LogError("WeaponAim: Please assign a Crosshair Transform in the inspector.");
        }
    }

    void Update()
    {
        HandleCrosshairMovement();

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    /// <summary>
    /// Converts mouse screen position to world coordinates and updates the crosshair position.
    /// </summary>
    private void HandleCrosshairMovement()
    {
        if (crosshair == null) return;

        // 1. Get mouse position in pixels (Screen Space)
        Vector3 mouseScreenPos = Input.mousePosition;

        // 2. Convert Screen Space to World Space
        // Note: For 2D, we ensure the z-position is relative to the camera distance or 0
        Vector3 mouseWorldPos = _cam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -_cam.transform.position.z));

        // 3. Smoothly move the crosshair to the mouse position
        Vector3 targetPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
        crosshair.position = Vector2.SmoothDamp(crosshair.position, targetPos, ref _crosshairVelocity, crosshairSmoothTime);
    }

    /// <summary>
    /// Spawns a bullet from the pool and launches it toward the current crosshair position.
    /// </summary>
    private void Shoot()
    {
        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("WeaponAim: ObjectPooler instance not found in scene!");
            return;
        }

        GameObject bulletObj = ObjectPooler.Instance.GetPooledObject();

        if (bulletObj != null)
        {
            // Position the bullet at the muzzle/fire point
            bulletObj.transform.position = firePoint.position;

            // Calculate direction from firePoint to the crosshair
            Vector2 aimDirection = ((Vector2)crosshair.position - (Vector2)firePoint.position).normalized;

            // Rotate the bullet to face the direction of travel (optional, assumes bullet sprite faces Right)
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            bulletObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            bulletObj.SetActive(true);

            // Interface with the Bullet script to set velocity
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Launch(aimDirection);
            }
        }
        else
        {
            Debug.LogWarning("WeaponAim: Object Pool is empty or null!");
        }
    }
}