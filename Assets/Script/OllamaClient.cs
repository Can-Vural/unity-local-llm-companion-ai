using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public class OllamaClient : MonoBehaviour, ILLMService
{
    [Header("Ollama Settings")]
    [SerializeField] private string modelName = "llama3.2";
    [SerializeField] private string apiUrl = "http://localhost:11434/api/generate";
    [SerializeField] private int timeoutSeconds = 10;

    [Header("Warm-up Settings")]
    [SerializeField] private bool autoWarmupOnStart = true;
    [SerializeField] private string warmupPrompt = "hi"; 

    [Serializable]
    private class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream;
        public string format;
    }

    [Serializable]
    private class OllamaResponse
    {
        public string response;
    }

    private void Start()
    {
        if (autoWarmupOnStart)
        {
            StartCoroutine(WarmupRoutine());
        }
    }

    private IEnumerator WarmupRoutine()
    {
        Debug.Log("[OllamaClient]: Modellin belleğe yüklenmesi için (warm-up) isteği gönderiliyor...");

        OllamaRequest requestObj = new OllamaRequest
        {
            model = modelName,
            prompt = warmupPrompt,
            stream = false,
            format = "json"
        };

        string jsonBody = JsonUtility.ToJson(requestObj);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 30;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[OllamaClient]: Isınma isteği başarılı! Model hazır ve belleğe yüklendi.");
            }
            else
            {
                Debug.LogWarning("[OllamaClient]: Isınma isteğinde uyarı: " + www.error);
            }
        }
    }

    public void AskAI(string promptText, Action<string, bool> onResponseReceived)
    {
        StartCoroutine(PostRequest(promptText, onResponseReceived));
    }

    private IEnumerator PostRequest(string prompt, Action<string, bool> callback)
    {
        OllamaRequest requestObj = new OllamaRequest
        {
            model = modelName,
            prompt = prompt,
            stream = false,
            format = "json"
        };

        string jsonBody = JsonUtility.ToJson(requestObj);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = timeoutSeconds;

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[Ollama Error]: " + www.error);
                callback?.Invoke("Go away!", false);
            }
            else
            {
                try
                {
                    string jsonResponse = www.downloadHandler.text;
                    OllamaResponse res = JsonUtility.FromJson<OllamaResponse>(jsonResponse);
                    callback?.Invoke(res.response, true);
                }
                catch (Exception e)
                {
                    Debug.LogError("[JSON Parse Error]: " + e.Message);
                    callback?.Invoke("Guard mumbled something incomprehensible.", false);
                }
            }
        }
    }
}