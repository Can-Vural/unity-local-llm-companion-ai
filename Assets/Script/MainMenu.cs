using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panelleri")]
    [SerializeField] private GameObject creditsPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çıkış yapıldı."); 
        Application.Quit();
    }
}