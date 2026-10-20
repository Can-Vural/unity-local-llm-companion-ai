using UnityEngine;
using UnityEngine.AI;

public class npc_movement : MonoBehaviour
{
    [SerializeField] Transform target;
    NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        agent.updateRotation = false; 
        agent.updateUpAxis = false; 
    }

    private void Update()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }
}