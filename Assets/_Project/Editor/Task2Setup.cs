using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class Task2Setup
{
    [MenuItem("ArcheryVR/Run Task 2 Setup")]
    public static void RunSetup()
    {
        Debug.Log("[Task2Setup] Starting Task 2 Migration...");

        // 1. Open Scene
        string scenePath = "Assets/_Project/Scenes/ArcheryGame.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 2. Setup Arrow.prefab
        string arrowPrefabPath = "Assets/ExternalAssets/Cartoon_Weapon_Pack/Prefab/Arrow.prefab";
        GameObject arrowPrefabObj = PrefabUtility.LoadPrefabContents(arrowPrefabPath);
        if (arrowPrefabObj != null)
        {
            Arrow arrowComp = arrowPrefabObj.GetComponent<Arrow>();
            if (arrowComp == null)
            {
                arrowComp = arrowPrefabObj.AddComponent<Arrow>();
                Debug.Log("[Task2Setup] Added Arrow.cs component to Arrow.prefab");
            }
            arrowComp.rotationOffset = new Vector3(0f, 90f, 0f);
            arrowComp.minSpeedToRotate = 0.1f;
            arrowComp.stickDepth = 0.08f;

            Rigidbody rb = arrowPrefabObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }

            PrefabUtility.SaveAsPrefabAsset(arrowPrefabObj, arrowPrefabPath);
            PrefabUtility.UnloadPrefabContents(arrowPrefabObj);
            Debug.Log("[Task2Setup] Arrow.prefab updated and saved.");
        }

        // 3. Find the Active Bow in the scene
        // It's Bow_01_EmissionWhite which has child BowString with LineRenderer
        GameObject activeBow = null;
        BowString[] bowStrings = Object.FindObjectsByType<BowString>(FindObjectsSortMode.None);
        foreach (var bs in bowStrings)
        {
            if (bs.transform.parent != null && bs.transform.parent.name.Contains("Bow_01"))
            {
                activeBow = bs.transform.parent.gameObject;
                break;
            }
        }

        if (activeBow == null)
        {
            // Fallback search
            GameObject bObj = GameObject.Find("Bow_01_EmissionWhite");
            if (bObj != null) activeBow = bObj;
        }

        if (activeBow == null)
        {
            Debug.LogError("[Task2Setup] Could not find Bow_01_EmissionWhite in scene!");
            return;
        }

        Debug.Log($"[Task2Setup] Found active bow: {activeBow.name}");

        // Unpack prefab instance completely so we can reparent cleanly
        if (PrefabUtility.IsPartOfPrefabInstance(activeBow))
        {
            PrefabUtility.UnpackPrefabInstance(activeBow, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // 4. Create or Find PC_Player
        GameObject pcPlayer = GameObject.Find("PC_Player");
        if (pcPlayer == null)
        {
            pcPlayer = new GameObject("PC_Player");
            Undo.RegisterCreatedObjectUndo(pcPlayer, "Create PC_Player");
        }

        pcPlayer.tag = "Player";
        pcPlayer.transform.position = new Vector3(15.9f, 1.0f, 17.9f);
        pcPlayer.transform.rotation = Quaternion.identity;

        CharacterController charCtrl = pcPlayer.GetComponent<CharacterController>();
        if (charCtrl == null)
        {
            charCtrl = pcPlayer.AddComponent<CharacterController>();
        }
        charCtrl.height = 1.8f;
        charCtrl.radius = 0.35f;
        charCtrl.center = new Vector3(0f, 0.9f, 0f);
        charCtrl.minMoveDistance = 0.001f;

        PlayerMovement movement = pcPlayer.GetComponent<PlayerMovement>();
        if (movement == null)
        {
            movement = pcPlayer.AddComponent<PlayerMovement>();
        }

        // 5. Create or Find PlayerCamera
        Transform camTrans = pcPlayer.transform.Find("PlayerCamera");
        GameObject camObj;
        if (camTrans == null)
        {
            camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(pcPlayer.transform);
            Undo.RegisterCreatedObjectUndo(camObj, "Create PlayerCamera");
        }
        else
        {
            camObj = camTrans.gameObject;
        }

        camObj.tag = "MainCamera";
        camObj.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        camObj.transform.localRotation = Quaternion.identity;

        Camera cam = camObj.GetComponent<Camera>();
        if (cam == null) cam = camObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 1000f;
        cam.fieldOfView = 60f;

        AudioListener listener = camObj.GetComponent<AudioListener>();
        if (listener == null) listener = camObj.AddComponent<AudioListener>();

        PlayerCameraController camController = camObj.GetComponent<PlayerCameraController>();
        if (camController == null)
        {
            camController = camObj.AddComponent<PlayerCameraController>();
        }

        // 6. Parent the Bow to PlayerCamera
        activeBow.transform.SetParent(camObj.transform);
        // Position bow naturally in first person view:
        // Bow model height is along Y. Shoot direction is -X.
        // Rotating (0, -90, 0) makes shoot direction point +Z (camera forward).
        activeBow.transform.localPosition = new Vector3(-0.25f, -0.2f, 0.5f);
        activeBow.transform.localRotation = Quaternion.Euler(0f, -90f, 5f);
        activeBow.transform.localScale = Vector3.one;

        // 7. Configure Bow Components
        Transform bowStringTrans = activeBow.transform.Find("BowString");
        if (bowStringTrans != null)
        {
            // Configure BowShoot
            BowShoot bowShoot = bowStringTrans.GetComponent<BowShoot>();
            if (bowShoot == null)
            {
                bowShoot = bowStringTrans.gameObject.AddComponent<BowShoot>();
            }

            SerializedObject soBowShoot = new SerializedObject(bowShoot);
            soBowShoot.FindProperty("playerCamera").objectReferenceValue = cam;
            soBowShoot.FindProperty("stringTop").objectReferenceValue = activeBow.transform.Find("StringTop");
            soBowShoot.FindProperty("stringBottom").objectReferenceValue = activeBow.transform.Find("StringBottom");
            soBowShoot.FindProperty("bowCenter").objectReferenceValue = activeBow.transform.Find("GripPoint");

            Transform pullPoint = bowStringTrans.Find("PullPoint");
            soBowShoot.FindProperty("pullPoint").objectReferenceValue = pullPoint;
            if (pullPoint != null)
            {
                soBowShoot.FindProperty("arrowSpawnPoint").objectReferenceValue = pullPoint.Find("ArrowSpawmPoint");
            }

            GameObject loadedArrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(arrowPrefabPath);
            soBowShoot.FindProperty("arrowPrefab").objectReferenceValue = loadedArrowPrefab;
            soBowShoot.FindProperty("arrowRotationOffset").vector3Value = new Vector3(0f, 90f, 0f);
            soBowShoot.FindProperty("maxPullDistance").floatValue = 0.35f;
            soBowShoot.FindProperty("drawDuration").floatValue = 0.8f;
            soBowShoot.FindProperty("minShootSpeed").floatValue = 15f;
            soBowShoot.FindProperty("maxShootSpeed").floatValue = 40f;
            soBowShoot.FindProperty("minPullToShoot").floatValue = 0.1f;
            soBowShoot.ApplyModifiedProperties();

            // Configure BowString
            BowString bowStringComp = bowStringTrans.GetComponent<BowString>();
            if (bowStringComp != null)
            {
                SerializedObject soBowString = new SerializedObject(bowStringComp);
                soBowString.FindProperty("stringTop").objectReferenceValue = activeBow.transform.Find("StringTop");
                soBowString.FindProperty("pullPoint").objectReferenceValue = pullPoint;
                soBowString.FindProperty("stringBottom").objectReferenceValue = activeBow.transform.Find("StringBottom");
                soBowString.ApplyModifiedProperties();
            }

            Debug.Log("[Task2Setup] BowShoot and BowString configured.");
        }

        // 8. Delete VR GameObjects
        string[] vrObjectNames = new string[] {
            "XR Origin (VR)",
            "XR Interaction Manager",
            "XR Device Simulator",
            "PlankBow" // The old unused bow
        };

        foreach (string vrName in vrObjectNames)
        {
            GameObject vrObj = GameObject.Find(vrName);
            if (vrObj != null)
            {
                Debug.Log($"[Task2Setup] Deleting VR object: {vrName}");
                Undo.DestroyObjectImmediate(vrObj);
            }
        }

        // Scan all root objects and their hierarchies
        foreach (var rootGo in scene.GetRootGameObjects())
        {
            var allChildren = rootGo.GetComponentsInChildren<Transform>(true);
            foreach (var t in allChildren)
            {
                if (t.name.ToLower().Contains("arrow"))
                {
                    Debug.Log($"[Task2Setup] Found arrow object in scene: '{t.name}', parent: {(t.parent != null ? t.parent.name : "null")}");
                    if (t.parent == null || t.parent.name == "ARECHERY" || t.parent.name == "ENVIRONMENT ")
                    {
                        Debug.Log($"[Task2Setup] Destroying arrow object: {t.name}");
                        Undo.DestroyObjectImmediate(t.gameObject);
                    }
                }
            }
            if (rootGo.name.ToLower().Contains("arrow"))
            {
                Debug.Log($"[Task2Setup] Destroying root arrow: {rootGo.name}");
                Undo.DestroyObjectImmediate(rootGo);
            }
        }

        // 9. Update Build Settings
        EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = newScenes;
        Debug.Log("[Task2Setup] Build Settings updated to ArcheryGame.unity as scene 0.");

        // 10. Save Scene & Assets
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[Task2Setup] TASK 2 SETUP COMPLETED SUCCESSFULLY!");
    }
}
