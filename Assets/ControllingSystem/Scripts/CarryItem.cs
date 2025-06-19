using UnityEngine;

public class CarryItem : MonoBehaviour
{
    public string itemName;

    public void PickUp()
    {
        if (!PlayerInventory.IsCarrying)
        {
            PlayerInventory.PickUp(this.gameObject, itemName);  // ✅ Beide Argumente übergeben
            gameObject.SetActive(false);
        }
    }
}



