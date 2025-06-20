using UnityEngine;
using UnityEngine.SceneManagement;

public class SpiderAttackTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // If the object entering the trigger is the player, switch scenes
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("Screamer", LoadSceneMode.Single);
        }
    }
}
