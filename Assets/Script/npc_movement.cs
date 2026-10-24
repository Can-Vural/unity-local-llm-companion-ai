using UnityEngine;
using UnityEngine.AI;

public class npc_movement : MonoBehaviour
{
    [Header("Hedefler ve Kapı")]
    [SerializeField] private Transform target;        // Player
    [SerializeField] private Transform zone2Target;   // KirmiziAlan_2
    [SerializeField] private GameObject door;         // Kapi

    [Header("Mesafe Ayarları")]
    [SerializeField] private float followDistance = 1.5f; // Oyuncunun kaç adım gerisinde duracağı mesafe

    [Header("Sensör Ayarları (Raycast)")]
    [SerializeField] private float sensorDistance = 3f; // Sensör menzili
    [SerializeField] private LayerMask obstacleLayer;    // Engeller katmanı

    private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;

    public enum AIState { FollowPlayer, GoToZone2, WaitingAtZone2 }
    private AIState currentState = AIState.FollowPlayer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        // 1. AJAN SENSÖR MEKANİZMASI (Raycast Gözlem)
        Vector2 moveDirection = agent.desiredVelocity.normalized;
        if (moveDirection != Vector2.zero)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, sensorDistance, obstacleLayer);

            if (hit.collider != null)
            {
                Debug.DrawRay(transform.position, moveDirection * hit.distance, Color.red);
            }
            else
            {
                Debug.DrawRay(transform.position, moveDirection * sensorDistance, Color.green);
            }
        }

        // 2. DURUM MAKİNESİ (FSM)
        switch (currentState)
        {
            case AIState.FollowPlayer:
                if (target != null)
                {
                    // Oyuncuyu takip ederken geride durması için mesafe sınırını ayarla
                    agent.stoppingDistance = followDistance;
                    agent.SetDestination(target.position);
                }
                break;

            case AIState.GoToZone2:
                if (zone2Target != null)
                {
                    // Kırmızı Alana giderken hedefe tam yanaşabilmesi için durma mesafesini sıfırla
                    agent.stoppingDistance = 0.2f;
                    agent.SetDestination(zone2Target.position);

                    if (!agent.pathPending && agent.remainingDistance <= 0.5f)
                    {
                        OpenDoor();
                        currentState = AIState.FollowPlayer;
                    }
                }
                break;

            case AIState.WaitingAtZone2:
                break;
        }

        HandleRotation();
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }

    private void HandleRotation()
    {
        Transform currentDestination = (currentState == AIState.FollowPlayer) ? target : zone2Target;

        if (currentDestination != null)
        {
            float relativeX = currentDestination.position.x - transform.position.x;

            if (relativeX > 0.1f)
            {
                if (spriteRenderer != null) spriteRenderer.flipX = false;
            }
            else if (relativeX < -0.1f)
            {
                if (spriteRenderer != null) spriteRenderer.flipX = true;
            }
        }
    }

    public void TriggerZone1()
    {
        if (currentState == AIState.FollowPlayer)
        {
            currentState = AIState.GoToZone2;
        }
    }

    private void OpenDoor()
    {
        if (door != null)
        {
            door.SetActive(false);
            Debug.Log("Kapı Açıldı! LLM Diyaloğu Başlayabilir.");
        }
    }
}