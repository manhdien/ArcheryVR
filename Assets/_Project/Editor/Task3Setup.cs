using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Task3Setup
{
    [MenuItem("Archery/Setup Task 3 (Scoring, Targets, NPCs, Ammo)")]
    public static void RunSetup()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/ArcheryGame.unity");
        Debug.Log("[Task3Setup] Starting Task 3 Setup on scene: " + scene.name);

        // 1. Setup ScoreManager on GAME SYSTEM
        GameObject gameSystem = GameObject.Find("GAME SYSTEM");
        if (gameSystem == null)
        {
            gameSystem = new GameObject("GAME SYSTEM");
            Undo.RegisterCreatedObjectUndo(gameSystem, "Create GAME SYSTEM");
        }
        ScoreManager sm = gameSystem.GetComponent<ScoreManager>();
        if (sm == null)
        {
            sm = gameSystem.AddComponent<ScoreManager>();
            Debug.Log("[Task3Setup] Attached ScoreManager to GAME SYSTEM.");
        }

        // 2. Setup 7 Archery Targets under ARECHERY
        GameObject archeryRoot = GameObject.Find("ARECHERY");
        if (archeryRoot != null)
        {
            int targetCount = archeryRoot.transform.childCount;
            Debug.Log($"[Task3Setup] Configuring {targetCount} Targets under ARECHERY...");

            for (int i = 0; i < targetCount; i++)
            {
                Transform targetRoot = archeryRoot.transform.GetChild(i);
                ConfigureArcheryTarget(targetRoot, i);
            }
        }
        else
        {
            Debug.LogError("[Task3Setup] ARECHERY root not found!");
        }

        // 3. Setup NPCs under NPC root
        GameObject npcRoot = GameObject.Find("NPC");
        if (npcRoot != null)
        {
            Debug.Log($"[Task3Setup] Configuring NPCs under NPC root ({npcRoot.transform.childCount} children)...");
            for (int i = 0; i < npcRoot.transform.childCount; i++)
            {
                Transform npcTransform = npcRoot.transform.GetChild(i);
                ConfigureNPC(npcTransform);
            }
        }
        else
        {
            Debug.LogError("[Task3Setup] NPC root not found!");
        }

        // 4. Setup BowShoot Ammo on PC_Player in scene
        GameObject player = GameObject.Find("PC_Player");
        if (player != null)
        {
            BowShoot bowShoot = player.GetComponentInChildren<BowShoot>();
            if (bowShoot != null)
            {
                SerializedObject so = new SerializedObject(bowShoot);
                SerializedProperty maxAmmoProp = so.FindProperty("maxAmmo");
                SerializedProperty currentAmmoProp = so.FindProperty("currentAmmo");
                if (maxAmmoProp != null) maxAmmoProp.intValue = 20;
                if (currentAmmoProp != null) currentAmmoProp.intValue = 20;
                so.ApplyModifiedProperties();
                Debug.Log("[Task3Setup] Configured ammo (20/20) on PC_Player BowShoot.");
            }
        }

        // 5. Update PC_Player Prefab
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/PC_Player.prefab");
        if (playerPrefab != null)
        {
            BowShoot prefabBowShoot = playerPrefab.GetComponentInChildren<BowShoot>();
            if (prefabBowShoot != null)
            {
                SerializedObject so = new SerializedObject(prefabBowShoot);
                SerializedProperty maxAmmoProp = so.FindProperty("maxAmmo");
                SerializedProperty currentAmmoProp = so.FindProperty("currentAmmo");
                if (maxAmmoProp != null) maxAmmoProp.intValue = 20;
                if (currentAmmoProp != null) currentAmmoProp.intValue = 20;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(playerPrefab);
                Debug.Log("[Task3Setup] Updated PC_Player prefab ammo properties.");
            }
        }

        // 6. Save scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[Task3Setup] Task 3 Setup completed and scene saved successfully!");
    }

    private static void ConfigureArcheryTarget(Transform targetRoot, int index)
    {
        Transform targetFace = targetRoot.Find("Target");
        Transform stand = targetRoot.Find("Stand");

        if (targetFace != null)
        {
            // Attach Target.cs
            Target targetScript = targetFace.GetComponent<Target>();
            if (targetScript == null)
            {
                targetScript = targetFace.gameObject.AddComponent<Target>();
            }

            // Task 6: Flat scoring — hitScore = 10 (no more zone-based scoring)
            targetScript.hitScore = 10;
            targetScript.targetCenter = targetFace;

            // Ensure MeshCollider on Target face
            MeshCollider mc = targetFace.GetComponent<MeshCollider>();
            if (mc == null)
            {
                mc = targetFace.gameObject.AddComponent<MeshCollider>();
            }
            MeshFilter mf = targetFace.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                mc.sharedMesh = mf.sharedMesh;
            }
        }

        // (Zone children Bullseye/MiddleZone/OuterZone kept in hierarchy for visual reference only)


        if (stand != null)
        {
            // Add BoxCollider for Stand legs (wood, 0 score)
            BoxCollider standCol = stand.GetComponent<BoxCollider>();
            if (standCol == null)
            {
                standCol = stand.gameObject.AddComponent<BoxCollider>();
            }
            standCol.size = new Vector3(0.94f, 1.31f, 1.26f);
            standCol.center = new Vector3(0.00f, 0.18f, 0.14f);
        }

        Debug.Log($"[Task3Setup] Target [{index}] '{targetRoot.name}': Configured Target script, MeshCollider, Zones, and Stand Collider.");
    }

    private static void ConfigureNPC(Transform npcTransform)
    {
        // Add CapsuleCollider to NPC root
        CapsuleCollider col = npcTransform.GetComponent<CapsuleCollider>();
        if (col == null)
        {
            col = npcTransform.gameObject.AddComponent<CapsuleCollider>();
        }
        col.height = 2.0f;
        col.radius = 0.45f;
        col.center = new Vector3(0f, 1.0f, 0f);

        // Add NPCTarget component
        NPCTarget npcTarget = npcTransform.GetComponent<NPCTarget>();
        if (npcTarget == null)
        {
            npcTarget = npcTransform.gameObject.AddComponent<NPCTarget>();
        }
        npcTarget.NpcScore = 75;

        // If it's a SimplePatrol NPC, make sure Rigidbody is kinematic if present, or no interference
        SimplePatrol patrol = npcTransform.GetComponent<SimplePatrol>();
        Debug.Log($"[Task3Setup] NPC '{npcTransform.name}' (Patrol={patrol != null}): Configured CapsuleCollider and NPCTarget (score=75).");
    }
}
