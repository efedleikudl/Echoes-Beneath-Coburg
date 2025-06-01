using UnityEngine;

public class KeyItem : MonoBehaviour, IInteractable
{
    public string keyID;

    public void Interact()
    {
        Debug.Log("Schlüssel aufgenommen: " + keyID);
        PlayerInventory.AddKey(keyID);
        Destroy(gameObject);
    }
}

