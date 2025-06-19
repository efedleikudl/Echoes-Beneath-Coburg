using UnityEngine;
using UnityEngine.AI;

public class SpiderAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public Animator animator;

    [Header("Speed Settings")]
    public float baseSpeed = 3.5f;
    public float patrolSpeedMultiplier = 0.6f;
    public float chaseSpeedMultiplier = 1.2f;

    private NavMeshAgent agent;
    private Vector3 lastKnownPlayerPos;
    private float huntTimer = 0f;
    private float searchTimer = 0f;
    private bool isHunting = false;
    private float losePlayerTimer = 0f;
    private Vector3 patrolTarget;
    private float patrolTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Make spider movement more erratic
        agent.angularSpeed = 240f;
        agent.acceleration = 8f;

        // Start with a random patrol target
        SetNewPatrolTarget();
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // Line of sight check for more realistic detection
        bool hasLineOfSight = false;
        RaycastHit hit;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer, out hit, detectionRange))
        {
            hasLineOfSight = hit.transform == player;
        }

        // Determine movement state
        Vector3 localVelocity = transform.InverseTransformDirection(agent.desiredVelocity);
        bool isTurningInPlace = Mathf.Abs(localVelocity.x) > 0.1f && localVelocity.z < 0.05f;
        bool isMovingForward = agent.velocity.magnitude > 0.1f;
        bool isWalking = isMovingForward || isTurningInPlace;

        // Enhanced detection - spider can sense player globally but needs line of sight for precise tracking
        if (distance < detectionRange && hasLineOfSight)
        {
            // Player detected with line of sight
            lastKnownPlayerPos = player.position;
            isHunting = true;
            huntTimer = Random.Range(5f, 10f); // Remember player for 5-10 seconds
            losePlayerTimer = 0f;

            if (distance < attackRange)
            {
                // Attack mode
                agent.ResetPath();
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttacking", true);
            }
            else
            {
                // Intelligent chase - sometimes flank, sometimes direct approach
                if (Random.Range(0f, 1f) > 0.7f && searchTimer <= 0f)
                {
                    // 30% chance to try flanking
                    Vector3 flankPos = player.position + Quaternion.Euler(0, Random.Range(-90f, 90f), 0) *
                                     (player.position - transform.position).normalized * Random.Range(3f, 5f);
                    agent.SetDestination(flankPos);
                    searchTimer = 2f;
                }
                else
                {
                    // Direct approach but with varying speed
                    agent.speed = baseSpeed * Random.Range(0.8f, chaseSpeedMultiplier);
                    agent.SetDestination(player.position);
                }

                animator.SetBool("isWalking", isWalking);
                animator.SetBool("isAttacking", false);
            }
        }
        else if (isHunting && huntTimer > 0f)
        {
            // Lost sight but still hunting - search behavior
            huntTimer -= Time.deltaTime;
            searchTimer -= Time.deltaTime;

            // Move to last known position with some randomness
            if (Vector3.Distance(transform.position, lastKnownPlayerPos) > 2f)
            {
                if (losePlayerTimer <= 0f)
                {
                    // Add randomness to search pattern
                    Vector3 searchPos = lastKnownPlayerPos + Random.insideUnitSphere * 5f;
                    searchPos.y = transform.position.y;
                    agent.SetDestination(searchPos);
                    losePlayerTimer = Random.Range(1f, 3f);
                }
                losePlayerTimer -= Time.deltaTime;
            }
            else
            {
                // Reached last known position, search around
                if (searchTimer <= 0f)
                {
                    Vector3 searchDirection = Random.insideUnitSphere * Random.Range(3f, 8f);
                    searchDirection.y = 0;
                    agent.SetDestination(transform.position + searchDirection);
                    searchTimer = Random.Range(2f, 4f);
                }
            }

            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isAttacking", false);

            // Occasionally stop and "listen"
            if (Random.Range(0f, 1f) > 0.98f)
            {
                agent.ResetPath();
                searchTimer = 1f;
            }
        }
        else
        {
            // Patrol mode - roam the map looking for player
            isHunting = false;
            agent.speed = baseSpeed * Random.Range(0.5f, patrolSpeedMultiplier);

            // Global awareness - spider can sense player anywhere but moves towards general area
            if (Random.Range(0f, 1f) > 0.95f) // 5% chance per frame to sense player
            {
                // Move towards player's location with minimal offset
                Vector3 huntDirection = (player.position - transform.position).normalized;
                float randomAngle = Random.Range(-10f, 10f); // Much smaller angle deviation
                huntDirection = Quaternion.Euler(0, randomAngle, 0) * huntDirection;

                // Move very close to player's actual position
                Vector3 huntTarget = player.position + Random.insideUnitSphere * Random.Range(1f, 3f);
                huntTarget.y = transform.position.y;
                agent.SetDestination(huntTarget);
                patrolTimer = Random.Range(5f, 8f);
            }
            else if (patrolTimer <= 0f || Vector3.Distance(transform.position, patrolTarget) < 2f)
            {
                // Regular patrol
                SetNewPatrolTarget();
                patrolTimer = Random.Range(4f, 8f);
            }

            patrolTimer -= Time.deltaTime;

            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isAttacking", false);

            // Occasionally pause during patrol (adds creepiness)
            if (Random.Range(0f, 1f) > 0.99f)
            {
                agent.ResetPath();
                patrolTimer = Random.Range(0.5f, 2f);
            }
        }
    }

    void SetNewPatrolTarget()
    {
        // Create patrol points across the map
        NavMeshHit hit;
        Vector3 randomDirection = Random.insideUnitSphere * 30f;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        if (NavMesh.SamplePosition(randomDirection, out hit, 30f, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
            agent.SetDestination(patrolTarget);
        }
    }
}