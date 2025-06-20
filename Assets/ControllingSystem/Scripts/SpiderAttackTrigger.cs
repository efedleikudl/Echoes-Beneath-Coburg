using UnityEngine;

public class SpiderAttackTrigger : MonoBehaviour
{
    private SpiderAI spiderAI;
    private Animator animator;

    void Start()
    {
        // Get components from parent
        spiderAI = GetComponentInParent<SpiderAI>();
        animator = GetComponentInParent<Animator>();

        // Ensure this trigger has proper tag
        if (!transform.parent.CompareTag("Spider"))
        {
            transform.parent.tag = "Spider";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if this is the player and spider is attacking
        if (other.CompareTag("Player") && animator != null && animator.GetBool("isAttacking"))
        {
            PlayerDeathScreener deathScript = other.GetComponent<PlayerDeathScreener>();
            if (deathScript != null)
            {
                deathScript.TriggerDeath();
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Continue checking during attack animation
        if (other.CompareTag("Player") && animator != null && animator.GetBool("isAttacking"))
        {
            PlayerDeathScreener deathScript = other.GetComponent<PlayerDeathScreener>();
            if (deathScript != null && !deathScript.isDead)
            {
                deathScript.TriggerDeath();
            }
        }
    }
}