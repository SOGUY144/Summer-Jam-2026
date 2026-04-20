using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class LaserBeam : MonoBehaviour
{
    [Header("Settings")]
    public float damageAmount = 30f;
    public float damageCooldown = 1.0f; // ใช้ 1.0f แทน 1s เพื่อแก้ Error CS1519
    public float laserLength = 10f;

    private LineRenderer lineRenderer;
    private EdgeCollider2D edgeCollider;
    private float nextDamageTime;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();

        // บังคับให้ Collider เป็น Trigger
        edgeCollider.isTrigger = true;

        // ตั้งค่า LineRenderer เบื้องต้น
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        UpdateLaserLogic();
    }

    void UpdateLaserLogic()
    {
        // คำนวณจุดเริ่มต้นและจุดจบในโลกจริง (World Space)
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + (transform.right * laserLength);

        // 1. อัปเดตเส้นที่มองเห็น
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        // 2. อัปเดต Collider ให้ตรงกับเส้น
        // EdgeCollider ใช้ Local Space ดังนั้นเราต้องแปลงจุดกลับเป็น Local
        Vector2 localStart = Vector2.zero;
        Vector2 localEnd = transform.InverseTransformDirection(transform.right) * laserLength;

        edgeCollider.points = new Vector2[] { localStart, localEnd };
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // เช็คว่าสิ่งที่โดนคือ Player หรือไม่
        if (collision.CompareTag("Player"))
        {
            if (Time.time >= nextDamageTime)
            {
                // มองหา Script HydrationSystem ใน Player
                HydrationSystem health = collision.GetComponent<HydrationSystem>();

                if (health != null)
                {
                    health.TakeDamage(damageAmount);
                    nextDamageTime = Time.time + damageCooldown;
                    Debug.Log("Laser hit player! Damage: " + damageAmount);
                }
            }
        }
    }
}