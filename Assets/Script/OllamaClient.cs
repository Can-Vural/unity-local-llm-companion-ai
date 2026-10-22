using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class OllamaClient : MonoBehaviour
{
    [Header("Ollama Settings")]
    [SerializeField] private string modelName = "llama3.2";
    private string apiUrl = "http://localhost:11434/api/generate";

    [System.Serializable]
    private class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream;
    }

    [System.Serializable]
    private class OllamaResponse
    {
        public string response;
    }

    public void AskAI(string promptText, System.Action<string> onResponseReceived)
    {
        StartCoroutine(PostRequest(promptText, onResponseReceived));
    }

    private IEnumerator PostRequest(string prompt, System.Action<string> callback)
    {
        OllamaRequest requestObj = new OllamaRequest
        {
            model = modelName,
            prompt = prompt,
            stream = false
        };

        string jsonBody = JsonUtility.ToJson(requestObj);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest www = new UnityWebRequest(apiUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Ollama Connection Error: " + www.error);
                callback?.Invoke("Could not establish connection.");
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                OllamaResponse res = JsonUtility.FromJson<OllamaResponse>(jsonResponse);
                callback?.Invoke(res.response);
            }
        }
    }
}