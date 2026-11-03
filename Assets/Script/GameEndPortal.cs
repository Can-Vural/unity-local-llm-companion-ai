using UnityEngine;
using TMPro;

public class GameEndPortal : MonoBehaviour
{
    [Header("UI Referansı")]
    [SerializeField] private GameObject winPanel;

    private bool isGameEnded = false;

    private void Start()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameEnded) return;

        // Oyuncu portala değdiğinde
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        isGameEnded = true;

        // Siyah ekranı ve TEŞEKKÜRLER yazısını aç
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        // Oyunu ve zamanı durdur
        Time.timeScale = 0f;
        Debug.Log("[GameEndPortal]: Oyun tamamlandı! X tuşu bekleniyor...");
    }

    private void Update()
    {
        if (isGameEnded)
        {
            // X tuşuna basıldığında oyundan çık
            if (Input.GetKeyDown(KeyCode.X))
            {
                Debug.Log("[GameEndPortal]: X tuşuna basıldı. Uygulama kapatılıyor...");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}