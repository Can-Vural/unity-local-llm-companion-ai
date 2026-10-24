using UnityEngine;
using TMPro;

public class GuardNPC : MonoBehaviour
{
    [Header("Ollama ve UI Referansları")]
    [SerializeField] private OllamaClient ollamaManager;
    [SerializeField] private TMP_InputField playerInputBox;
    [SerializeField] private TextMeshProUGUI guardDialogueText;

    [Header("Oyuncu Kontrolü")]
    [SerializeField] private MonoBehaviour playerMovementScript;

    [Header("Ayarlar")]
    [SerializeField] private float interactionRange = 3f;

    private Transform playerTransform;
    private Rigidbody2D playerRb;
    private bool isPlayerNearby = false;
    private bool isChatting = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerRb = player.GetComponent<Rigidbody2D>();

            if (playerMovementScript == null)
            {
                playerMovementScript = player.GetComponent("PlayerMovement") as MonoBehaviour
                                    ?? player.GetComponent("BirdMovement") as MonoBehaviour;
            }
        }

        // 1. DÜZELTME: Oyuna başlarken ekrandaki "New Text" yazısını siler
        if (guardDialogueText != null)
        {
            guardDialogueText.text = "";
        }

        if (playerInputBox != null)
        {
            playerInputBox.gameObject.SetActive(false);

            // Sadece Enter'a basıldığında mesajı göndermesi için Dinleyici ekliyoruz
            playerInputBox.onSubmit.RemoveAllListeners();
            playerInputBox.onSubmit.AddListener(OnPlayerSubmit);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerNearby = distance <= interactionRange;

        // E tuşuna basıldığında sohbeti aç/kapat (Yazı yazılırken E tuşunu yutmaz)
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && (playerInputBox == null || !playerInputBox.isFocused))
        {
            ToggleChat(!isChatting);
        }

        // ESC ile sohbetten çıkma
        if (isChatting && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleChat(false);
        }

        // Oyuncu muhafızdan uzaklaşırsa sohbeti kapat ve ekrandaki yazıyı sil
        if (!isPlayerNearby && isChatting)
        {
            ToggleChat(false);
        }
    }

    private void ToggleChat(bool state)
    {
        isChatting = state;

        if (playerInputBox != null)
        {
            playerInputBox.gameObject.SetActive(isChatting);
        }

        // Oyuncu hareketini kilitler / açar
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = !isChatting;
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        if (isChatting)
        {
            guardDialogueText.text = "Muhafız: Dur, yolcu! Buraya yaklaşmaya nasıl cüret edersin?";
            playerInputBox.ActivateInputField();
        }
        else
        {
            // 2. DÜZELTME: Sohbet kapandığında ekrandaki eski mesajı siler
            if (guardDialogueText != null)
            {
                guardDialogueText.text = "";
            }
        }
    }

    public void OnPlayerSubmit(string playerMessage)
    {
        if (string.IsNullOrWhiteSpace(playerMessage)) return;

        guardDialogueText.text = "Muhafız düşünüyor...";

        string prompt = $"Sen 2D bir platform oyununda kapıyı koruyan katı, şüpheci ve inatçı bir muhafız NPC'sisin. Oyuncu sana şöyle söyledi: '{playerMessage}'. Muhafız karakterine uygun, kısa, net ve kararlı bir yanıt ver. Yanıtın kesinlikle Türkçe olmalıdır.";

        ollamaManager.AskAI(prompt, (response) => {
            guardDialogueText.text = "Muhafız: " + response;
            playerInputBox.text = "";

            if (isChatting)
            {
                playerInputBox.ActivateInputField();
            }
        });
    }
}