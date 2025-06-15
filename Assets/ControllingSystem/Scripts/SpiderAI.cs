using UnityEngine;
using UnityEngine.AI;

public class SpiderAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public Animator animator;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // Determine if spider is moving or turning
        Vector3 localVelocity = transform.InverseTransformDirection(agent.desiredVelocity);
        bool isTurningInPlace = Mathf.Abs(localVelocity.x) > 0.1f && localVelocity.z < 0.05f;
        bool isMovingForward = agent.velocity.magnitude > 0.1f;

        bool isWalking = isMovingForward || isTurningInPlace;

        if (distance < detectionRange)
        {
            if (distance < attackRange)
            {
                // Attack mode
                agent.ResetPath();
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttacking", true);
            }
            else
            {
                // Chase mode
                agent.SetDestination(player.position);
                animator.SetBool("isWalking", isWalking);
                animator.SetBool("isAttacking", false);
            }
        }
        else
        {
            // Idle mode
            agent.ResetPath();
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", false);
        }
    }
}