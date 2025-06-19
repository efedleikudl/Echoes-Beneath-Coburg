using UnityEngine;

using System.Collections.Generic;

public static class PlayerInventory
{
    public static string CurrentItem { get; private set; } = null;
    private static GameObject carriedObject = null;
    private static HashSet<string> placedItems = new();
    private static readonly List<string> allItems = new() { "Skull", "Book", "Crystal" };

    public static bool IsCarrying => CurrentItem != null;


    public static void Place(string itemName)
    {
        if (CurrentItem == itemName && !placedItems.Contains(itemName))
        {
            placedItems.Add(itemName);
            CurrentItem = null;
            carriedObject = null;

            InventoryUI.instance?.UpdateUI(); // ✅ Update nach Platzieren

            if (placedItems.Count == allItems.Count)
            {
                Debug.Log("🎉 Victory! Alle Items platziert!");
                // Victory-Logik hier
            }
        }
    }

    public static List<string> GetRemainingItems()
    {
        List<string> remaining = new(allItems);
        foreach (var item in placedItems)
            remaining.Remove(item);
        return remaining;
    }



    public static GameObject GetCarriedObject()
    {
        return carriedObject;
    }

    public static void PickUp(GameObject itemObj, string itemName)
    {
        if (IsCarrying) return;
        CurrentItem = itemName;
        carriedObject = itemObj;
        carriedObject.SetActive(false);
        InventoryUI.instance?.UpdateUI(); // Update sofort nach Pickup
    }

    public static void Clear()
    {
        CurrentItem = null;
        carriedObject = null;
    }
}


