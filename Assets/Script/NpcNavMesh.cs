using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Transform target;
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
}