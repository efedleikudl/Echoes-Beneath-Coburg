using UnityEngine;

[ExecuteInEditMode]
public class ModularRoomManager : MonoBehaviour
{
    [Header("Wall Groups")]
    public GameObject[] topWalls;
    public GameObject[] bottomWalls;
    public GameObject[] leftWalls;
    public GameObject[] rightWalls;

    [Header("Editor Toggles")]
    public bool showTop = true;
    public bool showBottom = true;
    public bool showLeft = true;
    public bool showRight = true;

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateWallVisibility(topWalls, showTop);
            UpdateWallVisibility(bottomWalls, showBottom);
            UpdateWallVisibility(leftWalls, showLeft);
            UpdateWallVisibility(rightWalls, showRight);
        }
    }

    private void UpdateWallVisibility(GameObject[] walls, bool visible)
    {
        if (walls == null) return;
        foreach (var wall in walls)
        {
            if (wall != null)
                wall.SetActive(visible);
        }
    }
}
