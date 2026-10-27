using UnityEngine;

public class Spike : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Transform player;
    [SerializeField] private npc_movement npcScript;
    [SerializeField] private GameObject[] doors; // Tüm kapıları tutan dizi

    private Vector3 playerStartPos;
    private Vector3 npcStartPos;
    private Rigidbody2D playerRb;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            playerStartPos = player.position;
            playerRb = player.GetComponent<Rigidbody2D>() ?? player.GetComponentInChildren<Rigidbody2D>();
        }

        if (npcScript != null)
        {
            npcStartPos = npcScript.transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            ResetLevel();
        }
    }

    private void ResetLevel()
    {
        // 1. Oyuncuyu başlangıç pozisyonuna taşı ve hızını sıfırla
        if (player != null)
        {
            player.position = playerStartPos;
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
        }

        // 2. Yancı NPC'yi başlangıç konumuna taşı ve takibe döndür
        if (npcScript != null)
        {
            npcScript.ResetNPC(npcStartPos);
        }

        // 3. Açılmış olan TÜM kapıları tekrar aktif yap (kapat)
        if (doors != null)
        {
            foreach (GameObject door in doors)
            {
                if (door != null)
                {
                    door.SetActive(true);
                }
            }
        }

        Debug.Log("Dikenlere basıldı! Karakterler ve tüm kapılar sıfırlandı.");
    }
}