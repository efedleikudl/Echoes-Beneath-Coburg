using UnityEngine;

using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static int itemsPlaced = 0;

    public static void ReportItemPlaced(string id)
    {
        itemsPlaced++;
        Debug.Log($"Placed: {id}");

        if (itemsPlaced >= 3)
        {
            Debug.Log("Victory! You survived!");
            // Hier Victory-UI anzeigen oder Szenenwechsel etc.
        }
    }
}

