using UnityEngine;

public class ZoneSensor : MonoBehaviour
{
    [Header("Yapılandırma")]
    [SerializeField] private npc_movement npcScript;
    [SerializeField] private Transform npcTargetZone; 
    [SerializeField] private GameObject doorToOpen;   

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name} tetiklendi! NPC hedef alana yönlendiriliyor.");

            if (npcScript != null && npcTargetZone != null)
            {
                npcScript.CommandGoToZone(npcTargetZone, doorToOpen);
            }
            else
            {
                Debug.LogError("HATA: ZoneSensor üzerindeki npcScript veya npcTargetZone alanı boş!");
            }
        }
    }
}
