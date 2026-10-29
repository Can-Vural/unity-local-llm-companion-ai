using UnityEngine;

public class CoinVisuals : MonoBehaviour
{
    [Header("Yanıp Sönme (Alpha) Ayarları")]
    [SerializeField] private float blinkSpeed = 4f;      
    [SerializeField] private float minAlpha = 0.3f;     
    [SerializeField] private float maxAlpha = 1.0f;     

    [Header("Büyüyüp Küçülme (Pulse) Ayarları")]
    [SerializeField] private bool enablePulse = true;     
    [SerializeField] private float pulseAmount = 0.15f;   

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (spriteRenderer != null)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, maxAlpha - minAlpha) + minAlpha;
            Color currentColor = spriteRenderer.color;
            currentColor.a = alpha;
            spriteRenderer.color = currentColor;
        }

        if (enablePulse)
        {
            float scaleOffset = Mathf.Sin(Time.time * blinkSpeed) * pulseAmount;
            transform.localScale = baseScale + new Vector3(scaleOffset, scaleOffset, 0f);
        }
    }
}