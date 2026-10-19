using UnityEngine;

public class BirdMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [Tooltip("Kuşun hareket hızı")]
    public float moveSpeed = 5f;

    [Header("Bileşenler")]
    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Bileşenleri otomatik olarak alıyoruz
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Yer çekiminin kuşu aşağı çekmesini istemiyorsan Rigidbody2D'nin Gravity Scale'ini 0 yapmalısın.
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // Girdileri alıyoruz (A/D veya Sol/Sağ oklar yatay, W/S veya Yukarı/Aşağı oklar dikey)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movement = new Vector2(moveX, moveY).normalized;

        // Sağa veya sola giderken kuşun yönünü değiştirme (Sprite Flipping)
        if (moveX > 0)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = false; // Sağa bakıyor
        }
        else if (moveX < 0)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = true;  // Sola bakıyor
        }
    }

    void FixedUpdate()
    {
        // Fizik tabanlı pürüzsüz hareket
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}