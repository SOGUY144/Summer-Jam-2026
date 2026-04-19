using UnityEngine;

public class SavePoint : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite unactivatedSprite; // รูปแบบไม่มีสี
    public Sprite activatedSprite;   // รูปแบบมีสี

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // เริ่มต้นให้เป็นภาพไม่มีสี
        spriteRenderer.sprite = unactivatedSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // เช็คว่าคนทึ่มาชนคือ Player และยังไม่ได้เปิดใช้งาน
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateSavePoint();
        }
    }

    void ActivateSavePoint()
    {
        isActivated = true;
        spriteRenderer.sprite = activatedSprite; // เปลี่ยนเป็นภาพมีสี 🎨

        // บันทึกตำแหน่งปัจจุบันของ SavePoint นี้ลง PlayerPrefs (เซฟลงเครื่อง)
        PlayerPrefs.SetFloat("SafeX", transform.position.x);
        PlayerPrefs.SetFloat("SafeY", transform.position.y);
        PlayerPrefs.Save();

        Debug.Log("💾 บันทึกตำแหน่งที่พักใจแล้วที่: " + transform.position);
    }
}