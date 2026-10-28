using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Coin Test]: Temas eden obje -> {other.name}");

        // Oyuncuyu veya ebeveynini kontrol eder
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = other.GetComponentInParent<PlayerInventory>();
        }

        if (inventory != null)
        {
            inventory.AddGold(coinValue);
            Destroy(gameObject); // Altını sahneden siler
        }
        else
        {
            Debug.LogWarning("[Coin Test]: Temas gerçekleşti ama PlayerInventory bulunamadı!");
        }
    }
}