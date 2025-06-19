using UnityEngine;

public class PlaceTarget : MonoBehaviour
{
    public string expectedItem;
    public Transform placePoint;

    private bool isPlaced = false;

    public void TryPlace()
    {
        if (isPlaced) return;

        if (PlayerInventory.IsCarrying && PlayerInventory.CurrentItem == expectedItem)
        {
            GameObject carried = PlayerInventory.GetCarriedObject();
            if (carried != null)
            {
                carried.SetActive(true);
                carried.transform.SetParent(null);
                carried.transform.position = placePoint.position;
                carried.transform.rotation = placePoint.rotation;

                PlayerInventory.Place(expectedItem);
                isPlaced = true;

                Debug.Log($"{expectedItem} korrekt platziert.");
            }
        }
        else
        {
            Debug.LogWarning("Falsches Item für dieses Podest!");
        }
    }
}
