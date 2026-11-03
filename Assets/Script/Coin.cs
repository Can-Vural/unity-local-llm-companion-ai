using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Coin Test]: Temas eden obje -> {other.name}");

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = other.GetComponentInParent<PlayerInventory>();
        }

        if (inventory != null)
        {
            inventory.AddGold(coinValue);

            if (audioSource != null && audioSource.clip != null)
            {
                AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
            }

            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("[Coin Test]: Temas gerçekleşti ama PlayerInventory bulunamadı!");
        }
    }
}