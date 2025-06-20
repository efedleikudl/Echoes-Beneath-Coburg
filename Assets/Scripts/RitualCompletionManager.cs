// RitualWinChecker.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class RitualWinChecker : MonoBehaviour
{
    // The scene to switch to when the ritual is complete
    [SerializeField] string winSceneName = "Win";

    PlaceTarget crystal;
    PlaceTarget book;
    PlaceTarget skull;

    void Awake()
    {
        // Hard-wired object names from the hierarchy
        crystal = GameObject.Find("Pedestal_Crystal")?.GetComponent<PlaceTarget>();
        book = GameObject.Find("Pedestal_Book")?.GetComponent<PlaceTarget>();
        skull = GameObject.Find("Pedestal_Skull")?.GetComponent<PlaceTarget>();

        // Quick validation in case the names change later
        if (!crystal || !book || !skull)
            Debug.LogError("[RitualWinChecker] One or more pedestals couldn’t be found – double-check the names.");
    }

    void Update()
    {
        if (!crystal || !book || !skull) return;           // still missing references?
        if (!crystal.IsPlaced || !book.IsPlaced || !skull.IsPlaced) return;

        // All three pedestals have the correct prop → load the victory scene
        SceneManager.LoadScene(winSceneName);
    }
}
