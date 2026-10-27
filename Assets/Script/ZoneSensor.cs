using UnityEngine;

public class ZoneSensor : MonoBehaviour
{
    [Header("Yapılandırma")]
    [SerializeField] private npc_movement npcScript;
    [SerializeField] private Transform npcTargetZone; // Yancı NPC'nin gitmesi gereken alan
    [SerializeField] private GameObject doorToOpen;   // Açılacak ilgili kapı

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name} tetiklendi! Yancı NPC hedef alana yönlendiriliyor.");

            if (npcScript != null && npcTargetZone != null)
            {
                // Yancıya bağımsız olarak alan ve kapı bilgisini dinamik gönderir
                npcScript.CommandGoToZone(npcTargetZone, doorToOpen);
            }
            else
            {
                Debug.LogError("HATA: ZoneSensor üzerindeki npcScript veya npcTargetZone alanı boş!");
            }
        }
    }
}
