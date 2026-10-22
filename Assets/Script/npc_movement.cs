using UnityEngine;
using UnityEngine.AI;

public class npc_movement : MonoBehaviour
{
    [SerializeField] private Transform target;
    private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

       
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        if (target == null) return;

 
        agent.SetDestination(target.position);


        float relativePositionX = target.position.x - transform.position.x;

        if (relativePositionX > 0.1f)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = false;
        }
        else if (relativePositionX < -0.1f)
        {
            if (spriteRenderer != null) spriteRenderer.flipX = true;
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }
}