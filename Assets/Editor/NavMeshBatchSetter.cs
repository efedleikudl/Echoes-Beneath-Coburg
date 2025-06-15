using UnityEditor;
using UnityEngine;

public class NavMeshBatchSetter : MonoBehaviour
{
    [MenuItem("Tools/Set Navigation Static On Tiles")]
    static void SetTilesStatic()
    {
        int count = 0;
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj.name.StartsWith("Tile_B") || obj.name.StartsWith("Step_A"))
            {
                GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.NavigationStatic);
                count++;
            }
        }

        Debug.Log($"✔️ Set {count} objects to Navigation Static.");
    }
}
