using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class Task3Inspector
{
    [MenuItem("Archery/Inspect Targets and NPCs")]
    public static void Inspect()
    {
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/ArcheryGame.unity");

        Debug.Log("=== TASK 3 INSPECTION START ===");

        // 1. Inspect Archery Targets
        GameObject archeryParent = GameObject.Find("ARECHERY");
        if (archeryParent != null)
        {
            Debug.Log($"Found ARECHERY root with {archeryParent.transform.childCount} children.");
            for (int i = 0; i < archeryParent.transform.childCount; i++)
            {
                Transform child = archeryParent.transform.GetChild(i);
                MeshFilter[] mfs = child.GetComponentsInChildren<MeshFilter>(true);
                Collider[] cols = child.GetComponentsInChildren<Collider>(true);
                Debug.Log($"Target [{i}] '{child.name}': Pos={child.position}, MeshFilters={mfs.Length}, Colliders={cols.Length}");
                foreach (var mf in mfs)
                {
                    if (mf.sharedMesh != null)
                    {
                        Debug.Log($"   Mesh: {mf.name} | bounds={mf.sharedMesh.bounds.size}, center={mf.sharedMesh.bounds.center}");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("ARECHERY root not found!");
        }

        // 2. Inspect NPCs
        SimplePatrol[] patrols = Object.FindObjectsByType<SimplePatrol>(FindObjectsSortMode.None);
        Debug.Log($"Found {patrols.Length} SimplePatrol NPCs:");
        foreach (var p in patrols)
        {
            Collider col = p.GetComponent<Collider>();
            Collider[] allCols = p.GetComponentsInChildren<Collider>(true);
            Renderer rend = p.GetComponentInChildren<Renderer>();
            Bounds b = rend != null ? rend.bounds : new Bounds();
            Debug.Log($"NPC '{p.name}': Pos={p.transform.position}, RootCol={col != null}, ChildCols={allCols.Length}, Bounds={b.size}");
        }

        Debug.Log("=== TASK 3 INSPECTION END ===");
    }
}
