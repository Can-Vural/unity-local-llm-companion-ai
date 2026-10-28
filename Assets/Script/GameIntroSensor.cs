using UnityEngine;
using TMPro;
using System.Collections;

public class GameIntroSensor : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private TextMeshProUGUI introTextUI;

    [Header("Mesaj Ayarları")]
    [TextArea(3, 5)]
    [SerializeField] private string introMessage = "Hoş geldin! Yancı NPC seni takip ediyor. Engelleri aşmak için renkli alanları kullan.";
    [SerializeField] private float displayDuration = 4f; // Yazı bittikten sonra ekranda kalma süresi (saniye)
    [SerializeField] private float typingSpeed = 0.03f;  // Harf yazılma hızı

    private bool hasTriggered = false;
    private Coroutine displayCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece bir kez çalışmasını sağlar
        if (hasTriggered) return;

        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            hasTriggered = true;
            if (displayCoroutine != null) StopCoroutine(displayCoroutine);
            displayCoroutine = StartCoroutine(ShowAndHideMessage());
        }
    }

    private IEnumerator ShowAndHideMessage()
    {
        if (introTextUI == null) yield break;

        introTextUI.text = "";

        // Daktilo efekti ile harf harf yazdırır
        foreach (char letter in introMessage.ToCharArray())
        {
            introTextUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Belirtilen süre kadar ekranda bekletir
        yield return new WaitForSeconds(displayDuration);

        // Yazıyı temizler ve görünmez alanı kapatır
        introTextUI.text = "";
        gameObject.SetActive(false);
    }
}