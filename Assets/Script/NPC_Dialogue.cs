using UnityEngine;
using TMPro;

public class GuardNPC : MonoBehaviour
{
    [Header("Ollama and UI References")]
    [SerializeField] private OllamaClient ollamaManager;
    [SerializeField] private TMP_InputField playerInputBox;
    [SerializeField] private TextMeshProUGUI guardDialogueText;

    [Header("Settings")]
    [SerializeField] private float interactionRange = 3f;
    private Transform playerTransform;
    private bool isPlayerNearby = false;
    private bool isChatting = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (playerInputBox != null) playerInputBox.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerNearby = distance <= interactionRange;

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            isChatting = !isChatting;
            if (playerInputBox != null) playerInputBox.gameObject.SetActive(isChatting);

            if (isChatting)
            {
                guardDialogueText.text = "Guard: Halt, traveler! Who are you to dare approach this place?";
                playerInputBox.ActivateInputField();
            }
        }
    }

    public void OnPlayerSubmit(string playerMessage)
    {
        if (string.IsNullOrEmpty(playerMessage)) return;

        guardDialogueText.text = "Guard is thinking...";

        string prompt = $"You are a strict, suspicious, and stubborn guard NPC protecting a gate in a 2D platformer game. The player said to you: '{playerMessage}'. Give a short, clear, and firm response matching the guard persona, in English.";

        ollamaManager.AskAI(prompt, (response) => {
            guardDialogueText.text = "Guard: " + response;
            playerInputBox.text = "";
            playerInputBox.ActivateInputField();
        });
    }
}