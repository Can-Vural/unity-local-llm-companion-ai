using UnityEngine;
using TMPro;
using System.Collections;

public class GuardNPC : MonoBehaviour
{
    [Header("Ollama ve UI Referansları")]
    [SerializeField] private OllamaClient ollamaManager;
    [SerializeField] private TMP_InputField playerInputBox;
    [SerializeField] private TextMeshProUGUI guardDialogueText;

    [Header("Kapı Mantığı")]
    [SerializeField] private GameObject guardDoor;

    [Header("Oyuncu Kontrolü")]
    [SerializeField] private MonoBehaviour playerMovementScript;

    [Header("Yazı Ayarları")]
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private float interactionRange = 3f;

    private Transform playerTransform;
    private Rigidbody2D playerRb;
    private bool isPlayerNearby = false;
    private bool isChatting = false;
    private bool isWaitingForAI = false; // Çift istek atılmasını önleyen kilit

    private Coroutine typingCoroutine;

    [System.Serializable]
    public class LLMGuardData
    {
        public string reply;
        public bool openDoor;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerRb = player.GetComponent<Rigidbody2D>();

            if (playerMovementScript == null)
            {
                playerMovementScript = player.GetComponent("PlayerMovement") as MonoBehaviour;
            }
        }

        if (guardDialogueText != null)
        {
            guardDialogueText.text = "";
        }

        if (playerInputBox != null)
        {
            playerInputBox.gameObject.SetActive(false);
            playerInputBox.onSubmit.RemoveAllListeners();
            playerInputBox.onSubmit.AddListener(OnPlayerSubmit);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerNearby = distance <= interactionRange;

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && (playerInputBox == null || !playerInputBox.isFocused))
        {
            ToggleChat(!isChatting);
        }

        if (isChatting && Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleChat(false);
        }

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
            playerInputBox.interactable = state;
        }

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
            isWaitingForAI = false;
            ShowTextAnimated("Muhafız: Dur, yolcu! Altının yoksa bu kapıdan geçemezsin.");
            playerInputBox.ActivateInputField();
        }
        else
        {
            isWaitingForAI = false;
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            if (guardDialogueText != null)
            {
                guardDialogueText.text = "";
            }
        }
    }

    public void OnPlayerSubmit(string playerMessage)
    {
        // Cevap bekleniyorsa veya mesaj boşsa ikinci kez istek göndermez
        if (string.IsNullOrWhiteSpace(playerMessage) || isWaitingForAI) return;

        isWaitingForAI = true;

        if (playerInputBox != null)
        {
            playerInputBox.interactable = false; // Cevap gelene kadar metin kutusunu kilitler
        }

        ShowTextAnimated("Muhafız düşünüyor...");

        // openDoor alanının BAŞA alındığı ve ÖRNEKLİ (Few-Shot) ultra net prompt
        string prompt = $@"
You are a greedy guard in a game. Player says: '{playerMessage}'

CRITICAL: Output ONLY valid JSON.
If the player offers ANY bribe (gold, money, coins, jewels, altin, para, kese), set openDoor to true.
Otherwise set openDoor to false.

EXAMPLE 1 (Bribe offered):
{{""openDoor"": true, ""reply"": ""Harika, altınları aldım! Geçebilirsin.""}}

EXAMPLE 2 (No bribe):
{{""openDoor"": false, ""reply"": ""Altının yoksa buradan geçiş yok!""}}";

        ollamaManager.AskAI(prompt, (rawResponse, isSuccess) => {
            if (playerInputBox != null)
            {
                playerInputBox.text = "";
                playerInputBox.interactable = true; // Cevap gelince kilit açılır
            }

            isWaitingForAI = false; // Yeni mesaj atılabilir hale getirilir

            if (!isSuccess)
            {
                ShowTextAnimated(rawResponse);
                return;
            }

            Debug.Log($"[Ollama Raw Yanıt]: {rawResponse}");

            try
            {
                LLMGuardData data = JsonUtility.FromJson<LLMGuardData>(rawResponse);

                bool isBribeAccepted = false;
                if (data != null)
                {
                    isBribeAccepted = data.openDoor;

                    if (!isBribeAccepted)
                    {
                        string cleanJson = rawResponse.ToLower().Replace(" ", "");
                        if (cleanJson.Contains("\"opendoor\":true") || cleanJson.Contains("\"opendoor\":\"true\""))
                        {
                            isBribeAccepted = true;
                        }
                    }
                }

                if (data != null && !string.IsNullOrEmpty(data.reply))
                {
                    ShowTextAnimated("Muhafız: " + data.reply);

                    if (isBribeAccepted)
                    {
                        OpenGuardDoor();
                    }
                }
                else
                {
                    ShowTextAnimated("Muhafız: Altınlarından bahsetmeyeceksen çekil yolumdan!");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[JSON Parse Hatası]: {ex.Message}");
                ShowTextAnimated("Muhafız: (Paralarını sayıyor, seni duymadı...)");
            }

            if (isChatting && playerInputBox != null)
            {
                playerInputBox.ActivateInputField();
            }
        });
    }

    private void ShowTextAnimated(string fullText)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeTextRoutine(fullText));
    }

    private IEnumerator TypeTextRoutine(string textToType)
    {
        guardDialogueText.text = "";
        foreach (char letter in textToType.ToCharArray())
        {
            guardDialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void OpenGuardDoor()
    {
        if (guardDoor != null)
        {
            guardDoor.SetActive(false);
            Debug.Log("LLM Kararı: Rüşvet kabul edildi, kapı açıldı!");
        }
    }
}