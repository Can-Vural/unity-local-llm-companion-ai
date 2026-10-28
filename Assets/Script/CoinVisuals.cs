using UnityEngine;

public class CoinVisuals : MonoBehaviour
{
    [Header("Yanıp Sönme (Alpha) Ayarları")]
    [SerializeField] private float blinkSpeed = 4f;       // Yanıp sönme hızı
    [SerializeField] private float minAlpha = 0.3f;       // En düşük şeffaflık (0 = görünmez, 1 = tam)
    [SerializeField] private float maxAlpha = 1.0f;       // En yüksek şeffaflık

    [Header("Büyüyüp Küçülme (Pulse) Ayarları")]
    [SerializeField] private bool enablePulse = true;     // Büyüyüp küçülme aktif mi?
    [SerializeField] private float pulseAmount = 0.15f;   // Büyüme miktarı

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        // 1. Şeffaflık Yanıp Sönmesi (Alpha Pulse)
        if (spriteRenderer != null)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, maxAlpha - minAlpha) + minAlpha;
            Color currentColor = spriteRenderer.color;
            currentColor.a = alpha;
            spriteRenderer.color = currentColor;
        }

        // 2. Nefes Alma / Büyüyüp Küçülme Efekti
        if (enablePulse)
        {
            float scaleOffset = Mathf.Sin(Time.time * blinkSpeed) * pulseAmount;
            transform.localScale = baseScale + new Vector3(scaleOffset, scaleOffset, 0f);
        }
    }
}