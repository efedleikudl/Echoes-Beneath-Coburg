using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime component that ensures wall and door colliders are present when the scene loads.
/// This serves as a backup in case the editor script changes aren't properly saved.
/// </summary>
public class RuntimeColliderManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Automatically add missing colliders on scene start")]
    public bool autoAddCollidersOnStart = true;

    [Tooltip("Name of the Wall_B object to use as mesh reference for doors")]
    public string wallBReferenceName = "Wall_B";

    [Tooltip("Prefixes for wall objects that need colliders")]
    public string[] wallPrefixes = { "Wall_A", "Wall_C" };

    [Tooltip("Substring that identifies door objects")]
    public string doorIdentifier = "Door";

    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebugMessages = true;

    [Tooltip("Show collider statistics in inspector")]
    public bool showStatistics = false;

    [Header("Statistics (Read Only)")]
    [SerializeField] private int wallsWithColliders = 0;
    [SerializeField] private int wallsWithoutColliders = 0;
    [SerializeField] private int doorsWithColliders = 0;
    [SerializeField] private int doorsWithoutColliders = 0;
    [SerializeField] private bool wallBMeshFound = false;

    private Mesh wallBMesh;
    private static RuntimeColliderManager instance;

    public static RuntimeColliderManager Instance => instance;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (autoAddCollidersOnStart)
        {
            ValidateAndAddColliders();
        }

        if (showStatistics)
        {
            UpdateStatistics();
        }
    }

    /// <summary>
    /// Main method to validate and add missing colliders
    /// </summary>
    public void ValidateAndAddColliders()
    {
        DebugLog("🔍 Starting collider validation...");

        // Get reference mesh from Wall_B
        FindWallBMesh();

        // Process all objects in scene
        ProcessSceneObjects();

        DebugLog("✅ Collider validation complete!");
    }

    private void FindWallBMesh()
    {
        GameObject wallBObject = GameObject.Find(wallBReferenceName);

        if (wallBObject != null)
        {
            MeshFilter meshFilter = wallBObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                wallBMesh = meshFilter.sharedMesh;
                wallBMeshFound = true;
                DebugLog($"📦 Found reference mesh: {wallBMesh.name}");
            }
            else
            {
                DebugLogWarning($"❌ {wallBReferenceName} found but has no valid mesh!");
                wallBMeshFound = false;
            }
        }
        else
        {
            DebugLogWarning($"❌ Could not find {wallBReferenceName} in scene!");
            wallBMeshFound = false;
        }
    }

    private void ProcessSceneObjects()
    {
        // Find all GameObjects in scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        int wallsProcessed = 0;
        int doorsProcessed = 0;

        foreach (GameObject obj in allObjects)
        {
            // Skip destroyed or invalid objects
            if (obj == null) continue;

            // Check if this is a wall object
            bool isWall = false;
            foreach (string prefix in wallPrefixes)
            {
                if (obj.name.StartsWith(prefix))
                {
                    isWall = true;
                    break;
                }
            }

            if (isWall)
            {
                if (ProcessWallObject(obj))
                    wallsProcessed++;
            }
            // Check if this is a door object
            else if (obj.name.Contains(doorIdentifier))
            {
                if (ProcessDoorObject(obj))
                    doorsProcessed++;
            }
        }

        if (wallsProcessed > 0 || doorsProcessed > 0)
        {
            DebugLog($"🎯 Added colliders to {wallsProcessed} walls and {doorsProcessed} doors");
        }
        else
        {
            DebugLog("ℹ️ All objects already have proper colliders");
        }
    }

    private bool ProcessWallObject(GameObject obj)
    {
        MeshCollider collider = obj.GetComponent<MeshCollider>();

        if (collider == null)
        {
            // Add MeshCollider
            collider = obj.AddComponent<MeshCollider>();

            // Set mesh from object's MeshFilter if available
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                collider.sharedMesh = meshFilter.sharedMesh;
            }

            collider.convex = false; // Walls should be non-convex

            DebugLog($"➕ Added MeshCollider to wall: {obj.name}");
            return true;
        }

        return false;
    }

    private bool ProcessDoorObject(GameObject obj)
    {
        MeshCollider collider = obj.GetComponent<MeshCollider>();
        bool wasAdded = false;

        if (collider == null)
        {
            collider = obj.AddComponent<MeshCollider>();
            wasAdded = true;
        }

        // Set Wall_B mesh for doors (if available)
        if (wallBMesh != null)
        {
            collider.sharedMesh = wallBMesh;
            collider.convex = false;

            if (wasAdded)
                DebugLog($"➕ Added MeshCollider with Wall_B mesh to door: {obj.name}");
        }
        else
        {
            DebugLogWarning($"⚠️ Added MeshCollider to {obj.name} but couldn't set Wall_B mesh");
        }

        return wasAdded;
    }

    /// <summary>
    /// Update statistics for inspector display
    /// </summary>
    public void UpdateStatistics()
    {
        wallsWithColliders = 0;
        wallsWithoutColliders = 0;
        doorsWithColliders = 0;
        doorsWithoutColliders = 0;

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;

            bool hasCollider = obj.GetComponent<MeshCollider>() != null;

            // Check walls
            bool isWall = false;
            foreach (string prefix in wallPrefixes)
            {
                if (obj.name.StartsWith(prefix))
                {
                    isWall = true;
                    break;
                }
            }

            if (isWall)
            {
                if (hasCollider)
                    wallsWithColliders++;
                else
                    wallsWithoutColliders++;
            }
            // Check doors
            else if (obj.name.Contains(doorIdentifier))
            {
                if (hasCollider)
                    doorsWithColliders++;
                else
                    doorsWithoutColliders++;
            }
        }
    }

    /// <summary>
    /// Force refresh all colliders (removes and re-adds them)
    /// </summary>
    public void RefreshAllColliders()
    {
        DebugLog("🔄 Refreshing all colliders...");

        // Remove existing colliders
        RemoveAllColliders();

        // Add them back
        ValidateAndAddColliders();

        UpdateStatistics();
    }

    /// <summary>
    /// Remove all wall and door colliders
    /// </summary>
    public void RemoveAllColliders()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int removedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;

            // Check if this is a wall or door
            bool isTarget = false;

            foreach (string prefix in wallPrefixes)
            {
                if (obj.name.StartsWith(prefix))
                {
                    isTarget = true;
                    break;
                }
            }

            if (!isTarget && obj.name.Contains(doorIdentifier))
                isTarget = true;

            if (isTarget)
            {
                MeshCollider collider = obj.GetComponent<MeshCollider>();
                if (collider != null)
                {
                    if (Application.isPlaying)
                        Destroy(collider);
                    else
                        DestroyImmediate(collider);
                    removedCount++;
                }
            }
        }

        DebugLog($"🗑️ Removed {removedCount} colliders");
    }

    private void DebugLog(string message)
    {
        if (showDebugMessages)
            Debug.Log($"[ColliderManager] {message}");
    }

    private void DebugLogWarning(string message)
    {
        if (showDebugMessages)
            Debug.LogWarning($"[ColliderManager] {message}");
    }

    // Public methods for external access
    public int GetWallsWithColliders() => wallsWithColliders;
    public int GetWallsWithoutColliders() => wallsWithoutColliders;
    public int GetDoorsWithColliders() => doorsWithColliders;
    public int GetDoorsWithoutColliders() => doorsWithoutColliders;
    public bool IsWallBMeshFound() => wallBMeshFound;

    // Unity Inspector buttons (only visible in inspector)
    [System.Serializable]
    public class InspectorButtons
    {
        [Space]
        [Header("Runtime Controls")]
        public bool validateColliders;
        public bool refreshColliders;
        public bool removeAllColliders;
        public bool updateStatistics;
    }

    public InspectorButtons inspectorControls;

    void OnValidate()
    {
        // This runs when inspector values change
        if (inspectorControls != null)
        {
            if (inspectorControls.validateColliders)
            {
                inspectorControls.validateColliders = false;
                if (Application.isPlaying)
                    ValidateAndAddColliders();
            }

            if (inspectorControls.refreshColliders)
            {
                inspectorControls.refreshColliders = false;
                if (Application.isPlaying)
                    RefreshAllColliders();
            }

            if (inspectorControls.removeAllColliders)
            {
                inspectorControls.removeAllColliders = false;
                if (Application.isPlaying)
                    RemoveAllColliders();
            }

            if (inspectorControls.updateStatistics)
            {
                inspectorControls.updateStatistics = false;
                UpdateStatistics();
            }
        }
    }
}
