using UnityEngine;
using UnityEngine.AI;

public class npc_movement : MonoBehaviour
{
    [Header("Hedefler")]
    [SerializeField] private Transform target; // Player

    [Header("Mesafe Ayarları")]
    [SerializeField] private float followDistance = 1.5f;

    [Header("Sensör Ayarları (Raycast)")]
    [SerializeField] private float sensorDistance = 3f;
    [SerializeField] private LayerMask obstacleLayer;

    private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;

    public enum AIState { FollowPlayer, GoToZone, WaitingAtZone }
    private AIState currentState = AIState.FollowPlayer;

    // Dinamik Hedefler (Hardcode'u önleyen alanlar)
    private Transform currentZoneTarget;
    private GameObject currentDoorToOpen;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        SyncSpeedWithPlayer();
    }

    private void SyncSpeedWithPlayer()
    {
        if (target == null || agent == null) return;

        // Sadece aktif olan PlayerMovement script'ine bakar
        PlayerMovement playerMove = target.GetComponent<PlayerMovement>();
        if (playerMove != null)
        {
            agent.speed = playerMove.moveSpeed;
        }
    }

    private void Update()
    {
        // 1. AJAN SENSÖR MEKANİZMASI (Raycast Gözlem)
        Vector2 moveDirection = agent.desiredVelocity.normalized;
        if (moveDirection != Vector2.zero)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, sensorDistance, obstacleLayer);

            if (hit.collider != null)
                Debug.DrawRay(transform.position, moveDirection * hit.distance, Color.red);
            else
                Debug.DrawRay(transform.position, moveDirection * sensorDistance, Color.green);
        }

        // 2. DURUM MAKİNESİ (FSM)
        switch (currentState)
        {
            case AIState.FollowPlayer:
                if (target != null)
                {
                    agent.stoppingDistance = followDistance;
                    agent.SetDestination(target.position);
                }
                break;

            case AIState.GoToZone:
                if (currentZoneTarget != null)
                {
                    agent.stoppingDistance = 0.2f;
                    agent.SetDestination(currentZoneTarget.position);

                    if (!agent.pathPending && agent.remainingDistance <= 0.5f)
                    {
                        OpenDoor();
                        currentState = AIState.FollowPlayer;
                    }
                }
                break;

            case AIState.WaitingAtZone:
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
        Transform currentDestination = (currentState == AIState.FollowPlayer) ? target : currentZoneTarget;

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

    // DİNAMİK GÖREV TETİKLEYİCİSİ (Hangi sensöre basılırsa o hedefi alır)
    public void CommandGoToZone(Transform zoneTarget, GameObject doorToOpen)
    {
        currentZoneTarget = zoneTarget;
        currentDoorToOpen = doorToOpen;
        currentState = AIState.GoToZone;
    }

    private void OpenDoor()
    {
        if (currentDoorToOpen != null)
        {
            currentDoorToOpen.SetActive(false);
            Debug.Log($"{currentDoorToOpen.name} başarıyla açıldı!");
        }
    }

    public void ResetNPC(Vector3 startPos)
    {
        StopAllCoroutines();
        currentZoneTarget = null;
        currentDoorToOpen = null;
        currentState = AIState.FollowPlayer;

        if (agent != null)
        {
            agent.Warp(startPos);
        }
    }
}