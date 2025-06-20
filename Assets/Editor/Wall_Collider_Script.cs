using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class MeshColliderSetter
{
    [MenuItem("Tools/Set Mesh Colliders on Walls and Doors")]
    public static void SetMeshCollidersOnWallsAndDoors()
    {
        // Ensure we're in Edit mode for proper saving
        if (Application.isPlaying)
        {
            Debug.LogWarning("⚠️ This tool should be used in Edit mode, not Play mode, to ensure changes are saved!");
        }

        int countWalls = 0;
        int countDoors = 0;

        // Record undo operation for the entire operation
        Undo.SetCurrentGroupName("Add Wall and Door Colliders");
        int undoGroup = Undo.GetCurrentGroup();

        // Find Wall_B to get reference mesh
        GameObject wallBObject = GameObject.Find("Wall_B");
        Mesh wallBMesh = null;
        if (wallBObject != null)
        {
            MeshFilter mf = wallBObject.GetComponent<MeshFilter>();
            if (mf != null)
            {
                wallBMesh = mf.sharedMesh;
                Debug.Log($"✅ Found Wall_B reference mesh: {wallBMesh.name}");
            }
            else
            {
                Debug.LogWarning("Wall_B found but no MeshFilter component exists!");
            }
        }
        else
        {
            Debug.LogWarning("No GameObject named 'Wall_B' found!");
        }

        // Process all GameObjects in the scene
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Skip prefab instances in Project view
            if (PrefabUtility.IsPartOfPrefabAsset(obj))
                continue;

            MeshCollider existingCollider = obj.GetComponent<MeshCollider>();

            // Process Wall_A and Wall_C objects
            if (obj.name.StartsWith("Wall_A") || obj.name.StartsWith("Wall_C"))
            {
                if (existingCollider == null)
                {
                    // Record undo for this specific object
                    Undo.RegisterCreatedObjectUndo(obj, "Add Wall Collider");

                    MeshCollider newCollider = Undo.AddComponent<MeshCollider>(obj);

                    // Set up the collider properly
                    MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        newCollider.sharedMesh = meshFilter.sharedMesh;
                        newCollider.convex = false; // Walls should be non-convex for better collision
                    }

                    // Mark object as dirty to ensure saving
                    EditorUtility.SetDirty(obj);
                    countWalls++;

                    Debug.Log($"✅ Added MeshCollider to {obj.name}");
                }
                else
                {
                    Debug.Log($"ℹ️ {obj.name} already has a MeshCollider");
                }
            }
            // Process Door objects
            else if (obj.name.Contains("Door"))
            {
                if (existingCollider == null)
                {
                    Undo.RegisterCreatedObjectUndo(obj, "Add Door Collider");
                    existingCollider = Undo.AddComponent<MeshCollider>(obj);
                }
                else
                {
                    // Record undo for modifying existing component
                    Undo.RecordObject(existingCollider, "Modify Door Collider");
                }

                // Set the Wall_B mesh for doors
                if (wallBMesh != null)
                {
                    existingCollider.sharedMesh = wallBMesh;
                    existingCollider.convex = false; // Doors should also be non-convex
                    EditorUtility.SetDirty(obj);
                    countDoors++;
                    Debug.Log($"✅ Set Wall_B mesh for door: {obj.name}");
                }
                else
                {
                    Debug.LogWarning($"❌ Could not set mesh for {obj.name} - Wall_B mesh not available");
                }
            }
        }

        // Collapse undo operations
        Undo.CollapseUndoOperations(undoGroup);

        // Force save the scene
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("💾 Scene and assets saved!");
        }

        Debug.Log($"🎯 MeshColliders processed: {countWalls} Wall(s) and {countDoors} Door(s).");

        // Show completion dialog
        if (countWalls > 0 || countDoors > 0)
        {
            EditorUtility.DisplayDialog(
                "Collider Setup Complete",
                $"Successfully added/updated colliders:\n• {countWalls} Walls\n• {countDoors} Doors\n\nChanges have been saved to the scene.",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "No Changes Made",
                "No new colliders were added. All objects may already have colliders.",
                "OK"
            );
        }
    }

    [MenuItem("Tools/Remove All Wall and Door Colliders")]
    public static void RemoveAllWallAndDoorColliders()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("⚠️ This tool should be used in Edit mode!");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "Remove Colliders",
            "Are you sure you want to remove all MeshColliders from walls and doors?",
            "Yes",
            "Cancel"
        );

        if (!confirm) return;

        int removedCount = 0;
        Undo.SetCurrentGroupName("Remove Wall and Door Colliders");

        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(obj))
                continue;

            if (obj.name.StartsWith("Wall_A") || obj.name.StartsWith("Wall_C") || obj.name.Contains("Door"))
            {
                MeshCollider collider = obj.GetComponent<MeshCollider>();
                if (collider != null)
                {
                    Undo.DestroyObjectImmediate(collider);
                    EditorUtility.SetDirty(obj);
                    removedCount++;
                }
            }
        }

        if (removedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
        }

        Debug.Log($"🗑️ Removed {removedCount} MeshColliders");
        EditorUtility.DisplayDialog("Removal Complete", $"Removed {removedCount} colliders", "OK");
    }

    [MenuItem("Tools/Validate Wall and Door Colliders")]
    public static void ValidateWallAndDoorColliders()
    {
        int wallsWithColliders = 0;
        int wallsWithoutColliders = 0;
        int doorsWithColliders = 0;
        int doorsWithoutColliders = 0;

        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(obj))
                continue;

            MeshCollider collider = obj.GetComponent<MeshCollider>();

            if (obj.name.StartsWith("Wall_A") || obj.name.StartsWith("Wall_C"))
            {
                if (collider != null)
                    wallsWithColliders++;
                else
                    wallsWithoutColliders++;
            }
            else if (obj.name.Contains("Door"))
            {
                if (collider != null)
                    doorsWithColliders++;
                else
                    doorsWithoutColliders++;
            }
        }

        string report = $"Collider Validation Report:\n\n" +
                       $"Walls:\n" +
                       $"• With colliders: {wallsWithColliders}\n" +
                       $"• Without colliders: {wallsWithoutColliders}\n\n" +
                       $"Doors:\n" +
                       $"• With colliders: {doorsWithColliders}\n" +
                       $"• Without colliders: {doorsWithoutColliders}";

        Debug.Log(report);
        EditorUtility.DisplayDialog("Validation Report", report, "OK");
    }

    // Context menu for individual objects
    [MenuItem("CONTEXT/GameObject/Add Wall Collider", false, 0)]
    public static void AddWallColliderToSelected(MenuCommand command)
    {
        GameObject obj = command.context as GameObject;
        if (obj != null && obj.GetComponent<MeshCollider>() == null)
        {
            Undo.AddComponent<MeshCollider>(obj);
            EditorUtility.SetDirty(obj);
            Debug.Log($"Added MeshCollider to {obj.name}");
        }
    }

    [MenuItem("CONTEXT/GameObject/Add Wall Collider", true)]
    public static bool ValidateAddWallCollider(MenuCommand command)
    {
        GameObject obj = command.context as GameObject;
        return obj != null && obj.GetComponent<MeshCollider>() == null;
    }
}