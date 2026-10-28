using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Uçuş Ayarları")]
    public float moveSpeed = 7f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Vector2 movement;
    private bool hasIsMovingParam = false;

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        // Animator bileşeninde 'isMoving' parametresi olup olmadığını güvenle kontrol eder
        if (anim != null)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == "isMoving")
                {
                    hasIsMovingParam = true;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        // W/S (Yukarı/Aşağı) ve A/D (Sol/Sağ) girdileri
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Kuşun bakış yönünü ayarlar
        if (movement.x > 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }
        else if (movement.x < 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = true;
        }

        // 'isMoving' parametresi varsa tetikler, yoksa konsola sarı uyarı atmaz
        if (anim != null && hasIsMovingParam)
        {
            anim.SetBool("isMoving", movement.sqrMagnitude > 0);
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = movement.normalized * moveSpeed;
        }
    }
}