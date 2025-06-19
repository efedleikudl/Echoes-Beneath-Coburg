using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class InventoryUI : MonoBehaviour
{
    public TMP_Text carryingText;
    public TMP_Text remainingText;
    public static InventoryUI instance;

    private void Start()
    {
        if (carryingText == null || remainingText == null)
        {
            Debug.LogError("InventoryUI: Text components are not assigned.");
            return;
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (carryingText == null || remainingText == null)
        {
            Debug.LogError("InventoryUI: Text components not assigned!");
            return;
        }

        string current = PlayerInventory.CurrentItem ?? "";
        string remaining = string.Join(", ", PlayerInventory.GetRemainingItems());

        carryingText.text = $"Carrying: {current}";
        remainingText.text = $"Remaining: {remaining}";
    }


    void Awake()
    {
        instance = this;
        UpdateUI();
    }
    
    

}

