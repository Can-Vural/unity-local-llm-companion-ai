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
                callback?.Invoke("Guard is staring blankly at you.", false);
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