using UnityEngine;
using UnityEditor;

public class MeshColliderSetter
{
    [MenuItem("Tools/Set Mesh Colliders on Walls and Doors")]
    public static void SetMeshCollidersOnWallsAndDoors()
    {
        int countWalls = 0;
        int countDoors = 0;

        // Versuche, das GameObject mit dem Namen "Wall_B" zu finden, um dessen Mesh zu verwenden
        GameObject wallBObject = GameObject.Find("Wall_B");
        Mesh wallBMesh = null;
        if (wallBObject != null)
        {
            MeshFilter mf = wallBObject.GetComponent<MeshFilter>();
            if (mf != null)
            {
                wallBMesh = mf.sharedMesh;
            }
            else
            {
                Debug.LogWarning("Wall_B wurde gefunden, aber es existiert keine MeshFilter-Komponente an diesem Objekt!");
            }
        }
        else
        {
            Debug.LogWarning("Kein Objekt mit dem Namen 'Wall_B' gefunden!");
        }

        // Iteriere über alle GameObjects in der Szene
        foreach (GameObject obj in Object.FindObjectsOfType<GameObject>())
        {
            // Falls der Name mit "Wall_A" oder "Wall_C" beginnt, einen MeshCollider hinzufügen (falls noch nicht vorhanden)
            if (obj.name.StartsWith("Wall_A") || obj.name.StartsWith("Wall_C"))
            {
                if (obj.GetComponent<MeshCollider>() == null)
                {
                    obj.AddComponent<MeshCollider>();
                    countWalls++;
                }
            }
            // Falls der Name "Door" enthält, einen MeshCollider hinzufügen und dessen sharedMesh auf das aus Wall_B setzen
            else if (obj.name.Contains("Door"))
            {
                MeshCollider doorCollider = obj.GetComponent<MeshCollider>();
                if (doorCollider == null)
                {
                    doorCollider = obj.AddComponent<MeshCollider>();
                }
                if (wallBMesh != null)
                {
                    doorCollider.sharedMesh = wallBMesh;
                }
                else
                {
                    Debug.LogWarning($"Für {obj.name} konnte das Mesh nicht gesetzt werden, da Wall_B kein Mesh besitzt.");
                }
                countDoors++;
            }
        }
        Debug.Log($"✔️ MeshColliders hinzugefügt: {countWalls} Wall(s) und {countDoors} Door(s) bearbeitet.");
    }
}