using UnityEngine;

public class Zone1Sensor : MonoBehaviour
{
    [SerializeField] private npc_movement npcScript;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hem çarpan alt objeye hem de en üstteki ana objeye bakıyoruz
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            Debug.Log("SENSÖR ÇALIŞTI! Player kırmızı alana girdi.");

            if (npcScript != null)
            {
                npcScript.TriggerZone1();
            }
            else
            {
                Debug.LogError("HATA: Zone1Sensor içindeki NpcScript alanı boş!");
            }
        }
    }
}