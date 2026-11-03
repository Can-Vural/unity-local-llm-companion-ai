using UnityEngine;
using TMPro;
using System.Collections;

public class GuardNPC : MonoBehaviour
{
    [Header("Ollama & UI References")]
    [SerializeField] private MonoBehaviour llmServiceComponent;
    [SerializeField] private TMP_InputField playerInputBox;
    [SerializeField] private TextMeshProUGUI guardDialogueText;

    [Header("Door Logic")]
    [SerializeField] private GameObject guardDoor;

    [Header("Player Control")]
    [SerializeField] private MonoBehaviour playerMovementScript;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.03f;
    [SerializeField] private float interactionRange = 3f;

    [Header("Gold Requirements")]
    [SerializeField] private int requiredGold = 5;

    private ILLMService llmService;
    private Transform playerTransform;
    private Rigidbody2D playerRb;
    private PlayerInventory playerInventory;
    private bool isPlayerNearby = false;
    private bool isChatting = false;
    private bool isWaitingForAI = false;

    // Kapının açılıp açılmadığını takip eden durum değişkeni
    private bool isDoorOpened = false;

    private Coroutine typingCoroutine;

    [System.Serializable]
    public class LLMGuardData
    {
        public string dialogue;
        public string action;
    }

    private void Start()
    {
        // Interface atamasını doğrulama
        if (llmServiceComponent != null && llmServiceComponent is ILLMService service)
        {
            llmService = service;
        }
        else
        {
            llmService = GetComponent<ILLMService>();
            if (llmService == null)
            {
                llmService = FindFirstObjectByType<OllamaClient>();
            }
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerRb = player.GetComponent<Rigidbody2D>();

            playerInventory = player.GetComponent<PlayerInventory>();
            if (playerInventory == null)
            {
                playerInventory = player.GetComponentInParent<PlayerInventory>();
            }

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

        if (playerInventory == null) playerInventory = FindObjectOfType<PlayerInventory>();

        // Kapı zaten açıldıysa altın şartı aranmaz
        int currentGold = playerInventory != null ? playerInventory.currentGold : 0;
        bool hasEnoughGold = isDoorOpened || (currentGold >= requiredGold);

        if (playerInputBox != null)
        {
            playerInputBox.gameObject.SetActive(isChatting && hasEnoughGold);
            playerInputBox.interactable = state && hasEnoughGold;
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

            if (hasEnoughGold)
            {
                if (isDoorOpened)
                {
                    ShowTextAnimated("Guard: Welcome back, traveler! You may pass freely.");
                }
                else
                {
                    ShowTextAnimated("Guard: Halt, traveler! State your business or offer a fine bribe to pass.");
                }

                if (playerInputBox != null) playerInputBox.ActivateInputField();
            }
            else
            {
                ShowTextAnimated($"Guard: You do not have enough gold! (Need {requiredGold} gold)");
            }
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
        if (string.IsNullOrWhiteSpace(playerMessage) || isWaitingForAI) return;

        if (playerInventory == null) playerInventory = FindObjectOfType<PlayerInventory>();

        // Kapı açılmadıysa ve altın yetersizse engel ol
        int currentGold = playerInventory != null ? playerInventory.currentGold : 0;
        if (!isDoorOpened && currentGold < requiredGold)
        {
            ShowTextAnimated($"Guard: You do not have enough gold!");
            return;
        }

        if (llmService == null)
        {
            ShowTextAnimated("Guard: (LLM servisi bulunamadı!)");
            return;
        }

        isWaitingForAI = true;

        if (playerInputBox != null)
        {
            playerInputBox.interactable = false;
        }

        ShowTextAnimated("Guard is thinking...");

        string prompt = "You are a greedy, dramatic, and humorous medieval fantasy guard NPC in a 2D game.\n" +
                        "The player is trying to convince you to open the door.\n" +
                        "Player's current gold in inventory: " + currentGold + ". (Collecting or having gold is completely optional, just a mechanic).\n" +
                        "Player's message: '" + playerMessage + "'\n\n" +
                        "Instructions:\n" +
                        "1. Be easily persuaded! If the player attempts any form of bribery, clever argument, or good roleplay, you should happily agree to let them pass.\n" +
                        "2. If you decide to let them pass, set 'action' to 'OPEN_DOOR'. Otherwise, set 'action' to 'NONE'.\n" +
                        "3. Write your 'dialogue' response in English, staying in character.\n\n" +
                        "CRITICAL: Output MUST be strictly valid JSON with this exact structure, with no extra text or markdown formatting:\n" +
                        "{\n" +
                        "  \"dialogue\": \"Your response in English here\",\n" +
                        "  \"action\": \"OPEN_DOOR\"\n" +
                        "}";

        llmService.AskAI(prompt, (rawResponse, isSuccess) => {
            if (playerInputBox != null)
            {
                playerInputBox.text = "";
                playerInputBox.interactable = true;
            }

            isWaitingForAI = false;

            if (!isSuccess)
            {
                ShowTextAnimated("Guard: (Sunucuya ulaşılamadı veya Ollama kapalı!) " + rawResponse);
                if (isChatting && playerInputBox != null) playerInputBox.ActivateInputField();
                return;
            }

            Debug.Log("[Ollama Raw Response]: " + rawResponse);

            bool shouldOpenDoor = false;
            string dialogueText = "";

            try
            {
                string cleanedResponse = rawResponse.Trim();
                if (cleanedResponse.Contains("```"))
                {
                    int startIndex = cleanedResponse.IndexOf("```");
                    int endIndex = cleanedResponse.LastIndexOf("```");
                    if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
                    {
                        string sub = cleanedResponse.Substring(startIndex + 3, endIndex - (startIndex + 3));
                        if (sub.StartsWith("json")) sub = sub.Substring(4);
                        cleanedResponse = sub.Trim();
                    }
                }
                int firstBrace = cleanedResponse.IndexOf('{');
                int lastBrace = cleanedResponse.LastIndexOf('}');
                if (firstBrace != -1 && lastBrace != -1 && lastBrace > firstBrace)
                {
                    cleanedResponse = cleanedResponse.Substring(firstBrace, (lastBrace - firstBrace) + 1);
                }

                LLMGuardData data = JsonUtility.FromJson<LLMGuardData>(cleanedResponse);

                if (data != null && !string.IsNullOrEmpty(data.dialogue))
                {
                    dialogueText = data.dialogue;
                    if (!string.IsNullOrEmpty(data.action) && data.action.Trim().ToUpper() == "OPEN_DOOR")
                    {
                        shouldOpenDoor = true;
                    }
                }
                else
                {
                    dialogueText = rawResponse;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[JSON Parse Fallback Triggered]: " + ex.Message);
                dialogueText = rawResponse;
            }

            // --- GENİŞLETİLMİŞ AKILLI KONTROL ---
            string upperPlayerMsg = playerMessage.ToUpper();
            string upperResponse = rawResponse.ToUpper();

            // Oyuncu mesajı açıkça kapının açılmaması yönündeyse (Don't, Do Not, Stop, No vb.)
            bool isPlayerRejecting = upperPlayerMsg.Contains("DON'T") || upperPlayerMsg.Contains("DO NOT") || upperPlayerMsg.Contains("STOP");

            if (!isPlayerRejecting)
            {
                // Eklenen yeni kabul kelimeleri: YOU'RE ON, DEAL, ACCEPTED, PASS, GO THROUGH, ENTER
                if (shouldOpenDoor || upperResponse.Contains("\"ACTION\": \"OPEN_DOOR\""))
                {
                    shouldOpenDoor = true;
                }
                else if (upperResponse.Contains("YOU MAY PASS") ||
                         upperResponse.Contains("I WILL OPEN") ||
                         upperResponse.Contains("YOU CAN ENTER") ||
                         upperResponse.Contains("YOU'RE ON") ||
                         upperResponse.Contains("DEAL") ||
                         upperResponse.Contains("ACCEPTED") ||
                         upperResponse.Contains("WHOOP"))
                {
                    shouldOpenDoor = true;
                }
            }
            else
            {
                shouldOpenDoor = false;
            }

            ShowTextAnimated("Guard: " + dialogueText);

            if (shouldOpenDoor)
            {
                OpenGuardDoor();
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
            Debug.Log("LLM Action: OPEN_DOOR triggered, door opened successfully!");

            isDoorOpened = true;

            if (playerInventory == null)
            {
                playerInventory = FindFirstObjectByType<PlayerInventory>();
            }

            if (playerInventory != null)
            {
                playerInventory.ResetGold();
            }
        }
    }
}