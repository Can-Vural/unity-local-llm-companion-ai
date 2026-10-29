using UnityEngine;
using TMPro;
using System.Collections;

public class GameIntroSensor : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private TextMeshProUGUI introTextUI;

    [Header("Mesaj Ayarları")]
    [TextArea(3, 5)]
    [SerializeField] private string introMessage = "Hoş geldin! NPC seni takip ediyor. Engelleri aşmak için renkli alanları kullan.";
    [SerializeField] private float displayDuration = 4f; 
    [SerializeField] private float typingSpeed = 0.03f; 

    private bool hasTriggered = false;
    private Coroutine displayCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
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

        foreach (char letter in introMessage.ToCharArray())
        {
            introTextUI.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(displayDuration);

        introTextUI.text = "";
        gameObject.SetActive(false);
    }
}