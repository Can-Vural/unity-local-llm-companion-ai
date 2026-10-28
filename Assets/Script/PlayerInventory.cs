using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [Header("UI Referansı (Boş Bırakırsanız Otomatik Bulur)")]
    [SerializeField] private TMP_Text goldTextTMP;
    [SerializeField] private Text goldTextLegacy;

    public int currentGold = 0;

    private void Start()
    {
        // Inspector'dan atanmadıysa "GoldText" isimli objeyi otomatik bulur
        if (goldTextTMP == null && goldTextLegacy == null)
        {
            GameObject goldObj = GameObject.Find("GoldText");
            if (goldObj != null)
            {
                goldTextTMP = goldObj.GetComponent<TMP_Text>();
                goldTextLegacy = goldObj.GetComponent<Text>();
            }
        }

        UpdateUI();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
        Debug.Log($"[Envanter]: Altın eklendi! Yeni Miktar: {currentGold}");
    }

    private void UpdateUI()
    {
        if (goldTextTMP != null)
        {
            goldTextTMP.text = $"Altın: {currentGold}";
        }
        else if (goldTextLegacy != null)
        {
            goldTextLegacy.text = $"Altın: {currentGold}";
        }
    }
    public void ResetGold()
    {
        currentGold = 0;
        UpdateUI();
        Debug.Log("[Envanter]: Rüşvet verildi, altınlar sıfırlandı!");
    }
}